using Microsoft.EntityFrameworkCore;
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

    public StockRepository(SmartCartDbContext db) => _db = db;

    /// <inheritdoc/>
    public async Task<OutputStockMovement> ApplyMovementAsync(InputStockMovement input, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        try
        {
            // 1. Upsert inventory row (lock for update via EF tracking)
            Inventory? inv;
            if (input.FkProductVariant.HasValue)
                inv = await _db.Inventories.FirstOrDefaultAsync(
                    x => x.FkStore == input.FkStore && x.FkProduct == input.FkProduct && x.FkProductVariant == input.FkProductVariant, ct);
            else
                inv = await _db.Inventories.FirstOrDefaultAsync(
                    x => x.FkStore == input.FkStore && x.FkProduct == input.FkProduct && x.FkProductVariant == null, ct);

            if (inv is null)
            {
                inv = new Inventory
                {
                    FkStore = input.FkStore,
                    FkProduct = input.FkProduct,
                    FkProductVariant = input.FkProductVariant,
                    QuantityOnHand = 0,
                    QuantityReserved = 0,
                    ReorderLevel = 0,
                    CreatedAt = DateTime.UtcNow
                };
                _db.Inventories.Add(inv);
                await _db.SaveChangesAsync(ct); // flush to get id
            }

            // 2. Calculate new balance
            // For "adjustment": Quantity may be signed (positive=add, negative=remove).
            // For all other types: Quantity is always positive; direction determined by type.
            decimal delta;
            if (string.Equals(input.MovementType, "adjustment", StringComparison.OrdinalIgnoreCase))
            {
                // Signed quantity: +ve adds, -ve removes
                delta = input.Quantity;
            }
            else
            {
                delta = OutboundTypes.Contains(input.MovementType)
                    ? -Math.Abs(input.Quantity)
                    : Math.Abs(input.Quantity);
            }

            decimal newQty = inv.QuantityOnHand + delta;

            if (newQty < 0)
                return new OutputStockMovement
                {
                    ResponseCode = 2,
                    ResponseMsg = "Insufficient stock — negative balance not allowed.",
                    StatusCode = 422,
                    NewQuantityOnHand = inv.QuantityOnHand
                };

            inv.QuantityOnHand = newQty;
            inv.UpdatedAt = DateTime.UtcNow;

            // 3. Write movement record
            var movement = new StockMovement
            {
                FkStore = input.FkStore,
                FkProduct = input.FkProduct,
                FkProductVariant = input.FkProductVariant,
                MovementType = input.MovementType,
                Quantity = input.Quantity,
                ReferenceType = input.ReferenceType,
                ReferenceId = input.ReferenceId,
                Notes = input.Notes,
                CreatedBy = input.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };
            _db.StockMovements.Add(movement);

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return new OutputStockMovement
            {
                IdStockMovement = movement.IdStockMovement,
                NewQuantityOnHand = inv.QuantityOnHand
            };
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<OutputStockTransfer> TransferAsync(InputStockTransfer input, CancellationToken ct = default)
    {
        if (input.FromStoreId == input.ToStoreId)
            return new OutputStockTransfer { ResponseCode = 1, ResponseMsg = "From and To stores must differ.", StatusCode = 422 };

        // Deduct from source
        var outResult = await ApplyMovementAsync(new InputStockMovement
        {
            FkStore = input.FromStoreId,
            FkProduct = input.FkProduct,
            FkProductVariant = input.FkProductVariant,
            MovementType = "transfer_out",
            Quantity = input.Quantity,
            Notes = input.Notes,
            CreatedBy = input.CreatedBy
        }, ct);

        if (!outResult.Ok)
            return new OutputStockTransfer { ResponseCode = outResult.ResponseCode, ResponseMsg = outResult.ResponseMsg, StatusCode = outResult.StatusCode };

        // Add to destination
        var inResult = await ApplyMovementAsync(new InputStockMovement
        {
            FkStore = input.ToStoreId,
            FkProduct = input.FkProduct,
            FkProductVariant = input.FkProductVariant,
            MovementType = "transfer_in",
            Quantity = input.Quantity,
            Notes = input.Notes,
            CreatedBy = input.CreatedBy
        }, ct);

        if (!inResult.Ok)
            return new OutputStockTransfer { ResponseCode = inResult.ResponseCode, ResponseMsg = inResult.ResponseMsg, StatusCode = inResult.StatusCode };

        return new OutputStockTransfer();
    }

    public async Task<OutputInventorySelect?> GetInventoryAsync(InputInventorySelect input, CancellationToken ct = default)
    {
        var q = _db.Inventories
            .Include(x => x.Product)
            .Where(x => x.FkStore == input.FkStore && x.FkProduct == input.FkProduct);

        if (input.FkProductVariant.HasValue)
            q = q.Where(x => x.FkProductVariant == input.FkProductVariant.Value);
        else
            q = q.Where(x => x.FkProductVariant == null);

        return await q.Select(x => new OutputInventorySelect
        {
            IdInventory = x.IdInventory,
            FkStore = x.FkStore,
            FkProduct = x.FkProduct,
            ProductName = x.Product != null ? x.Product.ProductName : string.Empty,
            ProductCode = x.Product != null ? x.Product.ProductCode : null,
            FkProductVariant = x.FkProductVariant,
            QuantityOnHand = x.QuantityOnHand,
            QuantityReserved = x.QuantityReserved,
            ReorderLevel = x.ReorderLevel
        }).FirstOrDefaultAsync(ct);
    }

    public async Task<OutputInventoryList> ListInventoryAsync(InputInventoryList input, CancellationToken ct = default)
    {
        var q = _db.Inventories
            .Include(x => x.Product)
            .Where(x => x.FkStore == input.FkStore);

        if (!string.IsNullOrWhiteSpace(input.Search))
            q = q.Where(x => x.Product != null && (x.Product.ProductName.Contains(input.Search) || x.Product.ProductCode.Contains(input.Search)));

        if (input.FkCategory.HasValue)
            q = q.Where(x => x.Product != null && x.Product.FkCategory == input.FkCategory.Value);

        if (input.BelowReorderOnly)
            q = q.Where(x => x.QuantityOnHand <= x.ReorderLevel);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderBy(x => x.Product != null ? x.Product.ProductName : string.Empty)
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .Select(x => new OutputInventorySelect
            {
                IdInventory = x.IdInventory,
                FkStore = x.FkStore,
                FkProduct = x.FkProduct,
                ProductName = x.Product != null ? x.Product.ProductName : string.Empty,
                ProductCode = x.Product != null ? x.Product.ProductCode : null,
                FkProductVariant = x.FkProductVariant,
                QuantityOnHand = x.QuantityOnHand,
                QuantityReserved = x.QuantityReserved,
                ReorderLevel = x.ReorderLevel
            })
            .ToListAsync(ct);

        return new OutputInventoryList { TotalCount = total, Page = input.Page, PageSize = input.PageSize, Items = items };
    }

    public async Task<OutputStockMovementList> ListMovementsAsync(InputStockMovementList input, CancellationToken ct = default)
    {
        var q = _db.StockMovements
            .Include(x => x.Store)
            .Include(x => x.Product)
            .AsQueryable();

        if (input.FkStore.HasValue) q = q.Where(x => x.FkStore == input.FkStore.Value);
        if (input.FkProduct.HasValue) q = q.Where(x => x.FkProduct == input.FkProduct.Value);
        if (!string.IsNullOrWhiteSpace(input.MovementType)) q = q.Where(x => x.MovementType == input.MovementType);
        if (input.From.HasValue) q = q.Where(x => x.CreatedAt >= input.From.Value);
        if (input.To.HasValue) q = q.Where(x => x.CreatedAt <= input.To.Value);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(x => x.CreatedAt)
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .Select(x => new OutputStockMovementItem
            {
                IdStockMovement = x.IdStockMovement,
                FkStore = x.FkStore,
                StoreName = x.Store != null ? x.Store.StoreName : string.Empty,
                FkProduct = x.FkProduct,
                ProductName = x.Product != null ? x.Product.ProductName : string.Empty,
                MovementType = x.MovementType,
                Quantity = x.Quantity,
                ReferenceType = x.ReferenceType,
                ReferenceId = x.ReferenceId,
                Notes = x.Notes,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(ct);

        return new OutputStockMovementList { TotalCount = total, Page = input.Page, PageSize = input.PageSize, Items = items };
    }

    public async Task<OutputLowStockList> GetLowStockAsync(InputLowStockList input, CancellationToken ct = default)
    {
        var q = _db.Inventories
            .Include(x => x.Product)
            .Where(x => x.FkStore == input.FkStore && x.QuantityOnHand <= x.ReorderLevel);

        if (input.FkCategory.HasValue)
            q = q.Where(x => x.Product != null && x.Product.FkCategory == input.FkCategory.Value);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderBy(x => x.QuantityOnHand)
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .Select(x => new OutputInventorySelect
            {
                IdInventory = x.IdInventory,
                FkStore = x.FkStore,
                FkProduct = x.FkProduct,
                ProductName = x.Product != null ? x.Product.ProductName : string.Empty,
                ProductCode = x.Product != null ? x.Product.ProductCode : null,
                FkProductVariant = x.FkProductVariant,
                QuantityOnHand = x.QuantityOnHand,
                QuantityReserved = x.QuantityReserved,
                ReorderLevel = x.ReorderLevel
            })
            .ToListAsync(ct);

        return new OutputLowStockList { TotalCount = total, Page = input.Page, PageSize = input.PageSize, Items = items };
    }
}
