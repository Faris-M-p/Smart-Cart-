using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

public class StockRepository : IStockRepository
{
    private readonly SmartCartDbContext _db;

    // Movement types where stock always goes OUT (must not go negative by default).
    // "adjustment" uses the sign of Quantity (positive = add, negative = remove).
    private static readonly HashSet<string> OutboundTypes = new(StringComparer.OrdinalIgnoreCase)
        { "sale", "transfer_out", "return_out" };

    // Must match the DB CHECK on stock_movements.movementtype.
    private static readonly HashSet<string> MovementTypes = new(StringComparer.OrdinalIgnoreCase)
        { "purchase", "sale", "adjustment", "transfer_in", "transfer_out", "return_in", "return_out", "opening" };

    private const string AllowNegativeStockKey = "pos.allow_negative_stock";

    public StockRepository(SmartCartDbContext db) => _db = db;

    // ═══════════════════════════════════════════════════════════════
    // WRITE PATH
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<OutputStockMovement> ApplyMovementAsync(InputStockMovement input, CancellationToken ct = default)
    {
        // Sale, purchase and online-order flows already run inside their own transaction: join it
        // (EF Core does not allow a second BeginTransaction on the same context). Otherwise own one.
        var ownsTransaction = _db.Database.CurrentTransaction is null;
        await using var tx = ownsTransaction ? await _db.Database.BeginTransactionAsync(ct) : null;

        try
        {
            var result = await ApplyMovementCoreAsync(input, ct);

            if (ownsTransaction)
            {
                if (result.Ok) await tx!.CommitAsync(ct);
                else await tx!.RollbackAsync(ct);
            }

            return result;
        }
        catch
        {
            if (ownsTransaction) await tx!.RollbackAsync(ct);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<OutputStockTransfer> TransferAsync(InputStockTransfer input, CancellationToken ct = default)
    {
        var create = new InputStockTransferCreate
        {
            FromStoreId = input.FromStoreId,
            ToStoreId = input.ToStoreId,
            Notes = input.Notes,
            CreatedBy = input.CreatedBy,
            Lines = new List<InputStockTransferLine>
            {
                new InputStockTransferLine
                {
                    FkProduct = input.FkProduct,
                    FkProductVariant = input.FkProductVariant,
                    Quantity = input.Quantity
                }
            }
        };

        var result = await CreateTransferAsync(create, ct);

        return new OutputStockTransfer
        {
            ResponseCode = result.ResponseCode,
            ResponseMsg = result.ResponseMsg,
            StatusCode = result.StatusCode
        };
    }

    /// <inheritdoc/>
    public async Task<OutputStockTransferCreate> CreateTransferAsync(InputStockTransferCreate input, CancellationToken ct = default)
    {
        if (input.FromStoreId == input.ToStoreId)
            return TransferRejected(1, 422, "From and To stores must differ.");

        if (input.Lines.Count == 0)
            return TransferRejected(1, 422, "Add at least one product to transfer.");

        var stores = await _db.Stores.AsNoTracking()
            .Where(s => (s.IdStore == input.FromStoreId || s.IdStore == input.ToStoreId) && !s.Cancelled)
            .Select(s => new { s.IdStore, s.FkCompany, s.StoreName, s.IsActive })
            .ToListAsync(ct);

        var source = stores.FirstOrDefault(s => s.IdStore == input.FromStoreId);
        var destination = stores.FirstOrDefault(s => s.IdStore == input.ToStoreId);

        if (source is null)
            return TransferRejected(1, 404, "The source store was not found.");
        if (destination is null || !destination.IsActive)
            return TransferRejected(1, 422, "The destination store was not found or is inactive.");
        if (source.FkCompany != destination.FkCompany)
            return TransferRejected(1, 422, "Stock can only be transferred between stores of the same company.");

        var noteSuffix = string.IsNullOrWhiteSpace(input.Notes) ? string.Empty : " — " + input.Notes.Trim();

        // Out + in for every line commit together or not at all.
        var ownsTransaction = _db.Database.CurrentTransaction is null;
        await using var tx = ownsTransaction ? await _db.Database.BeginTransactionAsync(ct) : null;

        try
        {
            foreach (var line in input.Lines)
            {
                var outResult = await ApplyMovementCoreAsync(new InputStockMovement
                {
                    FkStore = source.IdStore,
                    FkProduct = line.FkProduct,
                    FkProductVariant = line.FkProductVariant,
                    MovementType = "transfer_out",
                    Quantity = line.Quantity,
                    ReferenceType = "transfer",
                    Notes = $"Transfer to {destination.StoreName}{noteSuffix}",
                    CreatedBy = input.CreatedBy
                }, ct);

                if (!outResult.Ok)
                {
                    if (ownsTransaction) await tx!.RollbackAsync(ct);
                    var label = await DescribeSkuAsync(line.FkProduct, line.FkProductVariant, ct);
                    return TransferRejected(outResult.ResponseCode, outResult.StatusCode, $"{label}: {outResult.ResponseMsg}");
                }

                var inResult = await ApplyMovementCoreAsync(new InputStockMovement
                {
                    FkStore = destination.IdStore,
                    FkProduct = line.FkProduct,
                    FkProductVariant = line.FkProductVariant,
                    MovementType = "transfer_in",
                    Quantity = line.Quantity,
                    ReferenceType = "transfer",
                    ReferenceId = outResult.IdStockMovement,
                    Notes = $"Transfer from {source.StoreName}{noteSuffix}",
                    CreatedBy = input.CreatedBy
                }, ct);

                if (!inResult.Ok)
                {
                    if (ownsTransaction) await tx!.RollbackAsync(ct);
                    var label = await DescribeSkuAsync(line.FkProduct, line.FkProductVariant, ct);
                    return TransferRejected(inResult.ResponseCode, inResult.StatusCode, $"{label}: {inResult.ResponseMsg}");
                }
            }

            if (ownsTransaction) await tx!.CommitAsync(ct);

            return new OutputStockTransferCreate { LineCount = input.Lines.Count };
        }
        catch
        {
            if (ownsTransaction) await tx!.RollbackAsync(ct);
            throw;
        }
    }

    /// <summary>
    /// Core of every stock change. Runs inside the caller's transaction (see <see cref="ApplyMovementAsync"/>).
    /// The balance is changed with one guarded UPDATE so two concurrent sales cannot both pass the stock check.
    /// </summary>
    private async Task<OutputStockMovement> ApplyMovementCoreAsync(InputStockMovement input, CancellationToken ct)
    {
        var type = (input.MovementType ?? string.Empty).Trim().ToLowerInvariant();
        if (!MovementTypes.Contains(type))
            return Rejected(1, 422, "Unknown stock movement type.");

        if (input.FkStore <= 0 || input.FkProduct <= 0)
            return Rejected(1, 422, "A store and a product are required for a stock movement.");

        var quantity = Math.Round(input.Quantity, 3, MidpointRounding.AwayFromZero);
        if (quantity == 0)
            return Rejected(1, 422, "The quantity must not be zero.");

        // adjustment: signed quantity (+ add, − remove). Every other type: magnitude only, direction comes from the type.
        var delta = type == "adjustment"
            ? quantity
            : (OutboundTypes.Contains(type) ? -Math.Abs(quantity) : Math.Abs(quantity));

        if (input.FkProductVariant.HasValue)
        {
            var variantId = input.FkProductVariant.Value;
            var variantMatches = await _db.ProductVariants.AsNoTracking()
                .AnyAsync(v => v.IdProductVariant == variantId && v.FkProduct == input.FkProduct, ct);
            if (!variantMatches)
                return Rejected(1, 422, "The selected variant does not belong to this product.");
        }

        // Only a sale may go below zero, and only when the store/company setting allows it.
        var allowNegative = type == "sale" && await IsNegativeStockAllowedAsync(input.FkStore, ct);

        var inventoryId = await InventoryRows(input.FkStore, input.FkProduct, input.FkProductVariant)
            .Select(x => (int?)x.IdInventory)
            .FirstOrDefaultAsync(ct);

        var now = DateTime.UtcNow;
        Inventory? newRow = null;
        StockMovement? movement = null;
        decimal newQuantity;

        try
        {
            if (inventoryId is null)
            {
                newQuantity = delta;
                if (newQuantity < 0 && !allowNegative)
                    return Insufficient(0m, quantity);

                newRow = new Inventory
                {
                    FkStore = input.FkStore,
                    FkProduct = input.FkProduct,
                    FkProductVariant = input.FkProductVariant,
                    QuantityOnHand = newQuantity,
                    QuantityReserved = 0,
                    ReorderLevel = 0,
                    CreatedAt = now
                };
                _db.Inventories.Add(newRow);
            }
            else
            {
                var id = inventoryId.Value;

                var updated = await _db.Inventories
                    .Where(x => x.IdInventory == id && (allowNegative || x.QuantityOnHand + delta >= 0))
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.QuantityOnHand, x => x.QuantityOnHand + delta)
                        .SetProperty(x => x.UpdatedAt, (DateTime?)now), ct);

                if (updated == 0)
                {
                    var onHand = await _db.Inventories.AsNoTracking()
                        .Where(x => x.IdInventory == id)
                        .Select(x => x.QuantityOnHand)
                        .FirstOrDefaultAsync(ct);
                    return Insufficient(onHand, quantity);
                }

                newQuantity = await _db.Inventories.AsNoTracking()
                    .Where(x => x.IdInventory == id)
                    .Select(x => x.QuantityOnHand)
                    .FirstAsync(ct);
            }

            movement = new StockMovement
            {
                FkStore = input.FkStore,
                FkProduct = input.FkProduct,
                FkProductVariant = input.FkProductVariant,
                MovementType = type,
                Quantity = quantity,
                ReferenceType = input.ReferenceType,
                ReferenceId = input.ReferenceId,
                Notes = input.Notes,
                CreatedBy = input.CreatedBy,
                CreatedAt = now
            };
            _db.StockMovements.Add(movement);

            await _db.SaveChangesAsync(ct);

            return new OutputStockMovement
            {
                IdStockMovement = movement.IdStockMovement,
                NewQuantityOnHand = newQuantity
            };
        }
        catch (Exception ex) when (allowNegative && delta < 0 && IsCheckViolation(ex))
        {
            // The setting allows negative sales but the table CHECK (quantityonhand >= 0) still blocks them.
            if (newRow is not null) _db.Entry(newRow).State = EntityState.Detached;
            if (movement is not null) _db.Entry(movement).State = EntityState.Detached;

            return Rejected(2, 422,
                "Insufficient stock: negative stock is enabled in settings, but the database still blocks negative balances. Ask your administrator to apply the stock patch.");
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // READS: BALANCES
    // ═══════════════════════════════════════════════════════════════

    public async Task<OutputInventorySelect?> GetInventoryAsync(InputInventorySelect input, CancellationToken ct = default)
        => await Project(InventoryRows(input.FkStore, input.FkProduct, input.FkProductVariant)).FirstOrDefaultAsync(ct);

    /// <inheritdoc/>
    public async Task<OutputInventorySelect?> GetStockLevelAsync(InputInventorySelect input, CancellationToken ct = default)
    {
        var product = await _db.Products.AsNoTracking()
            .Where(p => p.IdProduct == input.FkProduct && !p.Cancelled)
            .Select(p => new
            {
                p.IdProduct,
                p.ProductName,
                p.ProductCode,
                p.ReorderLevel,
                CategoryName = p.Category != null ? p.Category.CategoryName : null,
                UnitCode = _db.Units.Where(u => u.IdUnit == p.FkUnit).Select(u => u.UnitCode).FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);

        if (product is null) return null;

        string? variantName = null;
        if (input.FkProductVariant.HasValue)
        {
            var variantId = input.FkProductVariant.Value;
            variantName = await _db.ProductVariants.AsNoTracking()
                .Where(v => v.IdProductVariant == variantId && v.FkProduct == input.FkProduct && !v.Cancelled)
                .Select(v => v.VariantName)
                .FirstOrDefaultAsync(ct);

            if (variantName is null) return null;
        }

        var row = await Project(InventoryRows(input.FkStore, input.FkProduct, input.FkProductVariant)).FirstOrDefaultAsync(ct);
        if (row is not null) return row;

        // No inventory row yet: report the SKU with zero on hand so opening stock / transfers can start from it.
        return new OutputInventorySelect
        {
            IdInventory = 0,
            FkStore = input.FkStore,
            FkProduct = product.IdProduct,
            ProductName = product.ProductName,
            ProductCode = product.ProductCode,
            FkProductVariant = input.FkProductVariant,
            VariantName = variantName,
            CategoryName = product.CategoryName,
            UnitCode = product.UnitCode,
            QuantityOnHand = 0,
            QuantityReserved = 0,
            ReorderLevel = product.ReorderLevel
        };
    }

    /// <inheritdoc/>
    public async Task<bool> HasOpeningStockAsync(InputInventorySelect input, CancellationToken ct = default)
    {
        var q = _db.StockMovements.AsNoTracking()
            .Where(m => m.FkStore == input.FkStore && m.FkProduct == input.FkProduct && m.MovementType == "opening");

        q = input.FkProductVariant.HasValue
            ? q.Where(m => m.FkProductVariant == input.FkProductVariant.Value)
            : q.Where(m => m.FkProductVariant == null);

        return await q.AnyAsync(ct);
    }

    public async Task<OutputInventoryList> ListInventoryAsync(InputInventoryList input, CancellationToken ct = default)
    {
        var page = Math.Max(1, input.Page);
        var pageSize = Math.Clamp(input.PageSize <= 0 ? 25 : input.PageSize, 1, 200);
        var status = NormalizeStatus(input.StockStatus);
        var sortBy = (input.SortBy ?? string.Empty).Trim().ToLowerInvariant();

        var scope = Project(FilteredInventory(input.FkStore, input.FkCategory, input.Search));

        // Chip counts for the current search/category scope (independent of the status filter).
        var scopeCount = await scope.CountAsync(ct);
        var lowCount = await scope.CountAsync(r => r.QuantityOnHand > 0 && r.QuantityOnHand <= r.ReorderLevel, ct);
        var outCount = await scope.CountAsync(r => r.QuantityOnHand <= 0, ct);

        var q = scope;
        int total;
        if (status == "low")
        {
            q = q.Where(r => r.QuantityOnHand > 0 && r.QuantityOnHand <= r.ReorderLevel);
            total = lowCount;
        }
        else if (status == "out")
        {
            q = q.Where(r => r.QuantityOnHand <= 0);
            total = outCount;
        }
        else if (input.BelowReorderOnly)
        {
            q = q.Where(r => r.QuantityOnHand <= r.ReorderLevel);
            total = lowCount + outCount;
        }
        else
        {
            total = scopeCount;
        }

        q = sortBy switch
        {
            "productcode" => input.SortDesc
                ? q.OrderByDescending(r => r.ProductCode).ThenBy(r => r.ProductName)
                : q.OrderBy(r => r.ProductCode).ThenBy(r => r.ProductName),
            "quantityonhand" => input.SortDesc
                ? q.OrderByDescending(r => r.QuantityOnHand).ThenBy(r => r.ProductName)
                : q.OrderBy(r => r.QuantityOnHand).ThenBy(r => r.ProductName),
            "reorderlevel" => input.SortDesc
                ? q.OrderByDescending(r => r.ReorderLevel).ThenBy(r => r.ProductName)
                : q.OrderBy(r => r.ReorderLevel).ThenBy(r => r.ProductName),
            _ => input.SortDesc
                ? q.OrderByDescending(r => r.ProductName).ThenBy(r => r.VariantName)
                : q.OrderBy(r => r.ProductName).ThenBy(r => r.VariantName)
        };

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new OutputInventoryList
        {
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            Items = items,
            ScopeCount = scopeCount,
            LowStockCount = lowCount,
            OutOfStockCount = outCount,
            Filter = new InputInventoryList
            {
                FkStore = input.FkStore,
                Search = string.IsNullOrWhiteSpace(input.Search) ? null : input.Search.Trim(),
                FkCategory = input.FkCategory,
                BelowReorderOnly = input.BelowReorderOnly,
                StockStatus = status,
                SortBy = (sortBy is "productcode" or "quantityonhand" or "reorderlevel") ? sortBy : "productname",
                SortDesc = input.SortDesc,
                Page = page,
                PageSize = pageSize
            }
        };
    }

    public async Task<OutputLowStockList> GetLowStockAsync(InputLowStockList input, CancellationToken ct = default)
    {
        var page = Math.Max(1, input.Page);
        var pageSize = Math.Clamp(input.PageSize <= 0 ? 25 : input.PageSize, 1, 200);
        var sortBy = (input.SortBy ?? string.Empty).Trim().ToLowerInvariant();

        var q = Project(FilteredInventory(input.FkStore, input.FkCategory, input.Search))
            .Where(r => r.QuantityOnHand <= r.ReorderLevel);

        var total = await q.CountAsync(ct);

        q = sortBy switch
        {
            "productname" => input.SortDesc
                ? q.OrderByDescending(r => r.ProductName).ThenBy(r => r.VariantName)
                : q.OrderBy(r => r.ProductName).ThenBy(r => r.VariantName),
            "reorderlevel" => input.SortDesc
                ? q.OrderByDescending(r => r.ReorderLevel).ThenBy(r => r.ProductName)
                : q.OrderBy(r => r.ReorderLevel).ThenBy(r => r.ProductName),
            "shortage" => input.SortDesc
                ? q.OrderByDescending(r => r.ReorderLevel - r.QuantityOnHand).ThenBy(r => r.ProductName)
                : q.OrderBy(r => r.ReorderLevel - r.QuantityOnHand).ThenBy(r => r.ProductName),
            _ => input.SortDesc
                ? q.OrderByDescending(r => r.QuantityOnHand).ThenBy(r => r.ProductName)
                : q.OrderBy(r => r.QuantityOnHand).ThenBy(r => r.ProductName)
        };

        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new OutputLowStockList
        {
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            Items = items,
            Filter = new InputLowStockList
            {
                FkStore = input.FkStore,
                FkCategory = input.FkCategory,
                Search = string.IsNullOrWhiteSpace(input.Search) ? null : input.Search.Trim(),
                SortBy = (sortBy is "productname" or "reorderlevel" or "shortage") ? sortBy : "quantityonhand",
                SortDesc = input.SortDesc,
                Page = page,
                PageSize = pageSize
            }
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // READS: MOVEMENT HISTORY
    // ═══════════════════════════════════════════════════════════════

    public async Task<OutputStockMovementList> ListMovementsAsync(InputStockMovementList input, CancellationToken ct = default)
    {
        var page = Math.Max(1, input.Page);
        var pageSize = Math.Clamp(input.PageSize <= 0 ? 25 : input.PageSize, 1, 200);

        var q = _db.StockMovements.AsNoTracking().AsQueryable();

        if (input.FkStore is > 0) q = q.Where(x => x.FkStore == input.FkStore.Value);
        if (input.FkProduct is > 0) q = q.Where(x => x.FkProduct == input.FkProduct.Value);

        var movementType = string.IsNullOrWhiteSpace(input.MovementType) ? null : input.MovementType.Trim().ToLowerInvariant();
        if (movementType is not null) q = q.Where(x => x.MovementType == movementType);

        // The service prepares UTC bounds from the user's local days; plain From/To still work for other callers.
        var fromUtc = input.FromUtc ?? input.From;
        if (fromUtc.HasValue) q = q.Where(x => x.CreatedAt >= fromUtc.Value);

        if (input.ToUtcExclusive.HasValue)
            q = q.Where(x => x.CreatedAt < input.ToUtcExclusive.Value);
        else if (input.To.HasValue)
            q = q.Where(x => x.CreatedAt <= input.To.Value);

        var total = await q.CountAsync(ct);

        var items = await q
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.IdStockMovement)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new OutputStockMovementItem
            {
                IdStockMovement = x.IdStockMovement,
                FkStore = x.FkStore,
                StoreName = x.Store != null ? x.Store.StoreName : string.Empty,
                FkProduct = x.FkProduct,
                ProductName = x.Product != null ? x.Product.ProductName : string.Empty,
                ProductCode = x.Product != null ? x.Product.ProductCode : null,
                FkProductVariant = x.FkProductVariant,
                VariantName = _db.ProductVariants
                    .Where(v => v.IdProductVariant == x.FkProductVariant)
                    .Select(v => v.VariantName)
                    .FirstOrDefault(),
                MovementType = x.MovementType,
                Quantity = x.Quantity,
                ReferenceType = x.ReferenceType,
                ReferenceId = x.ReferenceId,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt,
                CreatedByName = _db.Users
                    .Where(u => u.IdUser == x.CreatedBy)
                    .Select(u => u.FullName)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        string? productName = null;
        if (input.FkProduct is > 0)
        {
            var productId = input.FkProduct.Value;
            productName = await _db.Products.AsNoTracking()
                .Where(p => p.IdProduct == productId)
                .Select(p => p.ProductName)
                .FirstOrDefaultAsync(ct);
        }

        return new OutputStockMovementList
        {
            TotalCount = total,
            Page = page,
            PageSize = pageSize,
            Items = items,
            FilterProductName = productName,
            Filter = new InputStockMovementList
            {
                FkStore = input.FkStore,
                FkProduct = (input.FkProduct is > 0) ? input.FkProduct : null,
                MovementType = movementType,
                From = input.From,
                To = input.To,
                TzOffsetMinutes = input.TzOffsetMinutes,
                Page = page,
                PageSize = pageSize
            }
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // READS: PICKERS AND LOOKUPS
    // ═══════════════════════════════════════════════════════════════

    /// <inheritdoc/>
    public async Task<OutputStockProductSearch> SearchProductsAsync(InputStockProductSearch input, CancellationToken ct = default)
    {
        var term = (input.Term ?? string.Empty).Trim();
        if (term.Length == 0) return new OutputStockProductSearch();

        var pattern = LikePattern(term);
        var limit = Math.Clamp(input.Limit <= 0 ? 15 : input.Limit, 1, 30);
        var storeId = input.FkStore;

        // One row per variant of products that have variants …
        var variantItems = await _db.ProductVariants.AsNoTracking()
            .Where(v => !v.Cancelled
                && !v.Product!.Cancelled
                && (EF.Functions.ILike(v.Product!.ProductName, pattern)
                    || EF.Functions.ILike(v.Product!.ProductCode, pattern)
                    || (v.Product!.Sku != null && EF.Functions.ILike(v.Product!.Sku!, pattern))
                    || (v.Product!.Barcode != null && EF.Functions.ILike(v.Product!.Barcode!, pattern))
                    || EF.Functions.ILike(v.VariantName, pattern)
                    || (v.SkuCode != null && EF.Functions.ILike(v.SkuCode!, pattern))
                    || (v.Barcode != null && EF.Functions.ILike(v.Barcode!, pattern))))
            .OrderBy(v => v.Product!.ProductName).ThenBy(v => v.VariantName)
            .Select(v => new OutputStockProductSearchItem
            {
                IdProduct = v.FkProduct,
                ProductCode = v.Product!.ProductCode,
                ProductName = v.Product!.ProductName,
                IdProductVariant = v.IdProductVariant,
                VariantName = v.VariantName,
                Sku = v.SkuCode ?? v.Product!.Sku,
                Barcode = v.Barcode ?? v.Product!.Barcode,
                UnitCode = _db.Units.Where(u => u.IdUnit == v.Product!.FkUnit).Select(u => u.UnitCode).FirstOrDefault(),
                QuantityOnHand = _db.Inventories
                    .Where(i => i.FkStore == storeId && i.FkProduct == v.FkProduct && i.FkProductVariant == v.IdProductVariant)
                    .Select(i => (decimal?)i.QuantityOnHand)
                    .FirstOrDefault() ?? 0m,
                IsActive = v.Product!.IsActive && v.IsActive
            })
            .Take(limit)
            .ToListAsync(ct);

        // … plus one row per product that has no variants at all.
        var productItems = await _db.Products.AsNoTracking()
            .Where(p => !p.Cancelled
                && !_db.ProductVariants.Any(v => v.FkProduct == p.IdProduct && !v.Cancelled)
                && (EF.Functions.ILike(p.ProductName, pattern)
                    || EF.Functions.ILike(p.ProductCode, pattern)
                    || (p.Sku != null && EF.Functions.ILike(p.Sku!, pattern))
                    || (p.Barcode != null && EF.Functions.ILike(p.Barcode!, pattern))))
            .OrderBy(p => p.ProductName)
            .Select(p => new OutputStockProductSearchItem
            {
                IdProduct = p.IdProduct,
                ProductCode = p.ProductCode,
                ProductName = p.ProductName,
                IdProductVariant = null,
                VariantName = null,
                Sku = p.Sku,
                Barcode = p.Barcode,
                UnitCode = _db.Units.Where(u => u.IdUnit == p.FkUnit).Select(u => u.UnitCode).FirstOrDefault(),
                QuantityOnHand = _db.Inventories
                    .Where(i => i.FkStore == storeId && i.FkProduct == p.IdProduct && i.FkProductVariant == null)
                    .Select(i => (decimal?)i.QuantityOnHand)
                    .FirstOrDefault() ?? 0m,
                IsActive = p.IsActive
            })
            .Take(limit)
            .ToListAsync(ct);

        var merged = variantItems
            .Concat(productItems)
            .OrderBy(x => x.ProductName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.VariantName, StringComparer.OrdinalIgnoreCase)
            .Take(limit)
            .ToList();

        return new OutputStockProductSearch { Items = merged };
    }

    /// <inheritdoc/>
    public async Task<List<OutputStockLookupItem>> ListCategoryOptionsAsync(CancellationToken ct = default)
    {
        return await _db.Categories.AsNoTracking()
            .Where(c => !c.Cancelled)
            .OrderBy(c => c.DisplayOrder).ThenBy(c => c.CategoryName)
            .Select(c => new OutputStockLookupItem
            {
                Id = c.IdCategory,
                Code = c.CategoryCode,
                Name = c.CategoryName
            })
            .ToListAsync(ct);
    }

    /// <inheritdoc/>
    public async Task<List<OutputStockLookupItem>> ListTransferStoresAsync(int fromStoreId, CancellationToken ct = default)
    {
        var companyId = await _db.Stores.AsNoTracking()
            .Where(s => s.IdStore == fromStoreId)
            .Select(s => (int?)s.FkCompany)
            .FirstOrDefaultAsync(ct);

        if (companyId is null) return new List<OutputStockLookupItem>();

        var company = companyId.Value;

        return await _db.Stores.AsNoTracking()
            .Where(s => s.FkCompany == company && s.IdStore != fromStoreId && s.IsActive && !s.Cancelled)
            .OrderBy(s => s.StoreName)
            .Select(s => new OutputStockLookupItem
            {
                Id = s.IdStore,
                Code = s.StoreCode,
                Name = s.StoreName
            })
            .ToListAsync(ct);
    }

    // ═══════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>Inventory rows for one SKU (variant or no variant) at one store.</summary>
    private IQueryable<Inventory> InventoryRows(int fkStore, int fkProduct, int? fkVariant)
    {
        var q = _db.Inventories.AsNoTracking()
            .Where(x => x.FkStore == fkStore && x.FkProduct == fkProduct);

        return fkVariant.HasValue
            ? q.Where(x => x.FkProductVariant == fkVariant.Value)
            : q.Where(x => x.FkProductVariant == null);
    }

    /// <summary>Store inventory rows narrowed by category and a case-insensitive search over product / variant fields.</summary>
    private IQueryable<Inventory> FilteredInventory(int fkStore, int? fkCategory, string? search)
    {
        var q = _db.Inventories.AsNoTracking().Where(i => i.FkStore == fkStore);

        if (fkCategory.HasValue)
            q = q.Where(i => i.Product!.FkCategory == fkCategory.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = LikePattern(search);
            q = q.Where(i =>
                EF.Functions.ILike(i.Product!.ProductName, pattern)
                || EF.Functions.ILike(i.Product!.ProductCode, pattern)
                || (i.Product!.Sku != null && EF.Functions.ILike(i.Product!.Sku!, pattern))
                || (i.Product!.Barcode != null && EF.Functions.ILike(i.Product!.Barcode!, pattern))
                || _db.ProductVariants.Any(v => v.IdProductVariant == i.FkProductVariant
                    && (EF.Functions.ILike(v.VariantName, pattern)
                        || (v.SkuCode != null && EF.Functions.ILike(v.SkuCode!, pattern))
                        || (v.Barcode != null && EF.Functions.ILike(v.Barcode!, pattern)))));
        }

        return q;
    }

    /// <summary>Inventory → output model (Entity → Output mapping lives only here).</summary>
    private IQueryable<OutputInventorySelect> Project(IQueryable<Inventory> rows)
        => rows.Select(i => new OutputInventorySelect
        {
            IdInventory = i.IdInventory,
            FkStore = i.FkStore,
            FkProduct = i.FkProduct,
            ProductName = i.Product!.ProductName,
            ProductCode = i.Product!.ProductCode,
            FkProductVariant = i.FkProductVariant,
            VariantName = _db.ProductVariants
                .Where(v => v.IdProductVariant == i.FkProductVariant)
                .Select(v => v.VariantName)
                .FirstOrDefault(),
            CategoryName = i.Product!.Category != null ? i.Product!.Category!.CategoryName : null,
            UnitCode = _db.Units
                .Where(u => u.IdUnit == i.Product!.FkUnit)
                .Select(u => u.UnitCode)
                .FirstOrDefault(),
            QuantityOnHand = i.QuantityOnHand,
            QuantityReserved = i.QuantityReserved,
            // A store row with its own level (> 0) wins; otherwise fall back to the product's level.
            ReorderLevel = i.ReorderLevel > 0 ? i.ReorderLevel : i.Product!.ReorderLevel
        });

    /// <summary>Reads <c>pos.allow_negative_stock</c>: store-level value wins over the company-level value. Missing = false.</summary>
    private async Task<bool> IsNegativeStockAllowedAsync(int fkStore, CancellationToken ct)
    {
        var raw = await _db.Settings.AsNoTracking()
            .Where(s => s.SettingKey == AllowNegativeStockKey
                && ((s.SettingScope == "store" && s.FkStore == fkStore)
                    || (s.SettingScope == "company" && s.FkStore == null)))
            .OrderBy(s => s.SettingScope == "store" ? 0 : 1)
            .Select(s => s.SettingValue)
            .FirstOrDefaultAsync(ct);

        return string.Equals(raw?.Trim(), "true", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Product name (and variant name) for error messages.</summary>
    private async Task<string> DescribeSkuAsync(int fkProduct, int? fkVariant, CancellationToken ct)
    {
        var name = await _db.Products.AsNoTracking()
            .Where(p => p.IdProduct == fkProduct)
            .Select(p => p.ProductName)
            .FirstOrDefaultAsync(ct);

        if (name is null) return "Product #" + fkProduct.ToString(CultureInfo.InvariantCulture);
        if (!fkVariant.HasValue) return name;

        var variantId = fkVariant.Value;
        var variant = await _db.ProductVariants.AsNoTracking()
            .Where(v => v.IdProductVariant == variantId)
            .Select(v => v.VariantName)
            .FirstOrDefaultAsync(ct);

        return variant is null ? name : name + " — " + variant;
    }

    private static string NormalizeStatus(string? status)
    {
        var value = (status ?? string.Empty).Trim().ToLowerInvariant();
        return (value is "low" or "out") ? value : string.Empty;
    }

    /// <summary>"%term%" for ILIKE with the LIKE wildcards inside the term escaped (PostgreSQL default escape is backslash).</summary>
    private static string LikePattern(string term)
        => "%" + term.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";

    private static bool IsCheckViolation(Exception ex)
        => ex is PostgresException { SqlState: PostgresErrorCodes.CheckViolation }
           || ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.CheckViolation };

    private static string FormatQty(decimal value) => value.ToString("0.###", CultureInfo.InvariantCulture);

    private static OutputStockMovement Rejected(int responseCode, int statusCode, string message)
        => new() { ResponseCode = responseCode, StatusCode = statusCode, ResponseMsg = message };

    private static OutputStockMovement Insufficient(decimal onHand, decimal requested)
        => new()
        {
            ResponseCode = 2,
            StatusCode = 422,
            ResponseMsg = $"Insufficient stock: {FormatQty(onHand)} on hand, {FormatQty(Math.Abs(requested))} requested.",
            NewQuantityOnHand = onHand
        };

    private static OutputStockTransferCreate TransferRejected(int responseCode, int statusCode, string message)
        => new() { ResponseCode = responseCode, StatusCode = statusCode, ResponseMsg = message };
}
