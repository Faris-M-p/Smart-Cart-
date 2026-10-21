using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly SmartCartDbContext _db;

    public ReportRepository(SmartCartDbContext db) => _db = db;

    public Task<List<OutputReportStoreOption>> ListActiveStoresAsync(CancellationToken ct = default) =>
        _db.Stores.AsNoTracking()
            .Where(s => s.IsActive && !s.Cancelled)
            .OrderBy(s => s.StoreName)
            .Select(s => new OutputReportStoreOption
            {
                IdStore = s.IdStore,
                StoreName = s.StoreName
            })
            .ToListAsync(ct);

    public Task<List<OutputReportCategoryOption>> ListActiveCategoriesAsync(CancellationToken ct = default) =>
        _db.Categories.AsNoTracking()
            .Where(c => c.IsActive && !c.Cancelled)
            .OrderBy(c => c.CategoryName)
            .Select(c => new OutputReportCategoryOption
            {
                IdCategory = c.IdCategory,
                CategoryName = c.CategoryName
            })
            .ToListAsync(ct);

    public Task<bool> ActiveStoreExistsAsync(int idStore, CancellationToken ct = default) =>
        _db.Stores.AsNoTracking()
            .AnyAsync(s => s.IdStore == idStore && s.IsActive && !s.Cancelled, ct);

    public async Task<List<OutputReportSaleFact>> ListSaleFactsAsync(InputReportSales input, CancellationToken ct = default)
    {
        var q = _db.Sales.AsNoTracking()
            .Where(s => s.SaleDate >= input.FromUtc && s.SaleDate < input.ToUtcExclusive);

        q = ApplyStore(q, input.StoreIds);

        return await q
            .Select(s => new OutputReportSaleFact
            {
                SaleDate = s.SaleDate,
                SubTotal = s.SubTotal,
                DiscountAmount = s.DiscountAmount,
                TaxAmount = s.TaxAmount,
                TotalAmount = s.TotalAmount,
                Cancelled = s.Cancelled,
                Status = s.Status
            })
            .ToListAsync(ct);
    }

    public async Task<List<OutputReportReturnFact>> ListReturnFactsAsync(InputReportSales input, CancellationToken ct = default)
    {
        var q = _db.SalesReturns.AsNoTracking()
            .Where(r => !r.Cancelled
                && r.ReturnDate >= input.FromUtc
                && r.ReturnDate < input.ToUtcExclusive);

        if (input.StoreIds is not null)
            q = q.Where(r => input.StoreIds.Contains(r.FkStore));

        return await q
            .Select(r => new OutputReportReturnFact
            {
                ReturnDate = r.ReturnDate,
                TotalAmount = r.TotalAmount
            })
            .ToListAsync(ct);
    }

    public async Task<List<OutputReportPaymentSlice>> ListSalesPaymentsAsync(InputReportSales input, CancellationToken ct = default)
    {
        var q =
            from p in _db.Payments.AsNoTracking()
            join s in _db.Sales.AsNoTracking() on p.FkSale equals s.IdSale
            where !p.Cancelled
                && !s.Cancelled
                && s.Status == "completed"
                && s.SaleDate >= input.FromUtc
                && s.SaleDate < input.ToUtcExclusive
            select new { p.PaymentMethod, p.Amount, s.FkStore };

        if (input.StoreIds is not null)
            q = q.Where(x => input.StoreIds.Contains(x.FkStore));

        return await q
            .GroupBy(x => x.PaymentMethod)
            .Select(g => new OutputReportPaymentSlice
            {
                PaymentMethod = g.Key,
                Amount = g.Sum(x => x.Amount),
                PaymentCount = g.Count()
            })
            .OrderByDescending(x => x.Amount)
            .ToListAsync(ct);
    }

    public async Task<List<OutputReportProductRow>> ListTopProductsAsync(InputReportProducts input, CancellationToken ct = default)
    {
        var q =
            from d in _db.SaleDetails.AsNoTracking()
            join s in _db.Sales.AsNoTracking() on d.FkSale equals s.IdSale
            join p in _db.Products.AsNoTracking() on d.FkProduct equals p.IdProduct
            join c in _db.Categories.AsNoTracking() on p.FkCategory equals c.IdCategory into cj
            from c in cj.DefaultIfEmpty()
            where !s.Cancelled
                && s.Status == "completed"
                && s.SaleDate >= input.FromUtc
                && s.SaleDate < input.ToUtcExclusive
            select new
            {
                d.FkProduct,
                p.ProductName,
                p.ProductCode,
                CategoryName = c != null ? c.CategoryName : null,
                p.FkCategory,
                d.Quantity,
                d.LineTotal,
                s.FkStore
            };

        if (input.StoreIds is not null)
            q = q.Where(x => input.StoreIds.Contains(x.FkStore));

        if (input.FkCategory is int categoryId and > 0)
            q = q.Where(x => x.FkCategory == categoryId);

        var grouped = q
            .GroupBy(x => new { x.FkProduct, x.ProductName, x.ProductCode, x.CategoryName })
            .Select(g => new OutputReportProductRow
            {
                FkProduct = g.Key.FkProduct,
                ProductName = g.Key.ProductName,
                ProductCode = g.Key.ProductCode,
                CategoryName = g.Key.CategoryName,
                QuantitySold = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.LineTotal),
                TransactionCount = g.Count()
            });

        var ranked = string.Equals(input.RankBy, "revenue", StringComparison.OrdinalIgnoreCase)
            ? grouped.OrderByDescending(r => r.Revenue)
            : grouped.OrderByDescending(r => r.QuantitySold);

        var top = input.Top is 10 or 25 or 50 or 100 ? input.Top : 10;
        return await ranked.Take(top).ToListAsync(ct);
    }

    public async Task<List<OutputReportStockValuationRow>> ListStockValuationAsync(InputReportStockValuation input, CancellationToken ct = default)
    {
        var q =
            from i in _db.Inventories.AsNoTracking()
            join p in _db.Products.AsNoTracking() on i.FkProduct equals p.IdProduct
            join c in _db.Categories.AsNoTracking() on p.FkCategory equals c.IdCategory into cj
            from c in cj.DefaultIfEmpty()
            join st in _db.Stores.AsNoTracking() on i.FkStore equals st.IdStore
            where !p.Cancelled && p.IsActive
            select new
            {
                i.FkStore,
                i.FkProduct,
                i.QuantityOnHand,
                p.ProductName,
                p.ProductCode,
                p.CostPrice,
                p.FkCategory,
                CategoryName = c != null ? c.CategoryName : null,
                st.StoreName
            };

        if (input.StoreIds is not null)
            q = q.Where(x => input.StoreIds.Contains(x.FkStore));

        if (input.FkCategory is int categoryId and > 0)
            q = q.Where(x => x.FkCategory == categoryId);

        if (!input.IncludeZeroNegative)
            q = q.Where(x => x.QuantityOnHand > 0);

        return await q
            .Select(x => new OutputReportStockValuationRow
            {
                FkProduct = x.FkProduct,
                ProductName = x.ProductName,
                ProductCode = x.ProductCode,
                CategoryName = x.CategoryName ?? "Uncategorised",
                StoreName = x.StoreName,
                QuantityOnHand = x.QuantityOnHand,
                UnitCost = x.CostPrice,
                StockValue = x.QuantityOnHand * x.CostPrice
            })
            .OrderByDescending(r => r.StockValue)
            .ToListAsync(ct);
    }

    public async Task<List<OutputReportTaxRow>> ListTaxSummaryAsync(InputReportTax input, CancellationToken ct = default)
    {
        var q =
            from d in _db.SaleDetails.AsNoTracking()
            join s in _db.Sales.AsNoTracking() on d.FkSale equals s.IdSale
            join p in _db.Products.AsNoTracking() on d.FkProduct equals p.IdProduct
            join t in _db.Taxes.AsNoTracking() on p.FkTax equals t.IdTax into tj
            from t in tj.DefaultIfEmpty()
            where !s.Cancelled
                && s.Status == "completed"
                && s.SaleDate >= input.FromUtc
                && s.SaleDate < input.ToUtcExclusive
            select new
            {
                s.IdSale,
                s.FkStore,
                TaxCode = t != null ? t.TaxCode : null,
                TaxName = t != null ? t.TaxName : null,
                TaxRate = t != null ? t.TaxRate : 0m,
                LineNet = d.LineTotal - d.TaxAmount,
                d.TaxAmount
            };

        if (input.StoreIds is not null)
            q = q.Where(x => input.StoreIds.Contains(x.FkStore));

        var facts = await q.ToListAsync(ct);

        return facts
            .GroupBy(x => new
            {
                TaxCode = string.IsNullOrWhiteSpace(x.TaxCode) ? "NONE" : x.TaxCode,
                TaxName = string.IsNullOrWhiteSpace(x.TaxName) ? "No tax" : x.TaxName,
                x.TaxRate
            })
            .Select(g => new OutputReportTaxRow
            {
                TaxCode = g.Key.TaxCode,
                TaxName = g.Key.TaxName,
                TaxRate = g.Key.TaxRate,
                SaleCount = g.Select(x => x.IdSale).Distinct().Count(),
                NetSales = g.Sum(x => x.LineNet),
                TaxCollected = g.Sum(x => x.TaxAmount)
            })
            .OrderByDescending(r => r.TaxCollected)
            .ToList();
    }

    public async Task<List<OutputReportPurchaseRow>> ListPurchasesBySupplierAsync(InputReportPurchases input, CancellationToken ct = default)
    {
        var headers = _db.Purchases.AsNoTracking()
            .Where(p => p.PurchaseDate >= input.FromLocal && p.PurchaseDate <= input.ToLocal);

        if (input.StoreIds is not null)
            headers = headers.Where(p => input.StoreIds.Contains(p.FkStore));

        if (input.ExcludeCancelled)
            headers = headers.Where(p => !p.Cancelled && p.Status != "cancelled");

        var headerRows = await (
            from p in headers
            join s in _db.Suppliers.AsNoTracking() on p.FkSupplier equals s.IdSupplier
            select new
            {
                p.IdPurchase,
                p.FkSupplier,
                s.SupplierName,
                p.TotalAmount
            }
        ).ToListAsync(ct);

        if (headerRows.Count == 0)
            return new List<OutputReportPurchaseRow>();

        var ids = headerRows.Select(h => h.IdPurchase).ToList();

        var ordered = await _db.PurchaseDetails.AsNoTracking()
            .Where(d => ids.Contains(d.FkPurchase))
            .GroupBy(d => d.FkPurchase)
            .Select(g => new { IdPurchase = g.Key, Qty = g.Sum(x => x.Quantity) })
            .ToListAsync(ct);

        var received = await _db.StockMovements.AsNoTracking()
            .Where(m => m.MovementType == "purchase"
                && m.ReferenceType == "purchase"
                && m.ReferenceId != null
                && ids.Contains(m.ReferenceId.Value))
            .GroupBy(m => m.ReferenceId)
            .Select(g => new { IdPurchase = g.Key, Qty = g.Sum(x => x.Quantity) })
            .ToListAsync(ct);

        var orderedMap = ordered.ToDictionary(x => x.IdPurchase, x => x.Qty);
        var receivedMap = received
            .Where(x => x.IdPurchase.HasValue)
            .ToDictionary(x => x.IdPurchase!.Value, x => x.Qty);

        return headerRows
            .GroupBy(h => new { h.FkSupplier, h.SupplierName })
            .Select(g => new OutputReportPurchaseRow
            {
                FkSupplier = g.Key.FkSupplier,
                SupplierName = g.Key.SupplierName,
                PurchaseCount = g.Count(),
                OrderedQty = g.Sum(h => orderedMap.GetValueOrDefault(h.IdPurchase)),
                ReceivedQty = g.Sum(h => receivedMap.GetValueOrDefault(h.IdPurchase)),
                OrderedAmount = g.Sum(h => h.TotalAmount)
            })
            .OrderByDescending(r => r.OrderedAmount)
            .ToList();
    }

    public async Task<List<OutputReportCustomerRow>> ListTopCustomersAsync(InputReportCustomers input, CancellationToken ct = default)
    {
        var q =
            from s in _db.Sales.AsNoTracking()
            join c in _db.Customers.AsNoTracking() on s.FkCustomer equals c.IdCustomer into cj
            from c in cj.DefaultIfEmpty()
            where !s.Cancelled
                && s.Status == "completed"
                && s.SaleDate >= input.FromUtc
                && s.SaleDate < input.ToUtcExclusive
            select new
            {
                s.FkCustomer,
                CustomerName = c != null ? c.FullName : null,
                s.TotalAmount,
                s.SaleDate,
                s.FkStore
            };

        if (input.StoreIds is not null)
            q = q.Where(x => input.StoreIds.Contains(x.FkStore));

        var facts = await q.ToListAsync(ct);
        var top = input.Top is 10 or 25 or 50 or 100 ? input.Top : 25;

        return facts
            .GroupBy(x => x.FkCustomer)
            .Select(g =>
            {
                var spend = g.Sum(x => x.TotalAmount);
                var visits = g.Count();
                var name = g.Select(x => x.CustomerName).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n));
                return new OutputReportCustomerRow
                {
                    FkCustomer = g.Key,
                    CustomerName = string.IsNullOrWhiteSpace(name) ? "Walk-in" : name,
                    VisitCount = visits,
                    TotalSpend = spend,
                    AverageSpend = visits == 0 ? 0 : Math.Round(spend / visits, 2, MidpointRounding.AwayFromZero),
                    LastPurchaseAt = g.Max(x => x.SaleDate)
                };
            })
            .OrderByDescending(r => r.TotalSpend)
            .Take(top)
            .ToList();
    }

    private static IQueryable<SmartCartPro.Entities.Sale> ApplyStore(IQueryable<SmartCartPro.Entities.Sale> q, List<int>? storeIds)
    {
        if (storeIds is null)
            return q;
        return q.Where(s => storeIds.Contains(s.FkStore));
    }
}
