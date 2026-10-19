using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

public class PurchaseRepository : IPurchaseRepository
{
    private readonly SmartCartDbContext _db;
    private readonly IStockRepository _stock;

    public PurchaseRepository(SmartCartDbContext db, IStockRepository stock)
    {
        _db = db;
        _stock = stock;
    }

    public async Task<OutputPurchaseSelect?> GetPurchaseAsync(InputPurchaseSelect input, CancellationToken ct = default)
    {
        var head = await _db.Purchases
            .Where(x => x.IdPurchase == input.IdPurchase && x.FkStore == input.FkStore)
            .Select(x => new
            {
                x.IdPurchase,
                x.FkStore,
                StoreName = x.Store != null ? x.Store.StoreName : string.Empty,
                x.FkSupplier,
                SupplierName = x.Supplier != null ? x.Supplier.SupplierName : string.Empty,
                SupplierCode = x.Supplier != null ? x.Supplier.SupplierCode : null,
                SupplierPhone = x.Supplier != null ? x.Supplier.Phone : null,
                SupplierEmail = x.Supplier != null ? x.Supplier.Email : null,
                SupplierTaxNumber = x.Supplier != null ? x.Supplier.TaxNumber : null,
                AddressLine1 = x.Supplier != null ? x.Supplier.AddressLine1 : null,
                City = x.Supplier != null ? x.Supplier.City : null,
                State = x.Supplier != null ? x.Supplier.State : null,
                PostalCode = x.Supplier != null ? x.Supplier.PostalCode : null,
                Country = x.Supplier != null ? x.Supplier.Country : null,
                x.PurchaseDate,
                x.InvoiceNumber,
                x.SubTotal,
                x.DiscountAmount,
                x.TaxAmount,
                x.TotalAmount,
                x.Status,
                x.Notes,
                x.CreatedAt,
                x.UpdatedAt,
                x.Cancelled,
                x.CancelledOn,
                x.CancelledReason
            })
            .FirstOrDefaultAsync(ct);

        if (head is null) return null;

        var details = await _db.PurchaseDetails
            .Where(d => d.FkPurchase == head.IdPurchase)
            .OrderBy(d => d.IdPurchaseDetail)
            .Select(d => new
            {
                d.IdPurchaseDetail,
                d.FkProduct,
                d.FkProductVariant,
                d.Quantity,
                d.UnitCost,
                d.TaxAmount,
                d.LineTotal
            })
            .ToListAsync(ct);

        var productIds = details.Select(d => d.FkProduct).Distinct().ToList();
        var products = await _db.Products
            .Where(p => productIds.Contains(p.IdProduct))
            .Select(p => new { p.IdProduct, p.ProductName, p.ProductCode, p.FkTax })
            .ToListAsync(ct);
        var productMap = products.ToDictionary(p => p.IdProduct);

        var variantIds = details
            .Where(d => d.FkProductVariant.HasValue)
            .Select(d => d.FkProductVariant!.Value)
            .Distinct()
            .ToList();
        var variantNames = variantIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.ProductVariants
                .Where(v => variantIds.Contains(v.IdProductVariant))
                .ToDictionaryAsync(v => v.IdProductVariant, v => v.VariantName, ct);

        var taxIds = products.Where(p => p.FkTax.HasValue).Select(p => p.FkTax!.Value).Distinct().ToList();
        var taxRates = taxIds.Count == 0
            ? new Dictionary<int, decimal>()
            : await _db.Taxes
                .Where(t => taxIds.Contains(t.IdTax) && !t.Cancelled)
                .ToDictionaryAsync(t => t.IdTax, t => t.TaxRate, ct);

        // Received quantity is derived from the stock movements posted for this purchase order.
        var movements = await _db.StockMovements
            .Where(m => m.FkStore == head.FkStore
                && m.MovementType == "purchase"
                && m.ReferenceType == "purchase"
                && m.ReferenceId == head.IdPurchase)
            .OrderBy(m => m.CreatedAt).ThenBy(m => m.IdStockMovement)
            .Select(m => new { m.IdStockMovement, m.FkProduct, m.FkProductVariant, m.Quantity, m.Notes, m.CreatedAt })
            .ToListAsync(ct);

        var pool = new Dictionary<(int, int), decimal>();
        foreach (var m in movements)
        {
            var movementKey = (m.FkProduct, m.FkProductVariant ?? 0);
            pool[movementKey] = pool.GetValueOrDefault(movementKey) + m.Quantity;
        }

        var lines = new List<OutputPurchaseLine>(details.Count);
        foreach (var d in details)
        {
            // Received stock is allocated to lines of the same product / variant in order, never above the ordered quantity.
            var key = (d.FkProduct, d.FkProductVariant ?? 0);
            var available = pool.GetValueOrDefault(key);
            var received = Math.Min(d.Quantity, available);
            pool[key] = available - received;

            productMap.TryGetValue(d.FkProduct, out var product);

            decimal taxRate = 0m;
            if (product?.FkTax is int taxId && taxRates.TryGetValue(taxId, out var rate))
                taxRate = rate;

            string? variantName = null;
            if (d.FkProductVariant is int variantId && variantNames.TryGetValue(variantId, out var vn))
                variantName = vn;

            // The table has no discount column: it is what is missing between gross + tax and the stored line total.
            var gross = Math.Round(d.Quantity * d.UnitCost, 2, MidpointRounding.AwayFromZero);
            var discount = Math.Max(0m, gross + d.TaxAmount - d.LineTotal);

            lines.Add(new OutputPurchaseLine
            {
                IdPurchaseDetail = d.IdPurchaseDetail,
                FkProduct = d.FkProduct,
                ProductName = product?.ProductName ?? string.Empty,
                ProductCode = product?.ProductCode,
                FkProductVariant = d.FkProductVariant,
                VariantName = variantName,
                Quantity = d.Quantity,
                ReceivedQty = received,
                UnitCost = d.UnitCost,
                DiscountAmount = discount,
                TaxRate = taxRate,
                TaxAmount = d.TaxAmount,
                LineTotal = d.LineTotal
            });
        }

        var receipts = new List<OutputPurchaseReceipt>(movements.Count);
        foreach (var m in movements)
        {
            productMap.TryGetValue(m.FkProduct, out var product);

            string? variantName = null;
            if (m.FkProductVariant is int variantId && variantNames.TryGetValue(variantId, out var vn))
                variantName = vn;

            receipts.Add(new OutputPurchaseReceipt
            {
                IdStockMovement = m.IdStockMovement,
                ReceivedAt = m.CreatedAt,
                FkProduct = m.FkProduct,
                ProductName = product?.ProductName ?? string.Empty,
                VariantName = variantName,
                Quantity = m.Quantity,
                Notes = m.Notes
            });
        }

        var address = string.Join(", ", new[] { head.AddressLine1, head.City, head.State, head.PostalCode, head.Country }
            .Where(s => !string.IsNullOrWhiteSpace(s)));

        return new OutputPurchaseSelect
        {
            IdPurchase = head.IdPurchase,
            FkStore = head.FkStore,
            StoreName = head.StoreName,
            FkSupplier = head.FkSupplier,
            SupplierName = head.SupplierName,
            SupplierCode = head.SupplierCode,
            SupplierPhone = head.SupplierPhone,
            SupplierEmail = head.SupplierEmail,
            SupplierTaxNumber = head.SupplierTaxNumber,
            SupplierAddress = address.Length == 0 ? null : address,
            PurchaseDate = head.PurchaseDate,
            InvoiceNumber = head.InvoiceNumber,
            SubTotal = head.SubTotal,
            DiscountAmount = head.DiscountAmount,
            TaxAmount = head.TaxAmount,
            TotalAmount = head.TotalAmount,
            Status = head.Status,
            Notes = head.Notes,
            CreatedAt = head.CreatedAt,
            UpdatedAt = head.UpdatedAt,
            Cancelled = head.Cancelled,
            CancelledOn = head.CancelledOn,
            CancelledReason = head.CancelledReason,
            Lines = lines,
            Receipts = receipts
        };
    }

    public async Task<OutputPurchaseList> ListPurchasesAsync(InputPurchaseList input, CancellationToken ct = default)
    {
        var q = _db.Purchases.AsQueryable();

        if (input.FkStore is int storeId) q = q.Where(x => x.FkStore == storeId);
        if (input.FkSupplier is int supplierId && supplierId > 0) q = q.Where(x => x.FkSupplier == supplierId);

        if (!string.IsNullOrWhiteSpace(input.Status))
        {
            var status = input.Status;
            q = q.Where(x => x.Status == status);
        }

        if (input.From is DateOnly dateFrom) q = q.Where(x => x.PurchaseDate >= dateFrom);
        if (input.To is DateOnly dateTo) q = q.Where(x => x.PurchaseDate <= dateTo);

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var term = input.Search.Trim().ToLower();
            var searchId = input.SearchId ?? 0;
            q = q.Where(x => (x.InvoiceNumber != null && x.InvoiceNumber.ToLower().Contains(term))
                || (x.Supplier != null && x.Supplier.SupplierName.ToLower().Contains(term))
                || (searchId > 0 && x.IdPurchase == searchId));
        }

        q = input.SortBy switch
        {
            "id" => input.SortDesc
                ? q.OrderByDescending(x => x.IdPurchase)
                : q.OrderBy(x => x.IdPurchase),
            "supplier" => input.SortDesc
                ? q.OrderByDescending(x => x.Supplier!.SupplierName).ThenByDescending(x => x.IdPurchase)
                : q.OrderBy(x => x.Supplier!.SupplierName).ThenByDescending(x => x.IdPurchase),
            "invoicenumber" => input.SortDesc
                ? q.OrderByDescending(x => x.InvoiceNumber).ThenByDescending(x => x.IdPurchase)
                : q.OrderBy(x => x.InvoiceNumber).ThenByDescending(x => x.IdPurchase),
            "totalamount" => input.SortDesc
                ? q.OrderByDescending(x => x.TotalAmount).ThenByDescending(x => x.IdPurchase)
                : q.OrderBy(x => x.TotalAmount).ThenByDescending(x => x.IdPurchase),
            "status" => input.SortDesc
                ? q.OrderByDescending(x => x.Status).ThenByDescending(x => x.IdPurchase)
                : q.OrderBy(x => x.Status).ThenByDescending(x => x.IdPurchase),
            _ => input.SortDesc
                ? q.OrderByDescending(x => x.PurchaseDate).ThenByDescending(x => x.IdPurchase)
                : q.OrderBy(x => x.PurchaseDate).ThenBy(x => x.IdPurchase)
        };

        var total = await q.CountAsync(ct);
        var items = await q
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .Select(x => new OutputPurchaseListItem
            {
                IdPurchase = x.IdPurchase,
                FkStore = x.FkStore,
                StoreName = x.Store != null ? x.Store.StoreName : string.Empty,
                FkSupplier = x.FkSupplier,
                SupplierName = x.Supplier != null ? x.Supplier.SupplierName : string.Empty,
                PurchaseDate = x.PurchaseDate,
                InvoiceNumber = x.InvoiceNumber,
                LineCount = x.Details.Count(),
                TotalAmount = x.TotalAmount,
                Status = x.Status,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(ct);

        return new OutputPurchaseList { TotalCount = total, Page = input.Page, PageSize = input.PageSize, Items = items };
    }

    public async Task<OutputPurchaseCreate> CreatePurchaseAsync(InputPurchaseCreate input, CancellationToken ct = default)
    {
        var supplierOk = await _db.Suppliers.AnyAsync(s => s.IdSupplier == input.FkSupplier && !s.Cancelled && s.IsActive, ct);
        if (!supplierOk)
            return new OutputPurchaseCreate { ResponseCode = 1, ResponseMsg = "The selected supplier is not available. Choose an active supplier.", StatusCode = 422 };

        var now = DateTime.UtcNow;
        var po = new Purchase
        {
            FkStore = input.FkStore,
            FkSupplier = input.FkSupplier,
            PurchaseDate = input.PurchaseDate,
            InvoiceNumber = input.InvoiceNumber,
            Notes = input.Notes,
            SubTotal = input.SubTotal,
            DiscountAmount = input.DiscountAmount,
            TaxAmount = input.TaxAmount,
            TotalAmount = input.TotalAmount,
            Status = input.SaveAsDraft ? "draft" : "posted",
            CreatedBy = input.CreatedBy is > 0 ? input.CreatedBy : null,
            CreatedAt = now
        };

        foreach (var line in input.Lines)
        {
            po.Details.Add(new PurchaseDetail
            {
                FkStore = input.FkStore,
                FkProduct = line.FkProduct,
                FkProductVariant = line.FkProductVariant,
                Quantity = line.Quantity,
                UnitCost = line.UnitCost,
                TaxAmount = line.TaxAmount,
                LineTotal = line.LineTotal,
                CreatedAt = now
            });
        }

        // Header and lines are written by one SaveChanges call, which EF Core wraps in a single transaction.
        _db.Purchases.Add(po);
        await _db.SaveChangesAsync(ct);

        return new OutputPurchaseCreate { IdPurchase = po.IdPurchase };
    }

    public async Task<OutputPurchaseUpdate> UpdatePurchaseAsync(InputPurchaseUpdate input, CancellationToken ct = default)
    {
        var po = await _db.Purchases
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.IdPurchase == input.IdPurchase && x.FkStore == input.FkStore, ct);
        if (po is null)
            return new OutputPurchaseUpdate { ResponseCode = 1, ResponseMsg = "Purchase order not found.", StatusCode = 404 };

        if (po.Cancelled || (po.Status != "draft" && po.Status != "posted"))
            return new OutputPurchaseUpdate { ResponseCode = 2, ResponseMsg = "This purchase order can no longer be edited.", StatusCode = 409 };

        var hasReceipts = await _db.StockMovements.AnyAsync(m => m.FkStore == po.FkStore
            && m.MovementType == "purchase"
            && m.ReferenceType == "purchase"
            && m.ReferenceId == po.IdPurchase, ct);
        if (hasReceipts)
            return new OutputPurchaseUpdate { ResponseCode = 3, ResponseMsg = "Goods have already been received against this purchase order, so it can no longer be edited.", StatusCode = 409 };

        if (po.Status == "posted" && input.SaveAsDraft)
            return new OutputPurchaseUpdate { ResponseCode = 4, ResponseMsg = "A posted purchase order cannot be moved back to draft.", StatusCode = 409 };

        if (po.FkSupplier != input.FkSupplier)
        {
            var supplierOk = await _db.Suppliers.AnyAsync(s => s.IdSupplier == input.FkSupplier && !s.Cancelled && s.IsActive, ct);
            if (!supplierOk)
                return new OutputPurchaseUpdate { ResponseCode = 5, ResponseMsg = "The selected supplier is not available. Choose an active supplier.", StatusCode = 422 };
        }

        var now = DateTime.UtcNow;
        po.FkSupplier = input.FkSupplier;
        po.PurchaseDate = input.PurchaseDate;
        po.InvoiceNumber = input.InvoiceNumber;
        po.Notes = input.Notes;
        po.SubTotal = input.SubTotal;
        po.DiscountAmount = input.DiscountAmount;
        po.TaxAmount = input.TaxAmount;
        po.TotalAmount = input.TotalAmount;
        po.Status = input.SaveAsDraft ? "draft" : "posted";
        po.UpdatedAt = now;

        var newDetails = input.Lines.Select(line => new PurchaseDetail
        {
            FkPurchase = po.IdPurchase,
            FkStore = po.FkStore,
            FkProduct = line.FkProduct,
            FkProductVariant = line.FkProductVariant,
            Quantity = line.Quantity,
            UnitCost = line.UnitCost,
            TaxAmount = line.TaxAmount,
            LineTotal = line.LineTotal,
            CreatedAt = now
        }).ToList();

        // Old lines are deleted and the new ones inserted by the same SaveChanges call (one transaction).
        _db.PurchaseDetails.RemoveRange(po.Details);
        _db.PurchaseDetails.AddRange(newDetails);
        await _db.SaveChangesAsync(ct);

        return new OutputPurchaseUpdate { IdPurchase = po.IdPurchase };
    }

    public async Task<OutputGoodsReceipt> ReceiveGoodsAsync(InputGoodsReceipt input, CancellationToken ct = default)
    {
        var po = await _db.Purchases
            .FirstOrDefaultAsync(x => x.IdPurchase == input.IdPurchase && x.FkStore == input.FkStore, ct);
        if (po is null)
            return new OutputGoodsReceipt { ResponseCode = 1, ResponseMsg = "Purchase order not found.", StatusCode = 404 };

        if (po.Cancelled || (po.Status != "posted" && po.Status != "partially_received"))
            return new OutputGoodsReceipt { ResponseCode = 2, ResponseMsg = "Goods can only be received against a posted or partially received purchase order.", StatusCode = 409 };

        var note = $"GRN {input.GrnNumber} | {input.ReceiptDate:yyyy-MM-dd}";
        if (!string.IsNullOrWhiteSpace(input.Notes))
            note += $" | {input.Notes.Trim()}";

        // No outer transaction on purpose: every stock movement opens (and commits) its own transaction.
        // The purchase status is written last, so it never claims more than what was actually received.
        foreach (var line in input.Lines)
        {
            var moved = await _stock.ApplyMovementAsync(new InputStockMovement
            {
                FkStore = po.FkStore,
                FkProduct = line.FkProduct,
                FkProductVariant = line.FkProductVariant,
                MovementType = "purchase",
                Quantity = line.ReceiveQty,
                ReferenceType = "purchase",
                ReferenceId = po.IdPurchase,
                Notes = note,
                CreatedBy = input.CreatedBy is > 0 ? input.CreatedBy : null
            }, ct);

            if (!moved.Ok)
                return new OutputGoodsReceipt { ResponseCode = moved.ResponseCode, ResponseMsg = moved.ResponseMsg, StatusCode = moved.StatusCode };
        }

        po.Status = input.NewStatus;
        po.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new OutputGoodsReceipt { GrnNumber = input.GrnNumber, NewPurchaseStatus = po.Status };
    }

    public async Task<OutputPurchaseCancel> CancelPurchaseAsync(InputPurchaseCancel input, CancellationToken ct = default)
    {
        var po = await _db.Purchases
            .FirstOrDefaultAsync(x => x.IdPurchase == input.IdPurchase && x.FkStore == input.FkStore, ct);
        if (po is null)
            return new OutputPurchaseCancel { ResponseCode = 1, ResponseMsg = "Purchase order not found.", StatusCode = 404 };

        if (po.Cancelled || po.Status == "cancelled")
            return new OutputPurchaseCancel { ResponseCode = 2, ResponseMsg = "This purchase order is already cancelled.", StatusCode = 409 };

        if (po.Status == "received")
            return new OutputPurchaseCancel { ResponseCode = 3, ResponseMsg = "A fully received purchase order cannot be cancelled.", StatusCode = 409 };

        var now = DateTime.UtcNow;
        po.Cancelled = true;
        po.CancelledOn = now;
        po.CancelledReason = input.Reason;
        po.Status = "cancelled";
        po.UpdatedAt = now;
        await _db.SaveChangesAsync(ct);

        return new OutputPurchaseCancel();
    }

    public async Task<IReadOnlyList<OutputPurchaseProductSelect>> ListProductsAsync(InputPurchaseProductList input, CancellationToken ct = default)
    {
        var products = _db.Products.Where(p => !p.Cancelled && p.IsActive);

        if (input.ProductIds is { Count: > 0 })
        {
            var ids = input.ProductIds;
            products = products.Where(p => ids.Contains(p.IdProduct));
        }
        else if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var term = input.Search.Trim().ToLower();
            products = products.Where(p => p.ProductName.ToLower().Contains(term)
                || p.ProductCode.ToLower().Contains(term)
                || (p.Sku != null && p.Sku.ToLower().Contains(term))
                || (p.Barcode != null && p.Barcode.ToLower().Contains(term))
                || _db.ProductVariants.Any(v => v.FkProduct == p.IdProduct && !v.Cancelled && v.IsActive
                    && (v.VariantName.ToLower().Contains(term)
                        || (v.SkuCode != null && v.SkuCode.ToLower().Contains(term))
                        || (v.Barcode != null && v.Barcode.ToLower().Contains(term)))));
        }
        else
        {
            return new List<OutputPurchaseProductSelect>();
        }

        var limit = input.Limit > 0 ? input.Limit : 10;
        var rows = await products
            .OrderBy(p => p.ProductName)
            .Take(limit)
            .Select(p => new
            {
                p.IdProduct,
                p.ProductCode,
                p.ProductName,
                p.Sku,
                p.Barcode,
                p.CostPrice,
                TaxRate = _db.Taxes
                    .Where(t => t.IdTax == p.FkTax && !t.Cancelled)
                    .Select(t => (decimal?)t.TaxRate)
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        var result = new List<OutputPurchaseProductSelect>();
        if (rows.Count == 0) return result;

        var productIds = rows.Select(r => r.IdProduct).ToList();
        var variants = await _db.ProductVariants
            .Where(v => productIds.Contains(v.FkProduct) && !v.Cancelled && v.IsActive)
            .OrderBy(v => v.VariantName)
            .Select(v => new { v.IdProductVariant, v.FkProduct, v.VariantName, v.SkuCode, v.Barcode })
            .ToListAsync(ct);

        foreach (var r in rows)
        {
            var rate = r.TaxRate ?? 0m;

            result.Add(new OutputPurchaseProductSelect
            {
                FkProduct = r.IdProduct,
                ProductCode = r.ProductCode,
                ProductName = r.ProductName,
                Sku = r.Sku,
                Barcode = r.Barcode,
                UnitCost = r.CostPrice,
                TaxRate = rate
            });

            foreach (var v in variants.Where(v => v.FkProduct == r.IdProduct))
            {
                result.Add(new OutputPurchaseProductSelect
                {
                    FkProduct = r.IdProduct,
                    FkProductVariant = v.IdProductVariant,
                    ProductCode = r.ProductCode,
                    ProductName = r.ProductName,
                    VariantName = v.VariantName,
                    Sku = v.SkuCode ?? r.Sku,
                    Barcode = v.Barcode ?? r.Barcode,
                    UnitCost = r.CostPrice,
                    TaxRate = rate
                });
            }
        }

        return result;
    }
}
