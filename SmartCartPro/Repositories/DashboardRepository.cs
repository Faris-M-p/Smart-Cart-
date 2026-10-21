using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly SmartCartDbContext _db;

    public DashboardRepository(SmartCartDbContext db) => _db = db;

    public async Task<OutputDashboardKpis> GetOperationalCountsAsync(
        List<int>? storeIds,
        CancellationToken ct = default)
    {
        var inventory = _db.Inventories.AsNoTracking();
        var purchases = _db.Purchases.AsNoTracking();
        var orders = _db.Orders.AsNoTracking();
        var sessions = _db.CashSessions.AsNoTracking();

        if (storeIds is not null)
        {
            inventory = inventory.Where(i => storeIds.Contains(i.FkStore));
            purchases = purchases.Where(p => storeIds.Contains(p.FkStore));
            orders = orders.Where(o => storeIds.Contains(o.FkStore));
            sessions = sessions.Where(s => storeIds.Contains(s.FkStore));
        }

        var lowStock = await inventory.CountAsync(
            i => (i.ReorderLevel > 0 && i.QuantityOnHand <= i.ReorderLevel)
                || (i.ReorderLevel <= 0 && i.QuantityOnHand <= 0),
            ct);

        var pendingPo = await purchases.CountAsync(
            p => !p.Cancelled && (p.Status == "posted" || p.Status == "partially_received"),
            ct);

        var pendingOrders = await orders.CountAsync(
            o => !o.Cancelled && o.Status == "pending",
            ct);

        var openCash = await sessions.CountAsync(
            s => s.Status == "open",
            ct);

        return new OutputDashboardKpis
        {
            LowStockCount = lowStock,
            PendingPurchaseOrders = pendingPo,
            PendingOnlineOrders = pendingOrders,
            OpenCashSessions = openCash
        };
    }

    public async Task<List<OutputDashboardSaleFact>> ListSaleFactsAsync(
        List<int>? storeIds,
        DateTime fromUtc,
        DateTime toUtcExclusive,
        CancellationToken ct = default)
    {
        var q = _db.Sales.AsNoTracking()
            .Where(s => !s.Cancelled
                && s.Status == "completed"
                && s.SaleDate >= fromUtc
                && s.SaleDate < toUtcExclusive);

        if (storeIds is not null)
            q = q.Where(s => storeIds.Contains(s.FkStore));

        return await q
            .Select(s => new OutputDashboardSaleFact
            {
                SaleDate = s.SaleDate,
                TotalAmount = s.TotalAmount
            })
            .ToListAsync(ct);
    }

    public async Task<List<OutputDashboardPaymentSlice>> ListPaymentsAsync(
        List<int>? storeIds,
        DateTime fromUtc,
        DateTime toUtcExclusive,
        CancellationToken ct = default)
    {
        var q =
            from p in _db.Payments.AsNoTracking()
            join s in _db.Sales.AsNoTracking() on p.FkSale equals s.IdSale
            where !p.Cancelled
                && !s.Cancelled
                && s.Status == "completed"
                && s.SaleDate >= fromUtc
                && s.SaleDate < toUtcExclusive
            select new { p.PaymentMethod, p.Amount, s.FkStore };

        if (storeIds is not null)
            q = q.Where(x => storeIds.Contains(x.FkStore));

        return await q
            .GroupBy(x => x.PaymentMethod)
            .Select(g => new OutputDashboardPaymentSlice
            {
                PaymentMethod = g.Key,
                Amount = g.Sum(x => x.Amount)
            })
            .OrderByDescending(x => x.Amount)
            .ToListAsync(ct);
    }

    public async Task<List<OutputDashboardProductPoint>> ListTopProductsAsync(
        List<int>? storeIds,
        DateTime fromUtc,
        DateTime toUtcExclusive,
        int take,
        CancellationToken ct = default)
    {
        var q =
            from d in _db.SaleDetails.AsNoTracking()
            join s in _db.Sales.AsNoTracking() on d.FkSale equals s.IdSale
            join p in _db.Products.AsNoTracking() on d.FkProduct equals p.IdProduct
            where !s.Cancelled
                && s.Status == "completed"
                && s.SaleDate >= fromUtc
                && s.SaleDate < toUtcExclusive
            select new
            {
                d.FkProduct,
                p.ProductName,
                d.Quantity,
                d.LineTotal,
                s.FkStore
            };

        if (storeIds is not null)
            q = q.Where(x => storeIds.Contains(x.FkStore));

        var limit = take > 0 ? take : 5;

        return await q
            .GroupBy(x => new { x.FkProduct, x.ProductName })
            .Select(g => new OutputDashboardProductPoint
            {
                FkProduct = g.Key.FkProduct,
                ProductName = g.Key.ProductName,
                Quantity = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.LineTotal)
            })
            .OrderByDescending(r => r.Revenue)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<List<OutputDashboardRecentInvoice>> ListRecentInvoicesAsync(
        List<int>? storeIds,
        int take,
        CancellationToken ct = default)
    {
        var q =
            from s in _db.Sales.AsNoTracking()
            join c in _db.Customers.AsNoTracking() on s.FkCustomer equals c.IdCustomer into cj
            from c in cj.DefaultIfEmpty()
            where !s.Cancelled && s.Status == "completed"
            select new
            {
                s.IdSale,
                s.InvoiceNumber,
                CustomerName = c != null ? c.FullName : null,
                s.TotalAmount,
                s.SaleDate,
                s.FkStore
            };

        if (storeIds is not null)
            q = q.Where(x => storeIds.Contains(x.FkStore));

        var limit = take > 0 ? take : 8;

        var rows = await q
            .OrderByDescending(x => x.SaleDate)
            .ThenByDescending(x => x.IdSale)
            .Take(limit)
            .ToListAsync(ct);

        return rows.Select(x => new OutputDashboardRecentInvoice
        {
            IdSale = x.IdSale,
            InvoiceNumber = x.InvoiceNumber,
            CustomerName = string.IsNullOrWhiteSpace(x.CustomerName) ? "Walk-in" : x.CustomerName,
            TotalAmount = x.TotalAmount,
            SaleDate = x.SaleDate
        }).ToList();
    }

    public async Task<List<OutputDashboardLowStockRow>> ListLowStockAsync(
        List<int>? storeIds,
        int take,
        CancellationToken ct = default)
    {
        var q =
            from i in _db.Inventories.AsNoTracking()
            join p in _db.Products.AsNoTracking() on i.FkProduct equals p.IdProduct
            where !p.Cancelled
                && ((i.ReorderLevel > 0 && i.QuantityOnHand <= i.ReorderLevel)
                    || (i.ReorderLevel <= 0 && i.QuantityOnHand <= 0))
            select new
            {
                p.ProductName,
                p.ProductCode,
                i.QuantityOnHand,
                i.ReorderLevel,
                i.FkStore
            };

        if (storeIds is not null)
            q = q.Where(x => storeIds.Contains(x.FkStore));

        var limit = take > 0 ? take : 8;

        return await q
            .OrderBy(x => x.QuantityOnHand)
            .Take(limit)
            .Select(x => new OutputDashboardLowStockRow
            {
                ProductName = x.ProductName,
                ProductCode = x.ProductCode,
                QuantityOnHand = x.QuantityOnHand,
                ReorderLevel = x.ReorderLevel
            })
            .ToListAsync(ct);
    }
}
