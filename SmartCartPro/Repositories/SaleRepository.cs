using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Exceptions;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Repositories;

public class SaleRepository : ISaleRepository
{
    private static readonly HashSet<string> DiscreteUnits = new(StringComparer.OrdinalIgnoreCase)
    {
        "PCS", "PC", "EA", "EACH", "NOS", "NO", "UNIT", "UNT", "BOX", "PKT", "PACK", "SET", "PAIR"
    };

    private readonly SmartCartDbContext _db;
    private readonly IStockRepository _stock;
    private readonly IDocumentNumberService _docNum;

    public SaleRepository(SmartCartDbContext db, IStockRepository stock, IDocumentNumberService docNum)
    {
        _db = db;
        _stock = stock;
        _docNum = docNum;
    }

    // ═══════════════════════════════════════════════════════════════
    // POS CONTEXT / GET / LIST
    // ═══════════════════════════════════════════════════════════════

    public async Task<OutputPosSelect?> GetPosContextAsync(InputPosSelect input, CancellationToken ct = default)
    {
        var store = await _db.Stores.AsNoTracking()
            .Where(s => s.IdStore == input.FkStore && !s.Cancelled)
            .Select(s => new
            {
                s.IdStore,
                s.StoreName,
                CurrencyCode = s.Company != null ? s.Company.CurrencyCode : "INR"
            })
            .FirstOrDefaultAsync(ct);

        if (store is null) return null;

        return new OutputPosSelect
        {
            FkStore = store.IdStore,
            StoreName = store.StoreName,
            CurrencyCode = string.IsNullOrWhiteSpace(store.CurrencyCode) ? "INR" : store.CurrencyCode,
            OpenSession = await GetOpenSessionAsync(new InputCashSessionSelect { FkStore = input.FkStore }, ct)
        };
    }

    public async Task<OutputSaleSelect?> GetSaleAsync(InputSaleSelect input, CancellationToken ct = default)
    {
        if (input.FkStore <= 0 || input.IdSale <= 0)
            return null;

        var q = _db.Sales.AsNoTracking()
            .Include(x => x.Store)!.ThenInclude(s => s!.Company)
            .Include(x => x.Customer)
            .Include(x => x.Details).ThenInclude(d => d.Product)
            .Include(x => x.Payments)
            .Where(x => x.IdSale == input.IdSale && x.FkStore == input.FkStore);

        var sale = await q.FirstOrDefaultAsync(ct);
        if (sale is null) return null;

        var variantIds = sale.Details
            .Where(d => d.FkProductVariant is > 0)
            .Select(d => d.FkProductVariant!.Value)
            .Distinct()
            .ToList();

        var variants = variantIds.Count == 0
            ? new Dictionary<int, (string Name, string? Sku)>()
            : await _db.ProductVariants.AsNoTracking()
                .Where(v => variantIds.Contains(v.IdProductVariant))
                .ToDictionaryAsync(v => v.IdProductVariant, v => (Name: v.VariantName, Sku: v.SkuCode), ct);

        var hasReturns = await _db.SalesReturns.AsNoTracking()
            .AnyAsync(r => r.FkSale == sale.IdSale && !r.Cancelled, ct);

        var cashier = sale.CreatedBy is int userId
            ? await _db.Users.AsNoTracking()
                .Where(u => u.IdUser == userId)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync(ct)
            : null;

        var store = sale.Store;
        var company = store?.Company;

        return new OutputSaleSelect
        {
            IdSale = sale.IdSale,
            FkStore = sale.FkStore,
            StoreName = store?.StoreName ?? string.Empty,
            StorePhone = store?.Phone,
            StoreEmail = store?.Email,
            StoreAddress = FormatAddress(store?.AddressLine1, store?.AddressLine2, store?.City, store?.State, store?.PostalCode, store?.Country),
            CompanyName = company?.CompanyName,
            CompanyLegalName = company?.LegalName,
            CompanyTaxNumber = company?.TaxNumber,
            CompanyPhone = company?.Phone,
            CompanyEmail = company?.Email,
            CompanyAddress = FormatAddress(company?.AddressLine1, company?.AddressLine2, company?.City, company?.State, company?.PostalCode, company?.Country),
            CurrencyCode = string.IsNullOrWhiteSpace(company?.CurrencyCode) ? "INR" : company!.CurrencyCode,
            FkCustomer = sale.FkCustomer,
            CustomerName = sale.Customer?.FullName,
            CustomerPhone = sale.Customer?.Phone,
            CustomerEmail = sale.Customer?.Email,
            InvoiceNumber = sale.InvoiceNumber,
            SaleDate = sale.SaleDate,
            SubTotal = sale.SubTotal,
            TaxAmount = sale.TaxAmount,
            DiscountAmount = sale.DiscountAmount,
            TotalAmount = sale.TotalAmount,
            PaidAmount = sale.PaidAmount,
            ChangeGiven = sale.ChangeGiven,
            Status = sale.Status,
            Notes = sale.Notes,
            CashierName = cashier,
            Cancelled = sale.Cancelled,
            CancelledOn = sale.CancelledOn,
            CancelledReason = sale.CancelledReason,
            HasReturns = hasReturns,
            Lines = sale.Details.Select(d =>
            {
                variants.TryGetValue(d.FkProductVariant ?? 0, out var variant);
                return new OutputSaleLine
                {
                    IdSaleDetail = d.IdSaleDetail,
                    FkProduct = d.FkProduct,
                    ProductName = d.Product?.ProductName ?? string.Empty,
                    ProductCode = d.Product?.ProductCode,
                    FkProductVariant = d.FkProductVariant,
                    VariantName = variant.Name,
                    Sku = variant.Sku ?? d.Product?.Sku,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    TaxAmount = d.TaxAmount,
                    DiscountAmount = d.DiscountAmount,
                    LineTotal = d.LineTotal
                };
            }).ToList(),
            Payments = sale.Payments.Where(p => !p.Cancelled).Select(p => new OutputSalePayment
            {
                IdPayment = p.IdPayment,
                PaymentMethod = p.PaymentMethod,
                Amount = p.Amount,
                ReferenceNo = p.ReferenceNo,
                PaidAt = p.PaidAt
            }).ToList()
        };
    }

    public async Task<OutputSaleList> ListSalesAsync(InputSaleList input, CancellationToken ct = default)
    {
        var q = _db.Sales.AsNoTracking().Where(x => x.FkStore == input.FkStore);

        var customerId = input.CustomerId ?? input.FkCustomer;
        if (customerId is > 0)
            q = q.Where(x => x.FkCustomer == customerId);

        if (!string.IsNullOrWhiteSpace(input.Status))
            q = q.Where(x => x.Status == input.Status);

        if (!string.IsNullOrWhiteSpace(input.PaymentMethod))
        {
            var method = input.PaymentMethod;
            q = q.Where(x => x.Payments.Any(p => !p.Cancelled && p.PaymentMethod == method));
        }

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var term = input.Search.Trim().ToLower();
            q = q.Where(x => x.InvoiceNumber.ToLower().Contains(term)
                || (x.Customer != null && x.Customer.FullName.ToLower().Contains(term))
                || (x.Customer != null && x.Customer.Phone != null && x.Customer.Phone.ToLower().Contains(term)));
        }

        if (input.FromUtc.HasValue)
            q = q.Where(x => x.SaleDate >= input.FromUtc.Value);
        if (input.ToExclusiveUtc.HasValue)
            q = q.Where(x => x.SaleDate < input.ToExclusiveUtc.Value);

        q = (input.SortBy, input.SortDesc) switch
        {
            ("invoicenumber", true) => q.OrderByDescending(x => x.InvoiceNumber),
            ("invoicenumber", false) => q.OrderBy(x => x.InvoiceNumber),
            ("customer", true) => q.OrderByDescending(x => x.Customer != null ? x.Customer.FullName : string.Empty),
            ("customer", false) => q.OrderBy(x => x.Customer != null ? x.Customer.FullName : string.Empty),
            ("totalamount", true) => q.OrderByDescending(x => x.TotalAmount),
            ("totalamount", false) => q.OrderBy(x => x.TotalAmount),
            ("paidamount", true) => q.OrderByDescending(x => x.PaidAmount),
            ("paidamount", false) => q.OrderBy(x => x.PaidAmount),
            ("status", true) => q.OrderByDescending(x => x.Status),
            ("status", false) => q.OrderBy(x => x.Status),
            ("id", true) => q.OrderByDescending(x => x.IdSale),
            ("id", false) => q.OrderBy(x => x.IdSale),
            (_, true) => q.OrderByDescending(x => x.SaleDate),
            _ => q.OrderBy(x => x.SaleDate)
        };

        var total = await q.CountAsync(ct);
        var items = await q
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .Select(x => new OutputSaleListItem
            {
                IdSale = x.IdSale,
                InvoiceNumber = x.InvoiceNumber,
                SaleDate = x.SaleDate,
                CustomerName = x.Customer != null ? x.Customer.FullName : null,
                SubTotal = x.SubTotal,
                TaxAmount = x.TaxAmount,
                DiscountAmount = x.DiscountAmount,
                TotalAmount = x.TotalAmount,
                PaidAmount = x.PaidAmount,
                Status = x.Status,
                PaymentMethod = x.Payments.Where(p => !p.Cancelled).Select(p => p.PaymentMethod).FirstOrDefault()
            })
            .ToListAsync(ct);

        return new OutputSaleList
        {
            TotalCount = total,
            Page = input.Page,
            PageSize = input.PageSize,
            PageSubTotal = items.Sum(i => i.SubTotal),
            PageTaxAmount = items.Sum(i => i.TaxAmount),
            PageDiscountAmount = items.Sum(i => i.DiscountAmount),
            PageTotalAmount = items.Sum(i => i.TotalAmount),
            PagePaidAmount = items.Sum(i => i.PaidAmount),
            Items = items
        };
    }

    // ═══════════════════════════════════════════════════════════════
    // PRODUCT LOOKUPS
    // ═══════════════════════════════════════════════════════════════

    public async Task<IReadOnlyList<OutputSaleProductSelect>> SearchProductsAsync(
        InputSaleProductSearch input, CancellationToken ct = default)
    {
        var term = (input.Term ?? string.Empty).Trim();
        if (term.Length == 0)
            return Array.Empty<OutputSaleProductSelect>();

        var take = input.Take is > 0 and <= 20 ? input.Take : 20;
        var like = term.ToLower();

        var variantRows = await (
            from v in _db.ProductVariants.AsNoTracking()
            join p in _db.Products.AsNoTracking() on v.FkProduct equals p.IdProduct
            where !v.Cancelled && v.IsActive && !p.Cancelled && p.IsActive
                && (p.ProductName.ToLower().Contains(like)
                    || p.ProductCode.ToLower().Contains(like)
                    || (p.Sku != null && p.Sku.ToLower().Contains(like))
                    || (p.Barcode != null && p.Barcode.ToLower().Contains(like))
                    || v.VariantName.ToLower().Contains(like)
                    || (v.SkuCode != null && v.SkuCode.ToLower().Contains(like))
                    || (v.Barcode != null && v.Barcode.ToLower().Contains(like)))
            orderby p.ProductName, v.VariantName
            select new { v, p }
        ).Take(take).ToListAsync(ct);

        var usedProducts = variantRows.Select(r => r.p.IdProduct).ToHashSet();

        var productRows = await _db.Products.AsNoTracking()
            .Where(p => !p.Cancelled && p.IsActive
                && !usedProducts.Contains(p.IdProduct)
                && !_db.ProductVariants.Any(v => v.FkProduct == p.IdProduct && !v.Cancelled && v.IsActive)
                && (p.ProductName.ToLower().Contains(like)
                    || p.ProductCode.ToLower().Contains(like)
                    || (p.Sku != null && p.Sku.ToLower().Contains(like))
                    || (p.Barcode != null && p.Barcode.ToLower().Contains(like))))
            .OrderBy(p => p.ProductName)
            .Take(take)
            .ToListAsync(ct);

        var picks = new List<OutputSaleProductSelect>(take);
        foreach (var row in variantRows)
        {
            if (picks.Count >= take) break;
            picks.Add(await ToPosProductAsync(row.p, row.v, input.FkStore, ct));
        }

        foreach (var product in productRows)
        {
            if (picks.Count >= take) break;
            picks.Add(await ToPosProductAsync(product, null, input.FkStore, ct));
        }

        return picks;
    }

    public async Task<OutputSaleProductSelect?> GetProductByBarcodeAsync(
        InputSaleProductBarcode input, CancellationToken ct = default)
    {
        var barcode = (input.Barcode ?? string.Empty).Trim();
        if (barcode.Length == 0)
            return null;

        var key = barcode.ToLower();

        var variant = await (
            from v in _db.ProductVariants.AsNoTracking()
            join p in _db.Products.AsNoTracking() on v.FkProduct equals p.IdProduct
            where !v.Cancelled && v.IsActive && !p.Cancelled && p.IsActive
                && v.Barcode != null && v.Barcode.ToLower() == key
            select new { v, p }
        ).FirstOrDefaultAsync(ct);

        if (variant is not null)
            return await ToPosProductAsync(variant.p, variant.v, input.FkStore, ct);

        var product = await _db.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => !p.Cancelled && p.IsActive
                && p.Barcode != null && p.Barcode.ToLower() == key, ct);

        if (product is null)
            return null;

        var firstVariant = await _db.ProductVariants.AsNoTracking()
            .Where(v => v.FkProduct == product.IdProduct && !v.Cancelled && v.IsActive)
            .OrderBy(v => v.IdProductVariant)
            .FirstOrDefaultAsync(ct);

        return await ToPosProductAsync(product, firstVariant, input.FkStore, ct);
    }

    public async Task<IReadOnlyList<OutputSaleProductSelect>> GetProductsByKeysAsync(
        InputSaleProductKeys input, CancellationToken ct = default)
    {
        var result = new List<OutputSaleProductSelect>();
        if (input.Keys.Count == 0)
            return result;

        var productIds = input.Keys.Select(k => k.FkProduct).Where(id => id > 0).Distinct().ToList();
        var products = await _db.Products.AsNoTracking()
            .Where(p => productIds.Contains(p.IdProduct) && !p.Cancelled && p.IsActive)
            .ToListAsync(ct);

        var variantIds = input.Keys
            .Select(k => k.FkProductVariant)
            .Where(id => id is > 0)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var variants = variantIds.Count == 0
            ? new List<ProductVariant>()
            : await _db.ProductVariants.AsNoTracking()
                .Where(v => variantIds.Contains(v.IdProductVariant) && !v.Cancelled && v.IsActive)
                .ToListAsync(ct);

        foreach (var key in input.Keys)
        {
            var product = products.FirstOrDefault(p => p.IdProduct == key.FkProduct);
            if (product is null) continue;

            ProductVariant? variant = null;
            if (key.FkProductVariant is > 0)
            {
                variant = variants.FirstOrDefault(v =>
                    v.IdProductVariant == key.FkProductVariant && v.FkProduct == key.FkProduct);
                if (variant is null) continue;
            }

            result.Add(await ToPosProductAsync(product, variant, input.FkStore, ct));
        }

        return result;
    }

    public async Task<decimal> GetTaxRateByCodeAsync(string taxCode, CancellationToken ct = default)
    {
        var code = taxCode.Trim().ToLower();
        return await _db.Taxes.AsNoTracking()
            .Where(t => !t.Cancelled && t.IsActive && t.TaxCode.ToLower() == code)
            .Select(t => t.TaxRate)
            .FirstOrDefaultAsync(ct);
    }

    // ═══════════════════════════════════════════════════════════════
    // CREATE SALE
    // ═══════════════════════════════════════════════════════════════

    public async Task<OutputSaleCreate> CreateSaleAsync(InputSaleCreate input, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(input.ClientRef))
        {
            var existing = await FindByClientRefAsync(input.FkStore, input.ClientRef, ct);
            if (existing is not null)
            {
                return new OutputSaleCreate
                {
                    IdSale = existing.IdSale,
                    InvoiceNumber = existing.InvoiceNumber,
                    TotalAmount = existing.TotalAmount,
                    PaidAmount = existing.PaidAmount,
                    ChangeGiven = existing.ChangeGiven
                };
            }
        }

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            if (!string.IsNullOrWhiteSpace(input.ClientRef))
            {
                var raced = await FindByClientRefAsync(input.FkStore, input.ClientRef, ct);
                if (raced is not null)
                {
                    await tx.RollbackAsync(ct);
                    return new OutputSaleCreate
                    {
                        IdSale = raced.IdSale,
                        InvoiceNumber = raced.InvoiceNumber,
                        TotalAmount = raced.TotalAmount,
                        PaidAmount = raced.PaidAmount,
                        ChangeGiven = raced.ChangeGiven
                    };
                }
            }

            var invoiceNumber = await _docNum.NextAsync(input.FkStore, "SALE", ct);

            var sale = new Sale
            {
                FkStore = input.FkStore,
                FkCustomer = input.FkCustomer,
                FkCashSession = input.FkCashSession,
                InvoiceNumber = invoiceNumber,
                SaleDate = DateTime.UtcNow,
                SubTotal = input.SubTotal,
                TaxAmount = input.TaxAmount,
                DiscountAmount = input.DiscountAmount,
                TotalAmount = input.TotalAmount,
                PaidAmount = input.PaidAmount,
                ChangeGiven = input.ChangeGiven,
                Status = "completed",
                Notes = BuildNotes(input.ClientRef, input.Notes),
                CreatedBy = input.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };

            _db.Sales.Add(sale);
            await _db.SaveChangesAsync(ct);

            foreach (var line in input.Lines)
            {
                _db.SaleDetails.Add(new SaleDetail
                {
                    FkSale = sale.IdSale,
                    FkStore = input.FkStore,
                    FkProduct = line.FkProduct,
                    FkProductVariant = line.FkProductVariant,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    TaxAmount = line.TaxAmount,
                    DiscountAmount = line.DiscountAmount,
                    LineTotal = line.LineTotal,
                    CreatedAt = DateTime.UtcNow
                });
            }

            foreach (var pay in input.Payments)
            {
                _db.Payments.Add(new Payment
                {
                    FkStore = input.FkStore,
                    FkSale = sale.IdSale,
                    FkCashSession = input.FkCashSession,
                    PaymentMethod = pay.PaymentMethod,
                    Amount = pay.Amount,
                    ReferenceNo = pay.ReferenceNo,
                    PaidAt = DateTime.UtcNow,
                    CreatedBy = input.CreatedBy,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _db.SaveChangesAsync(ct);

            foreach (var line in input.Lines)
            {
                var stockResult = await _stock.ApplyMovementAsync(new InputStockMovement
                {
                    FkStore = input.FkStore,
                    FkProduct = line.FkProduct,
                    FkProductVariant = line.FkProductVariant,
                    MovementType = "sale",
                    Quantity = line.Quantity,
                    ReferenceType = "sale",
                    ReferenceId = sale.IdSale,
                    Notes = $"Sale {invoiceNumber}",
                    CreatedBy = input.CreatedBy
                }, ct);

                if (!stockResult.Ok)
                {
                    await tx.RollbackAsync(ct);
                    return new OutputSaleCreate
                    {
                        ResponseCode = stockResult.ResponseCode,
                        ResponseMsg = stockResult.ResponseMsg,
                        StatusCode = stockResult.StatusCode == 0 ? 409 : stockResult.StatusCode
                    };
                }
            }

            await tx.CommitAsync(ct);

            return new OutputSaleCreate
            {
                IdSale = sale.IdSale,
                InvoiceNumber = sale.InvoiceNumber,
                TotalAmount = sale.TotalAmount,
                PaidAmount = sale.PaidAmount,
                ChangeGiven = sale.ChangeGiven
            };
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // CANCEL
    // ═══════════════════════════════════════════════════════════════

    public async Task<OutputSaleCancel> CancelSaleAsync(InputSaleCancel input, CancellationToken ct = default)
    {
        var sale = await _db.Sales
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.IdSale == input.IdSale && x.FkStore == input.FkStore, ct);

        if (sale is null)
            return new OutputSaleCancel { ResponseCode = 1, ResponseMsg = "Sale not found.", StatusCode = 404 };

        if (sale.Cancelled || sale.Status == "cancelled")
            return new OutputSaleCancel { ResponseCode = 2, ResponseMsg = "This sale is already cancelled.", StatusCode = 409 };

        if (!string.Equals(sale.Status, "completed", StringComparison.OrdinalIgnoreCase))
            return new OutputSaleCancel { ResponseCode = 3, ResponseMsg = "Only a completed sale can be cancelled.", StatusCode = 409 };

        var hasReturns = await _db.SalesReturns.AnyAsync(r => r.FkSale == sale.IdSale && !r.Cancelled, ct);
        if (hasReturns)
            return new OutputSaleCancel { ResponseCode = 4, ResponseMsg = "This sale has returns and cannot be cancelled.", StatusCode = 409 };

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            foreach (var line in sale.Details)
            {
                var stockResult = await _stock.ApplyMovementAsync(new InputStockMovement
                {
                    FkStore = sale.FkStore,
                    FkProduct = line.FkProduct,
                    FkProductVariant = line.FkProductVariant,
                    MovementType = "return_in",
                    Quantity = line.Quantity,
                    ReferenceType = "sale_cancel",
                    ReferenceId = sale.IdSale,
                    Notes = $"Cancellation of {sale.InvoiceNumber}",
                    CreatedBy = input.CancelledBy
                }, ct);

                if (!stockResult.Ok)
                {
                    await tx.RollbackAsync(ct);
                    return new OutputSaleCancel
                    {
                        ResponseCode = stockResult.ResponseCode,
                        ResponseMsg = stockResult.ResponseMsg,
                        StatusCode = stockResult.StatusCode == 0 ? 409 : stockResult.StatusCode
                    };
                }
            }

            sale.Cancelled = true;
            sale.CancelledOn = DateTime.UtcNow;
            sale.CancelledReason = input.Reason;
            sale.Status = "cancelled";
            sale.UpdatedAt = DateTime.UtcNow;

            var payments = await _db.Payments
                .Where(p => p.FkSale == sale.IdSale && !p.Cancelled)
                .ToListAsync(ct);
            foreach (var payment in payments)
            {
                payment.Cancelled = true;
                payment.CancelledOn = DateTime.UtcNow;
                payment.CancelledReason = input.Reason;
                payment.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return new OutputSaleCancel();
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // RETURN
    // ═══════════════════════════════════════════════════════════════

    public async Task<OutputSaleReturnableSelect?> GetReturnableSaleAsync(
        InputSaleReturnableSelect input, CancellationToken ct = default)
    {
        if (input.FkStore <= 0 || input.IdSale <= 0)
            return null;

        var sale = await _db.Sales.AsNoTracking()
            .Include(x => x.Customer)
            .Include(x => x.Details).ThenInclude(d => d.Product)
            .Where(x => x.IdSale == input.IdSale && x.FkStore == input.FkStore
                && !x.Cancelled && x.Status == "completed")
            .FirstOrDefaultAsync(ct);

        if (sale is null)
            return null;

        var variantMap = await LoadVariantMapAsync(sale.Details.Select(d => d.FkProductVariant), ct);
        var remaining = await RemainingBySaleDetailAsync(sale.IdSale, sale.Details, ct);

        var cashier = sale.CreatedBy is int userId
            ? await _db.Users.AsNoTracking()
                .Where(u => u.IdUser == userId)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync(ct)
            : null;

        return new OutputSaleReturnableSelect
        {
            IdSale = sale.IdSale,
            InvoiceNumber = sale.InvoiceNumber,
            SaleDate = sale.SaleDate,
            CustomerName = sale.Customer?.FullName,
            CashierName = cashier,
            Lines = sale.Details.Select(d =>
            {
                variantMap.TryGetValue(d.FkProductVariant ?? 0, out var variant);
                remaining.TryGetValue(d.IdSaleDetail, out var rem);
                var returned = d.Quantity - rem;
                if (returned < 0) returned = 0;
                return new OutputSaleReturnableLine
                {
                    IdSaleDetail = d.IdSaleDetail,
                    FkProduct = d.FkProduct,
                    FkProductVariant = d.FkProductVariant,
                    ProductName = d.Product?.ProductName ?? string.Empty,
                    VariantName = variant.Name,
                    Sku = variant.Sku ?? d.Product?.Sku,
                    OriginalQty = d.Quantity,
                    ReturnedQty = returned,
                    RemainingQty = rem,
                    UnitPrice = d.UnitPrice
                };
            }).ToList()
        };
    }

    public async Task<OutputSalesReturnList> ListSalesReturnsAsync(
        InputSalesReturnList input, CancellationToken ct = default)
    {
        var q = _db.SalesReturns.AsNoTracking().Where(r => r.FkStore == input.FkStore);

        if (!string.IsNullOrWhiteSpace(input.Status))
        {
            if (input.Status == "cancelled")
                q = q.Where(r => r.Cancelled || r.Status == "cancelled");
            else
                q = q.Where(r => !r.Cancelled && r.Status == input.Status);
        }

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var term = input.Search.Trim().ToLower();
            q = q.Where(r => r.ReturnNumber.ToLower().Contains(term)
                || (r.Sale != null && r.Sale.InvoiceNumber.ToLower().Contains(term)));
        }

        if (input.FromUtc.HasValue)
            q = q.Where(r => r.ReturnDate >= input.FromUtc.Value);
        if (input.ToExclusiveUtc.HasValue)
            q = q.Where(r => r.ReturnDate < input.ToExclusiveUtc.Value);

        q = (input.SortBy, input.SortDesc) switch
        {
            ("returnnumber", true) => q.OrderByDescending(r => r.ReturnNumber),
            ("returnnumber", false) => q.OrderBy(r => r.ReturnNumber),
            ("invoicenumber", true) => q.OrderByDescending(r => r.Sale != null ? r.Sale.InvoiceNumber : string.Empty),
            ("invoicenumber", false) => q.OrderBy(r => r.Sale != null ? r.Sale.InvoiceNumber : string.Empty),
            ("totalamount", true) => q.OrderByDescending(r => r.TotalAmount),
            ("totalamount", false) => q.OrderBy(r => r.TotalAmount),
            ("status", true) => q.OrderByDescending(r => r.Cancelled).ThenByDescending(r => r.Status),
            ("status", false) => q.OrderBy(r => r.Cancelled).ThenBy(r => r.Status),
            ("cashier", true) => q.OrderByDescending(r => r.CreatedBy == null
                ? string.Empty
                : _db.Users.Where(u => u.IdUser == r.CreatedBy).Select(u => u.FullName).FirstOrDefault()),
            ("cashier", false) => q.OrderBy(r => r.CreatedBy == null
                ? string.Empty
                : _db.Users.Where(u => u.IdUser == r.CreatedBy).Select(u => u.FullName).FirstOrDefault()),
            ("id", true) => q.OrderByDescending(r => r.IdSalesReturn),
            ("id", false) => q.OrderBy(r => r.IdSalesReturn),
            (_, true) => q.OrderByDescending(r => r.ReturnDate),
            _ => q.OrderBy(r => r.ReturnDate)
        };

        var total = await q.CountAsync(ct);
        var rows = await q
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .Select(r => new
            {
                r.IdSalesReturn,
                r.FkSale,
                r.ReturnNumber,
                InvoiceNumber = r.Sale != null ? r.Sale.InvoiceNumber : string.Empty,
                r.ReturnDate,
                r.CreatedBy,
                r.TotalAmount,
                Status = r.Cancelled ? "cancelled" : r.Status
            })
            .ToListAsync(ct);

        var cashierIds = rows.Where(r => r.CreatedBy is > 0).Select(r => r.CreatedBy!.Value).Distinct().ToList();
        var cashiers = cashierIds.Count == 0
            ? new Dictionary<int, string>()
            : await _db.Users.AsNoTracking()
                .Where(u => cashierIds.Contains(u.IdUser))
                .ToDictionaryAsync(u => u.IdUser, u => u.FullName, ct);

        return new OutputSalesReturnList
        {
            TotalCount = total,
            Page = input.Page,
            PageSize = input.PageSize,
            Items = rows.Select(r => new OutputSalesReturnListItem
            {
                IdSalesReturn = r.IdSalesReturn,
                FkSale = r.FkSale,
                ReturnNumber = r.ReturnNumber,
                InvoiceNumber = r.InvoiceNumber,
                ReturnDate = r.ReturnDate,
                CashierName = r.CreatedBy is int cid && cashiers.TryGetValue(cid, out var name) ? name : null,
                TotalAmount = r.TotalAmount,
                Status = r.Status
            }).ToList()
        };
    }

    public async Task<OutputSalesReturnSelect?> GetSalesReturnAsync(
        InputSalesReturnSelect input, CancellationToken ct = default)
    {
        if (input.FkStore <= 0 || input.IdSalesReturn <= 0)
            return null;

        var header = await _db.SalesReturns.AsNoTracking()
            .Include(r => r.Store)
            .Include(r => r.Sale)
                .ThenInclude(s => s!.Customer)
            .Include(r => r.Details).ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(r => r.IdSalesReturn == input.IdSalesReturn && r.FkStore == input.FkStore, ct);

        if (header is null)
            return null;

        var variantMap = await LoadVariantMapAsync(header.Details.Select(d => d.FkProductVariant), ct);

        var cashier = header.CreatedBy is int userId
            ? await _db.Users.AsNoTracking()
                .Where(u => u.IdUser == userId)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync(ct)
            : null;

        return new OutputSalesReturnSelect
        {
            IdSalesReturn = header.IdSalesReturn,
            FkStore = header.FkStore,
            FkSale = header.FkSale,
            StoreName = header.Store?.StoreName ?? string.Empty,
            ReturnNumber = header.ReturnNumber,
            InvoiceNumber = header.Sale?.InvoiceNumber ?? string.Empty,
            ReturnDate = header.ReturnDate,
            TotalAmount = header.TotalAmount,
            Reason = header.Reason,
            Status = header.Cancelled ? "cancelled" : header.Status,
            CustomerName = header.Sale?.Customer?.FullName,
            CashierName = cashier,
            Cancelled = header.Cancelled,
            CancelledOn = header.CancelledOn,
            CancelledReason = header.CancelledReason,
            Lines = header.Details.Select(d =>
            {
                variantMap.TryGetValue(d.FkProductVariant ?? 0, out var variant);
                return new OutputSalesReturnLine
                {
                    IdSalesReturnDetail = d.IdSalesReturnDetail,
                    FkProduct = d.FkProduct,
                    ProductName = d.Product?.ProductName ?? string.Empty,
                    FkProductVariant = d.FkProductVariant,
                    VariantName = variant.Name,
                    Sku = variant.Sku ?? d.Product?.Sku,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    LineTotal = d.LineTotal
                };
            }).ToList()
        };
    }

    public async Task<OutputSaleReturn> ReturnSaleAsync(InputSaleReturn input, CancellationToken ct = default)
    {
        var sale = await _db.Sales
            .Include(x => x.Details)
            .FirstOrDefaultAsync(x => x.IdSale == input.IdSale && x.FkStore == input.FkStore, ct);

        if (sale is null)
            throw new ResourceNotFoundException(
                "Invoice not found.",
                $"Sale {input.IdSale} was not found in store {input.FkStore}.");

        if (sale.Cancelled || string.Equals(sale.Status, "cancelled", StringComparison.OrdinalIgnoreCase))
            throw new ConflictException(
                "This sale is cancelled and cannot be returned.",
                $"Sale {sale.IdSale} is cancelled.");

        if (!string.Equals(sale.Status, "completed", StringComparison.OrdinalIgnoreCase))
            throw new ConflictException(
                "Only a completed sale can be returned.",
                $"Sale {sale.IdSale} has status '{sale.Status}'.");

        var posted = input.Lines
            .Select((line, index) => (line, index))
            .Where(x => x.line.ReturnQty > 0)
            .ToList();

        if (posted.Count == 0)
        {
            const string message = "Enter a return quantity for at least one line.";
            throw new ValidationException(message, message, new Dictionary<string, string[]> { ["Lines"] = new[] { message } });
        }

        var remainingByDetail = await RemainingBySaleDetailAsync(sale.IdSale, sale.Details, ct);
        var priceByDetail = sale.Details.ToDictionary(d => d.IdSaleDetail, d => d.UnitPrice);
        var fieldErrors = new Dictionary<string, string[]>();

        foreach (var (line, index) in posted)
        {
            line.FkProductVariant = line.FkProductVariant is > 0 ? line.FkProductVariant : null;
            line.ReturnQty = Math.Round(line.ReturnQty, 3, MidpointRounding.AwayFromZero);

            if (!TryMatchSaleDetail(sale.Details, remainingByDetail, line, out var detailId, out var remaining))
            {
                fieldErrors[$"Lines[{index}].ReturnQty"] = new[] { "This item is not on the original invoice." };
                continue;
            }

            if (line.ReturnQty > remaining)
            {
                fieldErrors[$"Lines[{index}].ReturnQty"] =
                    new[] { $"You can return at most {remaining.ToString("0.###")} of this item." };
                continue;
            }

            remainingByDetail[detailId] = remaining - line.ReturnQty;
            line.IdSaleDetail = detailId;
            line.UnitPrice = priceByDetail[detailId];
        }

        if (fieldErrors.Count > 0)
        {
            var summary = string.Join(" ", fieldErrors.Values.SelectMany(v => v).Distinct().Take(3));
            throw new ValidationException(summary, summary, fieldErrors);
        }

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            var returnNumber = await _docNum.NextAsync(input.FkStore, "RETURN", ct);
            decimal totalReturn = 0;

            var salesReturn = new SalesReturn
            {
                FkStore = input.FkStore,
                FkSale = sale.IdSale,
                ReturnNumber = returnNumber,
                ReturnDate = DateTime.UtcNow,
                Reason = input.Reason,
                Status = "completed",
                CreatedBy = input.CreatedBy,
                CreatedAt = DateTime.UtcNow
            };
            _db.SalesReturns.Add(salesReturn);
            await _db.SaveChangesAsync(ct);

            foreach (var (line, _) in posted)
            {
                var lineTotal = Money(line.ReturnQty * line.UnitPrice);
                totalReturn += lineTotal;

                _db.SalesReturnDetails.Add(new SalesReturnDetail
                {
                    FkSalesReturn = salesReturn.IdSalesReturn,
                    FkStore = input.FkStore,
                    FkProduct = line.FkProduct,
                    FkProductVariant = line.FkProductVariant,
                    Quantity = line.ReturnQty,
                    UnitPrice = line.UnitPrice,
                    LineTotal = lineTotal,
                    CreatedAt = DateTime.UtcNow
                });

                var stockResult = await _stock.ApplyMovementAsync(new InputStockMovement
                {
                    FkStore = input.FkStore,
                    FkProduct = line.FkProduct,
                    FkProductVariant = line.FkProductVariant,
                    MovementType = "return_in",
                    Quantity = line.ReturnQty,
                    ReferenceType = "sales_return",
                    ReferenceId = salesReturn.IdSalesReturn,
                    Notes = $"Return {returnNumber}",
                    CreatedBy = input.CreatedBy
                }, ct);

                if (!stockResult.Ok)
                {
                    var text = string.IsNullOrWhiteSpace(stockResult.ResponseMsg)
                        ? "Unable to restock this return."
                        : stockResult.ResponseMsg;
                    throw new ConflictException(
                        text,
                        $"ApplyMovementAsync failed for return {returnNumber}: {stockResult.StatusCode} {text}");
                }
            }

            salesReturn.TotalAmount = Money(totalReturn);
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return new OutputSaleReturn
            {
                IdSalesReturn = salesReturn.IdSalesReturn,
                ReturnNumber = returnNumber,
                TotalAmount = salesReturn.TotalAmount
            };
        }
        catch
        {
            await tx.RollbackAsync(ct);
            throw;
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // CASH SESSION
    // ═══════════════════════════════════════════════════════════════

    public async Task<OutputCashSessionOpen> OpenSessionAsync(InputCashSessionOpen input, CancellationToken ct = default)
    {
        var existing = await _db.CashSessions.AsNoTracking()
            .FirstOrDefaultAsync(x => x.FkStore == input.FkStore && x.Status == "open", ct);
        if (existing is not null)
            return new OutputCashSessionOpen { ResponseCode = 1, ResponseMsg = "A cash session is already open for this store.", StatusCode = 409 };

        var session = new CashSession
        {
            FkStore = input.FkStore,
            FkUser = input.FkUser,
            OpenedAt = DateTime.UtcNow,
            OpeningFloat = input.OpeningFloat,
            Status = "open",
            Notes = input.Notes,
            CreatedAt = DateTime.UtcNow
        };
        _db.CashSessions.Add(session);
        await _db.SaveChangesAsync(ct);
        return new OutputCashSessionOpen { IdCashSession = session.IdCashSession };
    }

    public async Task<OutputCashSessionClose> CloseSessionAsync(InputCashSessionClose input, CancellationToken ct = default)
    {
        var session = await _db.CashSessions.FirstOrDefaultAsync(
            x => x.IdCashSession == input.IdCashSession && x.FkStore == input.FkStore && x.Status == "open", ct);
        if (session is null)
            return new OutputCashSessionClose { ResponseCode = 1, ResponseMsg = "Open session not found.", StatusCode = 404 };

        var expectedCash = await ComputeExpectedCashAsync(session.IdCashSession, session.OpeningFloat, ct);
        var variance = input.ClosingCash - expectedCash;

        session.Status = "closed";
        session.ClosedAt = DateTime.UtcNow;
        session.ClosingCash = input.ClosingCash;
        session.ExpectedCash = expectedCash;
        session.Variance = variance;
        session.Notes = string.IsNullOrWhiteSpace(input.Notes) ? session.Notes : input.Notes;
        session.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new OutputCashSessionClose
        {
            ExpectedCash = expectedCash,
            ClosingCash = input.ClosingCash,
            Variance = variance
        };
    }

    public async Task<OutputCashSessionSelect?> GetOpenSessionAsync(InputCashSessionSelect input, CancellationToken ct = default)
    {
        var q = _db.CashSessions.AsNoTracking()
            .Where(x => x.FkStore == input.FkStore);

        if (input.IdCashSession.HasValue)
            q = q.Where(x => x.IdCashSession == input.IdCashSession.Value);
        else
            q = q.Where(x => x.Status == "open");

        return await ProjectSession(q).FirstOrDefaultAsync(ct);
    }

    public async Task<OutputCashSessionList> ListSessionsAsync(InputCashSessionList input, CancellationToken ct = default)
    {
        var q = _db.CashSessions.AsNoTracking().Where(x => x.FkStore == input.FkStore);

        if (!string.IsNullOrWhiteSpace(input.Status))
            q = q.Where(x => x.Status == input.Status);
        if (input.FromUtc.HasValue)
            q = q.Where(x => x.OpenedAt >= input.FromUtc.Value);
        if (input.ToExclusiveUtc.HasValue)
            q = q.Where(x => x.OpenedAt < input.ToExclusiveUtc.Value);

        q = q.OrderByDescending(x => x.OpenedAt);

        var total = await q.CountAsync(ct);
        var items = await q
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .Select(x => new OutputCashSessionListItem
            {
                IdCashSession = x.IdCashSession,
                UserName = x.User != null ? x.User.FullName : string.Empty,
                OpenedAt = x.OpenedAt,
                ClosedAt = x.ClosedAt,
                OpeningFloat = x.OpeningFloat,
                ClosingCash = x.ClosingCash,
                ExpectedCash = x.ExpectedCash,
                Variance = x.Variance,
                Status = x.Status,
                SaleCount = _db.Sales.Count(s => s.FkCashSession == x.IdCashSession && !s.Cancelled)
            })
            .ToListAsync(ct);

        return new OutputCashSessionList
        {
            TotalCount = total,
            Page = input.Page,
            PageSize = input.PageSize,
            OpenSession = await GetOpenSessionAsync(new InputCashSessionSelect { FkStore = input.FkStore }, ct),
            Items = items
        };
    }

    public async Task<OutputCashSummary?> GetCashSummaryAsync(InputCashSummary input, CancellationToken ct = default)
    {
        var session = await _db.CashSessions.AsNoTracking()
            .Where(x => x.IdCashSession == input.IdCashSession && x.FkStore == input.FkStore)
            .Select(x => new
            {
                x.IdCashSession,
                x.FkStore,
                x.FkUser,
                x.OpenedAt,
                x.ClosedAt,
                x.OpeningFloat,
                x.ClosingCash,
                x.ExpectedCash,
                x.Variance,
                x.Status,
                StoreName = x.Store != null ? x.Store.StoreName : string.Empty,
                UserName = x.User != null ? x.User.FullName : string.Empty
            })
            .FirstOrDefaultAsync(ct);

        if (session is null) return null;

        var sales = await _db.Sales.AsNoTracking()
            .Where(x => x.FkCashSession == input.IdCashSession)
            .Select(x => new { x.Cancelled, x.Status, x.TotalAmount, x.ChangeGiven })
            .ToListAsync(ct);

        var completed = sales.Where(x => !x.Cancelled && x.Status == "completed").ToList();
        var cancelled = sales.Where(x => x.Cancelled || x.Status == "cancelled").ToList();

        var returns = await _db.SalesReturns.AsNoTracking()
            .Where(x => !x.Cancelled
                && _db.Sales.Any(s => s.IdSale == x.FkSale && s.FkCashSession == input.IdCashSession))
            .Select(x => x.TotalAmount)
            .ToListAsync(ct);

        var byMethod = await _db.Payments.AsNoTracking()
            .Where(x => x.FkCashSession == input.IdCashSession && !x.Cancelled)
            .GroupBy(x => x.PaymentMethod)
            .Select(g => new OutputCashSummaryPaymentMethod
            {
                PaymentMethod = g.Key,
                Total = g.Sum(p => p.Amount),
                Count = g.Count()
            })
            .ToListAsync(ct);

        var cashIn = byMethod.Where(x => x.PaymentMethod == "cash").Sum(x => x.Total);
        var changeGiven = completed.Sum(x => x.ChangeGiven);
        var expected = session.Status == "closed" && session.ExpectedCash.HasValue
            ? session.ExpectedCash.Value
            : Money(session.OpeningFloat + cashIn - changeGiven);

        var totalReturns = returns.Sum();
        var salesAmount = completed.Sum(x => x.TotalAmount);

        return new OutputCashSummary
        {
            IdCashSession = session.IdCashSession,
            FkStore = session.FkStore,
            StoreName = session.StoreName,
            UserName = session.UserName,
            Status = session.Status,
            OpenedAt = session.OpenedAt,
            ClosedAt = session.ClosedAt,
            OpeningFloat = session.OpeningFloat,
            ClosingCash = session.ClosingCash,
            ExpectedCash = expected,
            Variance = session.Status == "closed" ? session.Variance : null,
            TotalSales = completed.Count,
            TotalSalesAmount = salesAmount,
            CancelledSales = cancelled.Count,
            ReturnCount = returns.Count,
            TotalReturns = totalReturns,
            NetSales = salesAmount - totalReturns,
            CashIn = cashIn,
            ChangeGiven = changeGiven,
            ByMethod = byMethod
        };
    }

    // ─── helpers ─────────────────────────────────────────────────

    private IQueryable<OutputCashSessionSelect> ProjectSession(IQueryable<CashSession> q) =>
        q.Select(x => new OutputCashSessionSelect
        {
            IdCashSession = x.IdCashSession,
            FkStore = x.FkStore,
            StoreName = x.Store != null ? x.Store.StoreName : string.Empty,
            FkUser = x.FkUser,
            UserName = x.User != null ? x.User.FullName : string.Empty,
            OpenedAt = x.OpenedAt,
            ClosedAt = x.ClosedAt,
            OpeningFloat = x.OpeningFloat,
            ClosingCash = x.ClosingCash,
            ExpectedCash = x.ExpectedCash,
            Variance = x.Variance,
            Status = x.Status,
            Notes = x.Notes
        });

    private async Task<decimal> ComputeExpectedCashAsync(int idCashSession, decimal openingFloat, CancellationToken ct)
    {
        var cashIn = await _db.Payments.AsNoTracking()
            .Where(x => x.FkCashSession == idCashSession && !x.Cancelled && x.PaymentMethod == "cash")
            .SumAsync(x => (decimal?)x.Amount, ct) ?? 0;

        var changeGiven = await _db.Sales.AsNoTracking()
            .Where(x => x.FkCashSession == idCashSession && !x.Cancelled && x.Status == "completed")
            .SumAsync(x => (decimal?)x.ChangeGiven, ct) ?? 0;

        return Money(openingFloat + cashIn - changeGiven);
    }

    private async Task<Sale?> FindByClientRefAsync(int fkStore, string clientRef, CancellationToken ct)
    {
        var marker = ClientRefMarker(clientRef);
        return await _db.Sales.AsNoTracking()
            .Where(x => x.FkStore == fkStore && x.Notes != null
                && (x.Notes == marker
                    || x.Notes.StartsWith(marker + "\n")
                    || x.Notes.StartsWith(marker + "\r\n")))
            .FirstOrDefaultAsync(ct);
    }

    private async Task<OutputSaleProductSelect> ToPosProductAsync(
        Product product, ProductVariant? variant, int fkStore, CancellationToken ct)
    {
        var unit = product.FkUnit is int unitId
            ? await _db.Units.AsNoTracking()
                .Where(u => u.IdUnit == unitId && !u.Cancelled)
                .Select(u => new { u.UnitName, u.UnitCode })
                .FirstOrDefaultAsync(ct)
            : null;

        var tax = product.FkTax is int taxId
            ? await _db.Taxes.AsNoTracking()
                .Where(t => t.IdTax == taxId && !t.Cancelled && t.IsActive)
                .Select(t => (decimal?)t.TaxRate)
                .FirstOrDefaultAsync(ct)
            : null;

        var variantId = variant?.IdProductVariant;
        var stock = await _db.Inventories.AsNoTracking()
            .Where(i => i.FkStore == fkStore
                && i.FkProduct == product.IdProduct
                && i.FkProductVariant == variantId)
            .Select(i => (decimal?)i.QuantityOnHand)
            .FirstOrDefaultAsync(ct) ?? 0;

        var price = Money(product.SellingPrice + (variant?.AdditionalPrice ?? 0));

        return new OutputSaleProductSelect
        {
            IdProduct = product.IdProduct,
            IdProductVariant = variantId,
            Name = product.ProductName,
            VariantName = variant?.VariantName,
            Sku = variant?.SkuCode ?? product.Sku,
            Barcode = variant?.Barcode ?? product.Barcode,
            UnitName = unit?.UnitName,
            UnitCode = unit?.UnitCode,
            AllowsFraction = AllowsFraction(unit?.UnitCode),
            SellingPrice = price,
            HasProductTax = tax.HasValue,
            TaxRate = tax ?? 0,
            StockOnHand = stock
        };
    }

    private static string ClientRefMarker(string clientRef) => "__ref:" + clientRef.Trim();

    private static string? BuildNotes(string? clientRef, string? notes)
    {
        var body = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        if (string.IsNullOrWhiteSpace(clientRef))
            return body;

        var marker = ClientRefMarker(clientRef);
        return body is null ? marker : marker + "\n" + body;
    }

    private static bool AllowsFraction(string? unitCode)
    {
        if (string.IsNullOrWhiteSpace(unitCode))
            return false;
        return !DiscreteUnits.Contains(unitCode.Trim());
    }

    private static string? FormatAddress(string? line1, string? line2, string? city, string? state, string? postal, string? country)
    {
        var parts = new[] { line1, line2, city, state, postal, country }
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Select(p => p!.Trim());
        var text = string.Join(", ", parts);
        return text.Length == 0 ? null : text;
    }

    private async Task<Dictionary<int, (string Name, string? Sku)>> LoadVariantMapAsync(
        IEnumerable<int?> variantIds, CancellationToken ct)
    {
        var ids = variantIds.Where(id => id is > 0).Select(id => id!.Value).Distinct().ToList();
        if (ids.Count == 0)
            return new Dictionary<int, (string Name, string? Sku)>();

        return await _db.ProductVariants.AsNoTracking()
            .Where(v => ids.Contains(v.IdProductVariant))
            .ToDictionaryAsync(v => v.IdProductVariant, v => (Name: v.VariantName, Sku: v.SkuCode), ct);
    }

    private async Task<Dictionary<int, decimal>> RemainingBySaleDetailAsync(
        int idSale, IEnumerable<SaleDetail> details, CancellationToken ct)
    {
        var prior = await (
            from d in _db.SalesReturnDetails.AsNoTracking()
            join r in _db.SalesReturns.AsNoTracking() on d.FkSalesReturn equals r.IdSalesReturn
            where r.FkSale == idSale && !r.Cancelled
            select new { d.FkProduct, Variant = d.FkProductVariant ?? 0, d.Quantity }
        ).ToListAsync(ct);

        var returnedBySku = prior
            .GroupBy(x => (x.FkProduct, x.Variant))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        var remaining = new Dictionary<int, decimal>();
        foreach (var line in details)
        {
            var key = (line.FkProduct, line.FkProductVariant ?? 0);
            var already = returnedBySku.GetValueOrDefault(key);
            var taken = Math.Min(line.Quantity, already);
            returnedBySku[key] = already - taken;
            remaining[line.IdSaleDetail] = line.Quantity - taken;
        }

        return remaining;
    }

    private static bool TryMatchSaleDetail(
        IEnumerable<SaleDetail> details,
        IReadOnlyDictionary<int, decimal> remainingByDetail,
        InputReturnLine line,
        out int detailId,
        out decimal remaining)
    {
        detailId = 0;
        remaining = 0;

        if (line.IdSaleDetail > 0)
        {
            var detail = details.FirstOrDefault(d => d.IdSaleDetail == line.IdSaleDetail);
            if (detail is null)
                return false;
            if (detail.FkProduct != line.FkProduct)
                return false;
            if ((detail.FkProductVariant ?? 0) != (line.FkProductVariant ?? 0))
                return false;

            detailId = detail.IdSaleDetail;
            remaining = remainingByDetail.GetValueOrDefault(detailId);
            return true;
        }

        var match = details.FirstOrDefault(d =>
            d.FkProduct == line.FkProduct
            && (d.FkProductVariant ?? 0) == (line.FkProductVariant ?? 0)
            && remainingByDetail.GetValueOrDefault(d.IdSaleDetail) > 0);
        if (match is null)
            return false;

        detailId = match.IdSaleDetail;
        remaining = remainingByDetail.GetValueOrDefault(detailId);
        return true;
    }

    private static decimal Money(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
