using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Exceptions;
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

    #region Purchases

    public async Task<OutputPurchaseSelect?> GetPurchaseAsync(InputPurchaseSelect input, CancellationToken ct = default)
    {
        var head = await _db.Purchases
            .AsNoTracking()
            .Include(p => p.Store)
            .Include(p => p.Supplier)
            .Include(p => p.Details)
                .ThenInclude(d => d.Product)
            .Include(p => p.Details)
                .ThenInclude(d => d.ProductVariant)
            .FirstOrDefaultAsync(x => x.IdPurchase == input.IdPurchase && x.FkStore == input.FkStore, ct);

        if (head is null) return null;

        var returnedMap = await _db.PurchaseReturnDetails
            .Where(r => r.PurchaseReturn!.FkPurchase == head.IdPurchase && r.PurchaseReturn.Status == "Approved" && !r.PurchaseReturn.Cancelled)
            .GroupBy(r => r.FkPurchaseDetail)
            .Select(g => new { FkPurchaseDetail = g.Key, Qty = g.Sum(x => x.Quantity) })
            .ToDictionaryAsync(x => x.FkPurchaseDetail, x => x.Qty, ct);

        return new OutputPurchaseSelect
        {
            IdPurchase = head.IdPurchase,
            FkStore = head.FkStore,
            StoreName = head.Store?.StoreName ?? string.Empty,
            FkSupplier = head.FkSupplier,
            SupplierName = head.Supplier?.SupplierName ?? string.Empty,
            SupplierCode = head.Supplier?.SupplierCode,
            SupplierPhone = head.Supplier?.Phone,
            SupplierEmail = head.Supplier?.Email,
            SupplierTaxNumber = head.Supplier?.TaxNumber,
            SupplierAddress = head.Supplier?.AddressLine1,
            PurchaseDate = head.PurchaseDate,
            InvoiceNumber = head.InvoiceNumber,
            InvoiceDate = head.InvoiceDate,
            SubTotal = head.SubTotal,
            DiscountAmount = head.DiscountAmount,
            TaxableAmount = head.TaxableAmount,
            TaxAmount = head.TaxAmount,
            Charges = head.Charges,
            RoundOff = head.RoundOff,
            TotalAmount = head.TotalAmount,
            AmountPaid = head.AmountPaid,
            PaymentMethod = head.PaymentMethod,
            Status = head.Status,
            Notes = head.Notes,
            CreatedAt = head.CreatedAt,
            UpdatedAt = head.UpdatedAt,
            ApprovedBy = head.ApprovedBy,
            ApprovedAt = head.ApprovedAt,
            Cancelled = head.Cancelled,
            CancelledOn = head.CancelledOn,
            CancelledReason = head.CancelledReason,
            Lines = head.Details.Select(d => new OutputPurchaseLine
            {
                IdPurchaseDetail = d.IdPurchaseDetail,
                FkProduct = d.FkProduct,
                ProductName = d.Product?.ProductName ?? string.Empty,
                ProductCode = d.Product?.ProductCode,
                FkProductVariant = d.FkProductVariant,
                VariantName = d.ProductVariant?.VariantName,
                Quantity = d.Quantity,
                UnitCost = d.UnitCost,
                DiscountAmount = d.DiscountAmount,
                TaxableAmount = d.TaxableAmount,
                TaxRate = d.TaxRate,
                TaxInclusive = d.TaxInclusive,
                TaxAmount = d.TaxAmount,
                LineTotal = d.LineTotal,
                ReturnedQty = returnedMap.GetValueOrDefault(d.IdPurchaseDetail, 0m)
            }).ToList()
        };
    }

    public async Task<OutputPurchaseList> ListPurchasesAsync(InputPurchaseList input, CancellationToken ct = default)
    {
        var query = _db.Purchases.AsNoTracking().Where(p => p.FkStore == input.FkStore);

        if (input.FkSupplier is > 0)
            query = query.Where(p => p.FkSupplier == input.FkSupplier);

        if (!string.IsNullOrWhiteSpace(input.Status))
            query = query.Where(p => p.Status == input.Status);

        if (input.From.HasValue)
            query = query.Where(p => p.PurchaseDate >= input.From.Value);

        if (input.To.HasValue)
            query = query.Where(p => p.PurchaseDate <= input.To.Value);

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var term = input.Search.Trim().ToLowerInvariant();
            query = query.Where(p => (p.InvoiceNumber != null && p.InvoiceNumber.ToLower().Contains(term))
                || (p.Supplier != null && p.Supplier.SupplierName.ToLower().Contains(term)));
        }

        var total = await query.CountAsync(ct);
        input.Page = Math.Max(1, input.Page);
        input.PageSize = Math.Clamp(input.PageSize, 1, 100);

        var items = await query
            .OrderByDescending(p => p.PurchaseDate)
            .ThenByDescending(p => p.IdPurchase)
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .Select(p => new OutputPurchaseListItem
            {
                IdPurchase = p.IdPurchase,
                FkStore = p.FkStore,
                StoreName = p.Store != null ? p.Store.StoreName : string.Empty,
                FkSupplier = p.FkSupplier,
                SupplierName = p.Supplier != null ? p.Supplier.SupplierName : string.Empty,
                PurchaseDate = p.PurchaseDate,
                InvoiceNumber = p.InvoiceNumber,
                LineCount = p.Details.Count,
                TotalAmount = p.TotalAmount,
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                ApprovedAt = p.ApprovedAt
            })
            .ToListAsync(ct);

        return new OutputPurchaseList
        {
            TotalCount = total,
            Page = input.Page,
            PageSize = input.PageSize,
            Items = items,
            Filter = input
        };
    }

    public async Task<OutputPurchaseCreate> CreatePurchaseAsync(InputPurchaseCreate input, CancellationToken ct = default)
    {
        var supplier = await _db.Suppliers.FirstOrDefaultAsync(s => s.IdSupplier == input.FkSupplier && s.IsActive && !s.Cancelled, ct);
        if (supplier is null)
            throw new ValidationException("Supplier", "Selected supplier does not exist or is inactive.");

        var existingInvoice = await _db.Purchases.AnyAsync(p => p.FkSupplier == input.FkSupplier && p.InvoiceNumber != null && p.InvoiceNumber.ToLower() == input.InvoiceNumber.Trim().ToLower(), ct);
        if (existingInvoice)
            throw new ConflictException($"Invoice number '{input.InvoiceNumber}' has already been recorded for this supplier.");

        var now = DateTime.UtcNow;
        var purchase = new Purchase
        {
            FkStore = input.FkStore,
            FkSupplier = input.FkSupplier,
            PurchaseDate = input.PurchaseDate,
            InvoiceNumber = input.InvoiceNumber.Trim(),
            InvoiceDate = input.InvoiceDate,
            SubTotal = input.SubTotal,
            DiscountAmount = input.DiscountAmount,
            TaxableAmount = input.TaxableAmount,
            TaxAmount = input.TaxAmount,
            Charges = input.Charges,
            RoundOff = input.RoundOff,
            TotalAmount = input.TotalAmount,
            AmountPaid = input.AmountPaid,
            PaymentMethod = input.PaymentMethod,
            Notes = input.Notes,
            Status = input.MarkAsApproved ? "Approved" : "Pending",
            CreatedBy = input.CreatedBy,
            CreatedAt = now,
            ApprovedBy = input.MarkAsApproved ? input.CreatedBy : null,
            ApprovedAt = input.MarkAsApproved ? now : null
        };

        foreach (var l in input.Lines)
        {
            purchase.Details.Add(new PurchaseDetail
            {
                FkStore = input.FkStore,
                FkProduct = l.FkProduct,
                FkProductVariant = l.FkProductVariant,
                Quantity = l.Quantity,
                UnitCost = l.UnitCost,
                DiscountAmount = l.DiscountAmount,
                TaxableAmount = l.TaxableAmount,
                TaxRate = l.TaxRate,
                TaxInclusive = l.TaxInclusive,
                TaxAmount = l.TaxAmount,
                LineTotal = l.LineTotal,
                CreatedAt = now
            });
        }

        using var tx = await _db.Database.BeginTransactionAsync(ct);

        _db.Purchases.Add(purchase);
        await _db.SaveChangesAsync(ct);

        if (input.MarkAsApproved)
        {
            foreach (var d in purchase.Details)
            {
                await _stock.ApplyMovementAsync(new InputStockMovement
                {
                    FkStore = input.FkStore,
                    FkProduct = d.FkProduct,
                    FkProductVariant = d.FkProductVariant,
                    MovementType = "purchase",
                    Quantity = d.Quantity,
                    ReferenceType = "purchase",
                    ReferenceId = purchase.IdPurchase,
                    UnitCost = d.UnitCost,
                    CreatedBy = input.CreatedBy,
                    Notes = $"Purchase PUR-{purchase.IdPurchase:D6} (Inv: {purchase.InvoiceNumber})"
                }, ct);
            }

            var billNum = $"PUR-{purchase.IdPurchase:D6}";
            _db.SupplierTransactions.Add(new SupplierTransaction
            {
                FkSupplier = purchase.FkSupplier,
                FkStore = purchase.FkStore,
                TransactionNumber = billNum,
                TransactionDate = purchase.PurchaseDate,
                TransactionType = "Purchase",
                SourceType = "Purchase",
                SourceId = purchase.IdPurchase,
                Amount = purchase.TotalAmount,
                SignedAmount = -purchase.TotalAmount, // Negative = payable
                ReferenceNo = purchase.InvoiceNumber,
                Notes = $"Purchase bill {billNum}",
                CreatedBy = input.CreatedBy ?? 0,
                CreatedAt = now
            });

            if (purchase.AmountPaid > 0)
            {
                _db.SupplierTransactions.Add(new SupplierTransaction
                {
                    FkSupplier = purchase.FkSupplier,
                    FkStore = purchase.FkStore,
                    TransactionNumber = $"PAY-{purchase.IdPurchase:D6}",
                    TransactionDate = purchase.PurchaseDate,
                    TransactionType = "Payment",
                    SourceType = "Purchase",
                    SourceId = purchase.IdPurchase,
                    Amount = purchase.AmountPaid,
                    SignedAmount = +purchase.AmountPaid, // Positive = payment reduces payable
                    PaymentMethod = purchase.PaymentMethod ?? "Cash",
                    ReferenceNo = purchase.InvoiceNumber,
                    Notes = $"Immediate payment on {billNum}",
                    CreatedBy = input.CreatedBy ?? 0,
                    CreatedAt = now
                });
            }

            await _db.SaveChangesAsync(ct);
        }

        await tx.CommitAsync(ct);

        return new OutputPurchaseCreate
        {
            IdPurchase = purchase.IdPurchase,
            ResponseCode = 0,
            StatusCode = 200,
            ResponseMsg = input.MarkAsApproved ? "Purchase recorded and approved into stock." : "Purchase saved as Pending."
        };
    }

    public async Task<OutputPurchaseUpdate> UpdatePurchaseAsync(InputPurchaseUpdate input, CancellationToken ct = default)
    {
        var purchase = await _db.Purchases
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.IdPurchase == input.IdPurchase && p.FkStore == input.FkStore, ct);

        if (purchase is null)
            throw new ResourceNotFoundException("Purchase not found.");

        if (purchase.Status != "Pending" || purchase.Cancelled)
            throw new ConflictException("Only pending, non-cancelled purchases can be edited.");

        var supplier = await _db.Suppliers.FirstOrDefaultAsync(s => s.IdSupplier == input.FkSupplier && s.IsActive && !s.Cancelled, ct);
        if (supplier is null)
            throw new ValidationException("Supplier", "Selected supplier does not exist or is inactive.");

        var now = DateTime.UtcNow;
        purchase.FkSupplier = input.FkSupplier;
        purchase.PurchaseDate = input.PurchaseDate;
        purchase.InvoiceNumber = input.InvoiceNumber.Trim();
        purchase.InvoiceDate = input.InvoiceDate;
        purchase.SubTotal = input.SubTotal;
        purchase.DiscountAmount = input.DiscountAmount;
        purchase.TaxableAmount = input.TaxableAmount;
        purchase.TaxAmount = input.TaxAmount;
        purchase.Charges = input.Charges;
        purchase.RoundOff = input.RoundOff;
        purchase.TotalAmount = input.TotalAmount;
        purchase.Notes = input.Notes;
        purchase.UpdatedAt = now;

        _db.PurchaseDetails.RemoveRange(purchase.Details);

        foreach (var l in input.Lines)
        {
            purchase.Details.Add(new PurchaseDetail
            {
                FkPurchase = purchase.IdPurchase,
                FkStore = input.FkStore,
                FkProduct = l.FkProduct,
                FkProductVariant = l.FkProductVariant,
                Quantity = l.Quantity,
                UnitCost = l.UnitCost,
                DiscountAmount = l.DiscountAmount,
                TaxableAmount = l.TaxableAmount,
                TaxRate = l.TaxRate,
                TaxInclusive = l.TaxInclusive,
                TaxAmount = l.TaxAmount,
                LineTotal = l.LineTotal,
                CreatedAt = now
            });
        }

        await _db.SaveChangesAsync(ct);
        return new OutputPurchaseUpdate { IdPurchase = purchase.IdPurchase, ResponseCode = 0, StatusCode = 200 };
    }

    public async Task ApprovePurchaseAsync(int id, int storeId, int userId, CancellationToken ct = default)
    {
        var purchase = await _db.Purchases
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.IdPurchase == id && p.FkStore == storeId, ct);

        if (purchase is null)
            throw new ResourceNotFoundException("Purchase not found.");

        if (purchase.Status == "Approved")
            throw new ConflictException("Purchase is already approved.");

        if (purchase.Status != "Pending" || purchase.Cancelled)
            throw new ConflictException("Only pending, non-cancelled purchases can be approved.");

        var now = DateTime.UtcNow;
        using var tx = await _db.Database.BeginTransactionAsync(ct);

        purchase.Status = "Approved";
        purchase.ApprovedBy = userId;
        purchase.ApprovedAt = now;
        purchase.UpdatedAt = now;

        foreach (var d in purchase.Details)
        {
            await _stock.ApplyMovementAsync(new InputStockMovement
            {
                FkStore = storeId,
                FkProduct = d.FkProduct,
                FkProductVariant = d.FkProductVariant,
                MovementType = "purchase",
                Quantity = d.Quantity,
                ReferenceType = "purchase",
                ReferenceId = purchase.IdPurchase,
                UnitCost = d.UnitCost,
                CreatedBy = userId,
                Notes = $"Approved Purchase PUR-{purchase.IdPurchase:D6}"
            }, ct);
        }

        var billNum = $"PUR-{purchase.IdPurchase:D6}";
        _db.SupplierTransactions.Add(new SupplierTransaction
        {
            FkSupplier = purchase.FkSupplier,
            FkStore = purchase.FkStore,
            TransactionNumber = billNum,
            TransactionDate = purchase.PurchaseDate,
            TransactionType = "Purchase",
            SourceType = "Purchase",
            SourceId = purchase.IdPurchase,
            Amount = purchase.TotalAmount,
            SignedAmount = -purchase.TotalAmount, // Negative = payable
            ReferenceNo = purchase.InvoiceNumber,
            Notes = $"Purchase {billNum} approved",
            CreatedBy = userId,
            CreatedAt = now
        });

        if (purchase.AmountPaid > 0)
        {
            _db.SupplierTransactions.Add(new SupplierTransaction
            {
                FkSupplier = purchase.FkSupplier,
                FkStore = purchase.FkStore,
                TransactionNumber = $"PAY-{purchase.IdPurchase:D6}",
                TransactionDate = purchase.PurchaseDate,
                TransactionType = "Payment",
                SourceType = "Purchase",
                SourceId = purchase.IdPurchase,
                Amount = purchase.AmountPaid,
                SignedAmount = +purchase.AmountPaid,
                PaymentMethod = purchase.PaymentMethod ?? "Cash",
                ReferenceNo = purchase.InvoiceNumber,
                Notes = $"Immediate payment on {billNum}",
                CreatedBy = userId,
                CreatedAt = now
            });
        }

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task<OutputPurchaseCancel> CancelPurchaseAsync(InputPurchaseCancel input, CancellationToken ct = default)
    {
        var purchase = await _db.Purchases.FirstOrDefaultAsync(p => p.IdPurchase == input.IdPurchase && p.FkStore == input.FkStore, ct);
        if (purchase is null)
            throw new ResourceNotFoundException("Purchase not found.");

        if (purchase.Status == "Approved")
            throw new ConflictException("Approved purchases cannot be directly cancelled. Use a Purchase Return.");

        if (purchase.Cancelled || purchase.Status == "Cancelled")
            throw new ConflictException("Purchase is already cancelled.");

        var now = DateTime.UtcNow;
        purchase.Cancelled = true;
        purchase.CancelledOn = now;
        purchase.CancelledBy = input.CancelledBy;
        purchase.CancelledReason = input.Reason;
        purchase.Status = "Cancelled";
        purchase.UpdatedAt = now;

        await _db.SaveChangesAsync(ct);
        return new OutputPurchaseCancel { ResponseCode = 0, StatusCode = 200 };
    }

    public async Task<IReadOnlyList<OutputPurchaseProductSelect>> ListProductsAsync(InputPurchaseProductList input, CancellationToken ct = default)
    {
        var products = _db.Products.Where(p => !p.Cancelled && p.IsActive);

        if (input.ProductIds is { Count: > 0 })
        {
            products = products.Where(p => input.ProductIds.Contains(p.IdProduct));
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

    #endregion

    #region Purchase Returns

    public async Task<OutputPurchaseReturnSelect?> GetReturnAsync(int id, int storeId, CancellationToken ct = default)
    {
        var ret = await _db.PurchaseReturns
            .AsNoTracking()
            .Include(r => r.Store)
            .Include(r => r.Supplier)
            .Include(r => r.Purchase)
            .Include(r => r.Details)
                .ThenInclude(d => d.Product)
            .Include(r => r.Details)
                .ThenInclude(d => d.ProductVariant)
            .FirstOrDefaultAsync(r => r.IdPurchaseReturn == id && r.FkStore == storeId, ct);

        if (ret is null) return null;

        return new OutputPurchaseReturnSelect
        {
            IdPurchaseReturn = ret.IdPurchaseReturn,
            ReturnNumber = ret.ReturnNumber,
            FkStore = ret.FkStore,
            StoreName = ret.Store?.StoreName ?? string.Empty,
            FkPurchase = ret.FkPurchase,
            InvoiceNumber = ret.Purchase?.InvoiceNumber,
            FkSupplier = ret.FkSupplier,
            SupplierName = ret.Supplier?.SupplierName ?? string.Empty,
            ReturnDate = ret.ReturnDate,
            Status = ret.Status,
            TotalAmount = ret.TotalAmount,
            Reason = ret.Reason,
            Notes = ret.Notes,
            CreatedAt = ret.CreatedAt,
            PostedAt = ret.PostedAt,
            Lines = ret.Details.Select(d => new OutputPurchaseReturnLine
            {
                IdPurchaseReturnDetail = d.IdPurchaseReturnDetail,
                FkPurchaseDetail = d.FkPurchaseDetail,
                FkProduct = d.FkProduct,
                ProductName = d.Product?.ProductName ?? string.Empty,
                ProductCode = d.Product?.ProductCode,
                FkProductVariant = d.FkProductVariant,
                VariantName = d.ProductVariant?.VariantName,
                Quantity = d.Quantity,
                UnitCost = d.UnitCost,
                TaxAmount = d.TaxAmount,
                LineTotal = d.LineTotal
            }).ToList()
        };
    }

    public async Task<OutputPurchaseReturnList> ListReturnsAsync(InputPurchaseReturnList input, CancellationToken ct = default)
    {
        var query = _db.PurchaseReturns.AsNoTracking().Where(r => r.FkStore == input.FkStore);

        if (input.FkSupplier is > 0)
            query = query.Where(r => r.FkSupplier == input.FkSupplier);

        if (!string.IsNullOrWhiteSpace(input.Status))
            query = query.Where(r => r.Status == input.Status);

        if (input.From.HasValue)
            query = query.Where(r => r.ReturnDate >= input.From.Value);

        if (input.To.HasValue)
            query = query.Where(r => r.ReturnDate <= input.To.Value);

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var term = input.Search.Trim().ToLower();
            query = query.Where(r => r.ReturnNumber.ToLower().Contains(term)
                || (r.Purchase != null && r.Purchase.InvoiceNumber != null && r.Purchase.InvoiceNumber.ToLower().Contains(term))
                || (r.Supplier != null && r.Supplier.SupplierName.ToLower().Contains(term)));
        }

        var total = await query.CountAsync(ct);
        input.Page = Math.Max(1, input.Page);
        input.PageSize = Math.Clamp(input.PageSize, 1, 100);

        var items = await query
            .OrderByDescending(r => r.ReturnDate)
            .ThenByDescending(r => r.IdPurchaseReturn)
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .Select(r => new OutputPurchaseReturnListItem
            {
                IdPurchaseReturn = r.IdPurchaseReturn,
                ReturnNumber = r.ReturnNumber,
                FkPurchase = r.FkPurchase,
                SupplierName = r.Supplier != null ? r.Supplier.SupplierName : string.Empty,
                ReturnDate = r.ReturnDate,
                TotalAmount = r.TotalAmount,
                Status = r.Status,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(ct);

        return new OutputPurchaseReturnList
        {
            TotalCount = total,
            Page = input.Page,
            PageSize = input.PageSize,
            Items = items,
            Filter = input
        };
    }

    public async Task<int> CreateReturnAsync(InPurchaseReturnCreateView input, int storeId, int userId, CancellationToken ct = default)
    {
        var purchase = await _db.Purchases
            .Include(p => p.Details)
            .FirstOrDefaultAsync(p => p.IdPurchase == input.FkPurchase && p.FkStore == storeId, ct);

        if (purchase is null)
            throw new ResourceNotFoundException("Purchase not found.");

        if (purchase.Status != "Approved")
            throw new ConflictException("Goods can only be returned against an Approved purchase.");

        var previousReturns = await _db.PurchaseReturnDetails
            .Where(r => r.PurchaseReturn!.FkPurchase == purchase.IdPurchase && r.PurchaseReturn.Status == "Approved" && !r.PurchaseReturn.Cancelled)
            .GroupBy(r => r.FkPurchaseDetail)
            .Select(g => new { FkPurchaseDetail = g.Key, ReturnedQty = g.Sum(x => x.Quantity) })
            .ToDictionaryAsync(x => x.FkPurchaseDetail, x => x.ReturnedQty, ct);

        var now = DateTime.UtcNow;
        var nextSeq = await _db.PurchaseReturns.CountAsync(r => r.FkStore == storeId, ct) + 1;
        var returnNum = $"PR-{storeId}-{nextSeq:D5}";

        decimal totalReturnAmount = 0m;
        var ret = new PurchaseReturn
        {
            FkStore = storeId,
            FkPurchase = purchase.IdPurchase,
            FkSupplier = purchase.FkSupplier,
            ReturnNumber = returnNum,
            ReturnDate = input.ReturnDate,
            Status = input.MarkAsApproved ? "Approved" : "Pending",
            Reason = input.Reason,
            Notes = input.Notes,
            CreatedBy = userId,
            CreatedAt = now,
            PostedBy = input.MarkAsApproved ? userId : null,
            PostedAt = input.MarkAsApproved ? now : null
        };

        foreach (var line in input.Lines)
        {
            var originalLine = purchase.Details.FirstOrDefault(d => d.IdPurchaseDetail == line.FkPurchaseDetail);
            if (originalLine is null)
                throw new ValidationException("Lines", "One of the returned items was not part of the original purchase.");

            var alreadyReturned = previousReturns.GetValueOrDefault(line.FkPurchaseDetail, 0m);
            var eligibleQty = originalLine.Quantity - alreadyReturned;

            if (line.Quantity > eligibleQty)
                throw new ValidationException("Quantity", $"Cannot return {line.Quantity}. Eligible return quantity for this item is {eligibleQty}.");

            var unitCost = originalLine.UnitCost;
            var lineTotal = Math.Round(line.Quantity * unitCost, 2, MidpointRounding.AwayFromZero);
            totalReturnAmount += lineTotal;

            ret.Details.Add(new PurchaseReturnDetail
            {
                FkPurchaseDetail = line.FkPurchaseDetail,
                FkStore = storeId,
                FkProduct = originalLine.FkProduct,
                FkProductVariant = originalLine.FkProductVariant,
                Quantity = line.Quantity,
                UnitCost = unitCost,
                LineTotal = lineTotal
            });
        }

        ret.TotalAmount = totalReturnAmount;

        using var tx = await _db.Database.BeginTransactionAsync(ct);
        _db.PurchaseReturns.Add(ret);
        await _db.SaveChangesAsync(ct);

        if (input.MarkAsApproved)
        {
            foreach (var d in ret.Details)
            {
                await _stock.ApplyMovementAsync(new InputStockMovement
                {
                    FkStore = storeId,
                    FkProduct = d.FkProduct,
                    FkProductVariant = d.FkProductVariant,
                    MovementType = "return_out",
                    Quantity = d.Quantity,
                    ReferenceType = "purchase_return",
                    ReferenceId = ret.IdPurchaseReturn,
                    UnitCost = d.UnitCost,
                    CreatedBy = userId,
                    Notes = $"Purchase Return {ret.ReturnNumber}"
                }, ct);
            }

            _db.SupplierTransactions.Add(new SupplierTransaction
            {
                FkSupplier = ret.FkSupplier,
                FkStore = ret.FkStore,
                TransactionNumber = ret.ReturnNumber,
                TransactionDate = ret.ReturnDate,
                TransactionType = "Return",
                SourceType = "PurchaseReturn",
                SourceId = ret.IdPurchaseReturn,
                Amount = ret.TotalAmount,
                SignedAmount = +ret.TotalAmount, // Positive = supplier credit
                ReferenceNo = purchase.InvoiceNumber,
                Notes = $"Goods returned against Purchase PUR-{purchase.IdPurchase:D6}",
                CreatedBy = userId,
                CreatedAt = now
            });

            await _db.SaveChangesAsync(ct);
        }

        await tx.CommitAsync(ct);
        return ret.IdPurchaseReturn;
    }

    public async Task ApproveReturnAsync(int id, int storeId, int userId, CancellationToken ct = default)
    {
        var ret = await _db.PurchaseReturns
            .Include(r => r.Details)
            .Include(r => r.Purchase)
            .FirstOrDefaultAsync(r => r.IdPurchaseReturn == id && r.FkStore == storeId, ct);

        if (ret is null)
            throw new ResourceNotFoundException("Purchase return not found.");

        if (ret.Status == "Approved")
            throw new ConflictException("Return is already approved.");

        if (ret.Status != "Pending" || ret.Cancelled)
            throw new ConflictException("Only pending, non-cancelled returns can be approved.");

        var now = DateTime.UtcNow;
        using var tx = await _db.Database.BeginTransactionAsync(ct);

        ret.Status = "Approved";
        ret.PostedBy = userId;
        ret.PostedAt = now;

        foreach (var d in ret.Details)
        {
            await _stock.ApplyMovementAsync(new InputStockMovement
            {
                FkStore = storeId,
                FkProduct = d.FkProduct,
                FkProductVariant = d.FkProductVariant,
                MovementType = "return_out",
                Quantity = d.Quantity,
                ReferenceType = "purchase_return",
                ReferenceId = ret.IdPurchaseReturn,
                UnitCost = d.UnitCost,
                CreatedBy = userId,
                Notes = $"Purchase Return {ret.ReturnNumber}"
            }, ct);
        }

        _db.SupplierTransactions.Add(new SupplierTransaction
        {
            FkSupplier = ret.FkSupplier,
            FkStore = ret.FkStore,
            TransactionNumber = ret.ReturnNumber,
            TransactionDate = ret.ReturnDate,
            TransactionType = "Return",
            SourceType = "PurchaseReturn",
            SourceId = ret.IdPurchaseReturn,
            Amount = ret.TotalAmount,
            SignedAmount = +ret.TotalAmount, // Positive = supplier credit
            ReferenceNo = ret.Purchase?.InvoiceNumber,
            Notes = $"Goods returned against Purchase PUR-{ret.FkPurchase:D6}",
            CreatedBy = userId,
            CreatedAt = now
        });

        await _db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task CancelReturnAsync(int id, int storeId, string reason, CancellationToken ct = default)
    {
        var ret = await _db.PurchaseReturns.FirstOrDefaultAsync(r => r.IdPurchaseReturn == id && r.FkStore == storeId, ct);
        if (ret is null)
            throw new ResourceNotFoundException("Purchase return not found.");

        if (ret.Status == "Approved")
            throw new ConflictException("Approved returns cannot be cancelled.");

        if (ret.Cancelled || ret.Status == "Cancelled")
            throw new ConflictException("Return is already cancelled.");

        ret.Cancelled = true;
        ret.CancelledOn = DateTime.UtcNow;
        ret.CancelledReason = reason;
        ret.Status = "Cancelled";

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<OutputPurchaseListItem>> ListReturnSourcePurchasesAsync(int storeId, string? search = null, CancellationToken ct = default)
    {
        var query = _db.Purchases.AsNoTracking().Where(p => p.FkStore == storeId && p.Status == "Approved" && !p.Cancelled);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p => (p.InvoiceNumber != null && p.InvoiceNumber.ToLower().Contains(term))
                || (p.Supplier != null && p.Supplier.SupplierName.ToLower().Contains(term)));
        }

        return await query
            .OrderByDescending(p => p.PurchaseDate)
            .Take(50)
            .Select(p => new OutputPurchaseListItem
            {
                IdPurchase = p.IdPurchase,
                FkStore = p.FkStore,
                FkSupplier = p.FkSupplier,
                SupplierName = p.Supplier != null ? p.Supplier.SupplierName : string.Empty,
                PurchaseDate = p.PurchaseDate,
                InvoiceNumber = p.InvoiceNumber,
                TotalAmount = p.TotalAmount,
                Status = p.Status
            })
            .ToListAsync(ct);
    }

    #endregion

    #region Purchase Orders

    public async Task<OutputPurchaseOrderSelect?> GetOrderAsync(int id, int storeId, CancellationToken ct = default)
    {
        var po = await _db.PurchaseOrders
            .AsNoTracking()
            .Include(o => o.Store)
            .Include(o => o.Supplier)
            .Include(o => o.Creator)
            .Include(o => o.Details)
                .ThenInclude(d => d.Product)
            .Include(o => o.Details)
                .ThenInclude(d => d.ProductVariant)
            .FirstOrDefaultAsync(o => o.IdPurchaseOrder == id && o.FkStore == storeId, ct);

        if (po is null) return null;

        return new OutputPurchaseOrderSelect
        {
            IdPurchaseOrder = po.IdPurchaseOrder,
            OrderNumber = po.OrderNumber,
            FkStore = po.FkStore,
            StoreName = po.Store?.StoreName ?? string.Empty,
            FkSupplier = po.FkSupplier,
            SupplierName = po.Supplier?.SupplierName,
            OrderDate = po.OrderDate,
            Status = po.Status,
            Notes = po.Notes,
            CreatedAt = po.CreatedAt,
            CreatorName = po.Creator?.FullName ?? po.Creator?.UserName,
            Lines = po.Details.Select(d => new OutputPurchaseOrderLine
            {
                IdPurchaseOrderDetail = d.IdPurchaseOrderDetail,
                FkProduct = d.FkProduct,
                ProductName = d.Product?.ProductName ?? string.Empty,
                ProductCode = d.Product?.ProductCode,
                FkProductVariant = d.FkProductVariant,
                VariantName = d.ProductVariant?.VariantName,
                Quantity = d.Quantity,
                UnitCost = d.UnitCost,
                Notes = d.Notes
            }).ToList()
        };
    }

    public async Task<OutputPurchaseOrderList> ListOrdersAsync(InputPurchaseOrderList input, CancellationToken ct = default)
    {
        var query = _db.PurchaseOrders.AsNoTracking().Where(o => o.FkStore == input.FkStore);

        if (input.TodayOnly)
            query = query.Where(o => o.OrderDate == DateOnly.FromDateTime(DateTime.Today));

        if (input.FkSupplier is > 0)
            query = query.Where(o => o.FkSupplier == input.FkSupplier);

        if (!string.IsNullOrWhiteSpace(input.Status))
            query = query.Where(o => o.Status == input.Status);

        if (input.From.HasValue)
            query = query.Where(o => o.OrderDate >= input.From.Value);

        if (input.To.HasValue)
            query = query.Where(o => o.OrderDate <= input.To.Value);

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var term = input.Search.Trim().ToLower();
            query = query.Where(o => o.OrderNumber.ToLower().Contains(term)
                || (o.Supplier != null && o.Supplier.SupplierName.ToLower().Contains(term)));
        }

        var total = await query.CountAsync(ct);
        input.Page = Math.Max(1, input.Page);
        input.PageSize = Math.Clamp(input.PageSize, 1, 100);

        var items = await query
            .OrderByDescending(o => o.OrderDate)
            .ThenByDescending(o => o.IdPurchaseOrder)
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .Select(o => new OutputPurchaseOrderListItem
            {
                IdPurchaseOrder = o.IdPurchaseOrder,
                OrderNumber = o.OrderNumber,
                SupplierName = o.Supplier != null ? o.Supplier.SupplierName : "Any / General",
                OrderDate = o.OrderDate,
                LineCount = o.Details.Count,
                TotalQuantity = o.Details.Sum(d => d.Quantity),
                Status = o.Status,
                CreatedAt = o.CreatedAt
            })
            .ToListAsync(ct);

        return new OutputPurchaseOrderList
        {
            TotalCount = total,
            Page = input.Page,
            PageSize = input.PageSize,
            Items = items,
            Filter = input
        };
    }

    public async Task<int> SaveOrderAsync(InPurchaseOrderView input, int storeId, int userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        if (input.IdPurchaseOrder > 0)
        {
            var existing = await _db.PurchaseOrders
                .Include(o => o.Details)
                .FirstOrDefaultAsync(o => o.IdPurchaseOrder == input.IdPurchaseOrder && o.FkStore == storeId, ct);

            if (existing is null)
                throw new ResourceNotFoundException("Purchase Order not found.");

            existing.OrderDate = input.OrderDate;
            existing.FkSupplier = input.FkSupplier > 0 ? input.FkSupplier : null;
            existing.Notes = input.Notes;
            existing.UpdatedAt = now;

            _db.PurchaseOrderDetails.RemoveRange(existing.Details);
            foreach (var l in input.Lines)
            {
                existing.Details.Add(new PurchaseOrderDetail
                {
                    FkPurchaseOrder = existing.IdPurchaseOrder,
                    FkProduct = l.FkProduct,
                    FkProductVariant = l.FkProductVariant,
                    Quantity = l.Quantity,
                    UnitCost = l.UnitCost,
                    Notes = l.Notes,
                    CreatedAt = now
                });
            }

            await _db.SaveChangesAsync(ct);
            return existing.IdPurchaseOrder;
        }
        else
        {
            var nextSeq = await _db.PurchaseOrders.CountAsync(o => o.FkStore == storeId, ct) + 1;
            var orderNum = $"PO-{storeId}-{nextSeq:D5}";

            var po = new PurchaseOrder
            {
                FkStore = storeId,
                OrderNumber = orderNum,
                OrderDate = input.OrderDate,
                FkSupplier = input.FkSupplier > 0 ? input.FkSupplier : null,
                Status = "Open",
                Notes = input.Notes,
                CreatedBy = userId,
                CreatedAt = now
            };

            foreach (var l in input.Lines)
            {
                po.Details.Add(new PurchaseOrderDetail
                {
                    FkProduct = l.FkProduct,
                    FkProductVariant = l.FkProductVariant,
                    Quantity = l.Quantity,
                    UnitCost = l.UnitCost,
                    Notes = l.Notes,
                    CreatedAt = now
                });
            }

            _db.PurchaseOrders.Add(po);
            await _db.SaveChangesAsync(ct);
            return po.IdPurchaseOrder;
        }
    }

    public async Task DeleteOrderAsync(int id, int storeId, CancellationToken ct = default)
    {
        var po = await _db.PurchaseOrders.FirstOrDefaultAsync(o => o.IdPurchaseOrder == id && o.FkStore == storeId, ct);
        if (po is null)
            throw new ResourceNotFoundException("Purchase Order not found.");

        _db.PurchaseOrders.Remove(po);
        await _db.SaveChangesAsync(ct);
    }

    #endregion

    #region Supplier Financials & Balance

    public async Task<OutputSupplierBalance> GetSupplierBalanceAsync(int supplierId, int storeId, CancellationToken ct = default)
    {
        var supplier = await _db.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.IdSupplier == supplierId, ct);
        if (supplier is null)
            throw new ResourceNotFoundException("Supplier not found.");

        // Positive = supplier owes our business money / credit; Negative = our business owes the supplier (payable).
        var currentBalance = await _db.SupplierTransactions
            .Where(t => t.FkSupplier == supplierId)
            .SumAsync(t => (decimal?)t.SignedAmount, ct) ?? 0m;

        return new OutputSupplierBalance
        {
            IdSupplier = supplier.IdSupplier,
            SupplierName = supplier.SupplierName,
            CurrentBalance = currentBalance
        };
    }

    public async Task<OutputSupplierStatement> GetSupplierStatementAsync(int supplierId, int storeId, DateOnly? from = null, DateOnly? to = null, CancellationToken ct = default)
    {
        var supplier = await _db.Suppliers.AsNoTracking().FirstOrDefaultAsync(s => s.IdSupplier == supplierId, ct);
        if (supplier is null)
            throw new ResourceNotFoundException("Supplier not found.");

        decimal openingBalance = 0m;
        if (from.HasValue)
        {
            openingBalance = await _db.SupplierTransactions
                .Where(t => t.FkSupplier == supplierId && t.TransactionDate < from.Value)
                .SumAsync(t => (decimal?)t.SignedAmount, ct) ?? 0m;
        }

        var query = _db.SupplierTransactions.AsNoTracking().Where(t => t.FkSupplier == supplierId);
        if (from.HasValue) query = query.Where(t => t.TransactionDate >= from.Value);
        if (to.HasValue) query = query.Where(t => t.TransactionDate <= to.Value);

        var rows = await query
            .OrderBy(t => t.TransactionDate)
            .ThenBy(t => t.IdSupplierTransaction)
            .ToListAsync(ct);

        decimal running = openingBalance;
        var entries = new List<OutputSupplierStatementRow>(rows.Count);
        foreach (var r in rows)
        {
            running += r.SignedAmount;
            entries.Add(new OutputSupplierStatementRow
            {
                Id = r.IdSupplierTransaction,
                TransactionDate = r.TransactionDate,
                TransactionNumber = r.TransactionNumber,
                TransactionType = r.TransactionType,
                ReferenceNo = r.ReferenceNo,
                Notes = r.Notes,
                Debit = r.SignedAmount < 0 ? -r.SignedAmount : 0m,
                Credit = r.SignedAmount > 0 ? r.SignedAmount : 0m,
                SignedAmount = r.SignedAmount,
                RunningBalance = running
            });
        }

        var currentTotal = await _db.SupplierTransactions
            .Where(t => t.FkSupplier == supplierId)
            .SumAsync(t => (decimal?)t.SignedAmount, ct) ?? 0m;

        return new OutputSupplierStatement
        {
            IdSupplier = supplier.IdSupplier,
            SupplierName = supplier.SupplierName,
            SupplierCode = supplier.SupplierCode,
            From = from,
            To = to,
            OpeningBalance = openingBalance,
            ClosingBalance = running,
            CurrentBalance = currentTotal,
            Entries = entries
        };
    }

    public async Task<OutputSupplierTransactionList> ListSupplierTransactionsAsync(InputSupplierTransactionList input, CancellationToken ct = default)
    {
        var query = _db.SupplierTransactions.AsNoTracking();

        if (input.FkStore is > 0)
            query = query.Where(t => t.FkStore == input.FkStore);

        if (input.FkSupplier is > 0)
            query = query.Where(t => t.FkSupplier == input.FkSupplier);

        if (input.From.HasValue)
            query = query.Where(t => t.TransactionDate >= input.From.Value);

        if (input.To.HasValue)
            query = query.Where(t => t.TransactionDate <= input.To.Value);

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var term = input.Search.Trim().ToLower();
            query = query.Where(t => t.TransactionNumber.ToLower().Contains(term)
                || (t.ReferenceNo != null && t.ReferenceNo.ToLower().Contains(term))
                || (t.Supplier != null && t.Supplier.SupplierName.ToLower().Contains(term)));
        }

        var total = await query.CountAsync(ct);
        input.Page = Math.Max(1, input.Page);
        input.PageSize = Math.Clamp(input.PageSize, 1, 100);

        var payments = await query.Where(t => t.TransactionType == "Payment").SumAsync(t => (decimal?)t.Amount, ct) ?? 0m;
        var refunds = await query.Where(t => t.TransactionType == "Refund").SumAsync(t => (decimal?)t.Amount, ct) ?? 0m;

        var items = await query
            .OrderByDescending(t => t.TransactionDate)
            .ThenByDescending(t => t.IdSupplierTransaction)
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .Select(t => new OutputSupplierTransactionItem
            {
                Id = t.IdSupplierTransaction,
                TransactionNumber = t.TransactionNumber,
                FkSupplier = t.FkSupplier,
                SupplierName = t.Supplier != null ? t.Supplier.SupplierName : string.Empty,
                TransactionDate = t.TransactionDate,
                TransactionType = t.TransactionType,
                Amount = t.Amount,
                SignedAmount = t.SignedAmount,
                PaymentMethod = t.PaymentMethod,
                ReferenceNo = t.ReferenceNo,
                Notes = t.Notes,
                IsReversed = t.FkReversalOf != null
            })
            .ToListAsync(ct);

        return new OutputSupplierTransactionList
        {
            TotalCount = total,
            Page = input.Page,
            PageSize = input.PageSize,
            TotalPayments = payments,
            TotalRefunds = refunds,
            Items = items,
            Filter = input
        };
    }

    public async Task<int> RecordSupplierTransactionAsync(InSupplierPaymentView input, int storeId, int userId, CancellationToken ct = default)
    {
        var supplier = await _db.Suppliers.FirstOrDefaultAsync(s => s.IdSupplier == input.FkSupplier && s.IsActive && !s.Cancelled, ct);
        if (supplier is null)
            throw new ValidationException("Supplier", "Selected supplier does not exist or is inactive.");

        var now = DateTime.UtcNow;
        var nextSeq = await _db.SupplierTransactions.CountAsync(t => t.FkStore == storeId, ct) + 1;
        var prefix = input.TransactionType == "Refund" ? "SREF" : "SPAY";
        var txNum = $"{prefix}-{storeId}-{nextSeq:D5}";

        decimal signed = input.TransactionType == "Payment" ? +input.Amount
                       : input.TransactionType == "Refund" ? -input.Amount
                       : input.TransactionType == "Opening" ? +input.Amount
                       : +input.Amount;

        var st = new SupplierTransaction
        {
            FkSupplier = input.FkSupplier,
            FkStore = storeId,
            TransactionNumber = txNum,
            TransactionDate = input.TransactionDate,
            TransactionType = input.TransactionType,
            SourceType = "Payment",
            Amount = input.Amount,
            SignedAmount = signed,
            PaymentMethod = input.PaymentMethod,
            ReferenceNo = input.ReferenceNo,
            Notes = input.Notes,
            CreatedBy = userId,
            CreatedAt = now
        };

        _db.SupplierTransactions.Add(st);
        await _db.SaveChangesAsync(ct);
        return st.IdSupplierTransaction;
    }

    public async Task ReverseSupplierTransactionAsync(int id, int storeId, int userId, string reason, CancellationToken ct = default)
    {
        var original = await _db.SupplierTransactions.FirstOrDefaultAsync(t => t.IdSupplierTransaction == id && t.FkStore == storeId, ct);
        if (original is null)
            throw new ResourceNotFoundException("Transaction not found.");

        if (original.FkReversalOf.HasValue || original.TransactionType == "Reversal")
            throw new ConflictException("A reversed transaction or reversal cannot be reversed.");

        var alreadyReversed = await _db.SupplierTransactions.AnyAsync(t => t.FkReversalOf == id, ct);
        if (alreadyReversed)
            throw new ConflictException("This transaction has already been reversed.");

        var now = DateTime.UtcNow;
        var nextSeq = await _db.SupplierTransactions.CountAsync(t => t.FkStore == storeId, ct) + 1;
        var txNum = $"SREV-{storeId}-{nextSeq:D5}";

        var reversal = new SupplierTransaction
        {
            FkSupplier = original.FkSupplier,
            FkStore = storeId,
            TransactionNumber = txNum,
            TransactionDate = DateOnly.FromDateTime(DateTime.Today),
            TransactionType = "Reversal",
            SourceType = original.SourceType,
            SourceId = original.SourceId,
            Amount = original.Amount,
            SignedAmount = -original.SignedAmount, // Inverted sign to cancel original financial impact
            PaymentMethod = original.PaymentMethod,
            ReferenceNo = original.TransactionNumber,
            Notes = $"Reversal of {original.TransactionNumber}: {reason}",
            FkReversalOf = original.IdSupplierTransaction,
            CreatedBy = userId,
            CreatedAt = now
        };

        _db.SupplierTransactions.Add(reversal);
        await _db.SaveChangesAsync(ct);
    }

    #endregion
}
