using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly SmartCartDbContext _db;

    public DashboardRepository(SmartCartDbContext db) => _db = db;

    #region Essential Queries

    public async Task<(decimal Sales, int Bills)> GetSalesSummaryAsync(
        int storeId,
        int? userId,
        DateTime fromUtc,
        DateTime toUtcExclusive,
        CancellationToken ct = default)
    {
        var q = _db.Sales.AsNoTracking()
            .Where(s => !s.Cancelled && s.Status == "completed" && s.FkStore == storeId
                        && s.SaleDate >= fromUtc && s.SaleDate < toUtcExclusive);

        if (userId.HasValue && userId.Value > 0)
            q = q.Where(s => s.CreatedBy == userId.Value);

        var count = await q.CountAsync(ct);
        var total = count == 0 ? 0m : await q.SumAsync(s => s.TotalAmount, ct);
        return (total, count);
    }

    public async Task<(decimal Returns, int ReturnCount)> GetReturnsSummaryAsync(
        int storeId,
        int? userId,
        DateTime fromUtc,
        DateTime toUtcExclusive,
        CancellationToken ct = default)
    {
        var q = _db.SalesReturns.AsNoTracking()
            .Where(r => !r.Cancelled && r.FkStore == storeId
                        && r.ReturnDate >= fromUtc && r.ReturnDate < toUtcExclusive);

        if (userId.HasValue && userId.Value > 0)
            q = q.Where(r => r.CreatedBy == userId.Value);

        var count = await q.CountAsync(ct);
        var total = count == 0 ? 0m : await q.SumAsync(r => r.TotalAmount, ct);
        return (total, count);
    }

    public async Task<List<OutputDashboardPaymentSlice>> ListPaymentsAsync(
        int storeId,
        int? userId,
        DateTime fromUtc,
        DateTime toUtcExclusive,
        CancellationToken ct = default)
    {
        var q = from p in _db.Payments.AsNoTracking()
                join s in _db.Sales.AsNoTracking() on p.FkSale equals s.IdSale
                where !p.Cancelled && !s.Cancelled && s.Status == "completed"
                      && s.FkStore == storeId
                      && s.SaleDate >= fromUtc && s.SaleDate < toUtcExclusive
                select new { p.PaymentMethod, p.Amount, s.CreatedBy };

        if (userId.HasValue && userId.Value > 0)
            q = q.Where(x => x.CreatedBy == userId.Value);

        var grouped = await q.GroupBy(x => x.PaymentMethod)
            .Select(g => new { Method = g.Key, Amount = g.Sum(x => x.Amount) })
            .OrderByDescending(x => x.Amount)
            .ToListAsync(ct);

        var total = grouped.Sum(x => x.Amount);
        var pal = new[] { "var(--c1)", "var(--c7)", "var(--c4)", "var(--c3)", "var(--c5)", "var(--c6)", "var(--c2)", "var(--c8)" };
        var index = 0;

        return grouped.Select(x => new OutputDashboardPaymentSlice
        {
            PaymentMethod = string.IsNullOrWhiteSpace(x.Method) ? "Other" : x.Method.ToUpperInvariant(),
            Amount = x.Amount,
            Percentage = total == 0 ? 0 : Math.Round(x.Amount * 100m / total, 1),
            Color = pal[index++ % pal.Length]
        }).ToList();
    }

    public async Task<List<OutputDashboardSaleFact>> ListSaleFactsAsync(
        int storeId,
        int? userId,
        DateTime fromUtc,
        DateTime toUtcExclusive,
        CancellationToken ct = default)
    {
        var q = _db.Sales.AsNoTracking()
            .Where(s => !s.Cancelled
                && s.Status == "completed"
                && s.FkStore == storeId
                && s.SaleDate >= fromUtc
                && s.SaleDate < toUtcExclusive);

        if (userId.HasValue && userId.Value > 0)
            q = q.Where(s => s.CreatedBy == userId.Value);

        return await q
            .Select(s => new OutputDashboardSaleFact
            {
                SaleDate = s.SaleDate,
                TotalAmount = s.TotalAmount
            })
            .ToListAsync(ct);
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

    public async Task<List<OutputDashboardTopProduct>> ListTopProductsAsync(
        int storeId,
        int? userId,
        DateTime fromUtc,
        DateTime toUtcExclusive,
        int take,
        CancellationToken ct = default)
    {
        var q = from d in _db.SaleDetails.AsNoTracking()
                join s in _db.Sales.AsNoTracking() on d.FkSale equals s.IdSale
                join p in _db.Products.AsNoTracking() on d.FkProduct equals p.IdProduct
                where !s.Cancelled && s.Status == "completed"
                      && s.FkStore == storeId
                      && s.SaleDate >= fromUtc && s.SaleDate < toUtcExclusive
                select new { d.FkProduct, p.ProductName, d.Quantity, d.LineTotal, s.CreatedBy };

        if (userId.HasValue && userId.Value > 0)
            q = q.Where(x => x.CreatedBy == userId.Value);

        var limit = take > 0 ? take : 5;

        return await q.GroupBy(x => new { x.FkProduct, x.ProductName })
            .Select(g => new OutputDashboardTopProduct
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

    public async Task<List<OutputDashboardLowStockItem>> ListLowStockAsync(
        int storeId,
        int take,
        CancellationToken ct = default)
    {
        var limit = take > 0 ? take : 5;

        return await (from i in _db.Inventories.AsNoTracking()
                      join p in _db.Products.AsNoTracking() on i.FkProduct equals p.IdProduct
                      where i.FkStore == storeId && !p.Cancelled
                            && ((i.ReorderLevel > 0 && i.QuantityOnHand <= i.ReorderLevel)
                                || (i.ReorderLevel <= 0 && i.QuantityOnHand <= 0))
                      orderby i.QuantityOnHand ascending
                      select new OutputDashboardLowStockItem
                      {
                          FkProduct = p.IdProduct,
                          ProductName = p.ProductName,
                          ProductCode = p.ProductCode,
                          QuantityOnHand = i.QuantityOnHand,
                          ReorderLevel = i.ReorderLevel,
                          IsOutOfStock = i.QuantityOnHand <= 0
                      })
                      .Take(limit)
                      .ToListAsync(ct);
    }

    public async Task<List<OutputDashboardRecentInvoiceItem>> ListRecentInvoicesAsync(
        int storeId,
        int? userId,
        int take,
        CancellationToken ct = default)
    {
        var q = from s in _db.Sales.AsNoTracking()
                join c in _db.Customers.AsNoTracking() on s.FkCustomer equals c.IdCustomer into cj
                from c in cj.DefaultIfEmpty()
                where !s.Cancelled && s.Status == "completed" && s.FkStore == storeId
                select new
                {
                    s.IdSale,
                    s.InvoiceNumber,
                    CustomerName = c != null ? c.FullName : "Walk-in",
                    s.TotalAmount,
                    s.SaleDate,
                    s.CreatedBy
                };

        if (userId.HasValue && userId.Value > 0)
            q = q.Where(x => x.CreatedBy == userId.Value);

        var limit = take > 0 ? take : 5;
        var sales = await q.OrderByDescending(x => x.SaleDate).Take(limit).ToListAsync(ct);
        var saleIds = sales.Select(s => s.IdSale).ToList();

        var paymentModes = saleIds.Count == 0
            ? new Dictionary<int, string>()
            : (await _db.Payments.AsNoTracking()
                .Where(p => p.FkSale != null && saleIds.Contains(p.FkSale.Value) && !p.Cancelled)
                .GroupBy(p => p.FkSale!.Value)
                .Select(g => new { SaleId = g.Key, Mode = g.OrderByDescending(x => x.Amount).Select(x => x.PaymentMethod).FirstOrDefault() })
                .ToListAsync(ct))
                .ToDictionary(x => x.SaleId, x => string.IsNullOrWhiteSpace(x.Mode) ? "Cash" : x.Mode.ToUpperInvariant());

        return sales.Select(x => new OutputDashboardRecentInvoiceItem
        {
            IdSale = x.IdSale,
            InvoiceNumber = x.InvoiceNumber,
            CustomerName = string.IsNullOrWhiteSpace(x.CustomerName) ? "Walk-in" : x.CustomerName,
            Amount = x.TotalAmount,
            PaymentMode = paymentModes.TryGetValue(x.IdSale, out var m) ? m : "CASH",
            SaleTimeFormatted = x.SaleDate.ToString("hh:mm tt"),
            DetailsUrl = $"/Sales/InvoiceDetails/{x.IdSale}"
        }).ToList();
    }

    public async Task<(int LowStockCount, int OutOfStockCount, int PendingPos, int PendingOnlineOrders, int OpenCashSessions, string? OpenCashierName, decimal OpenExpectedCash)> GetOperationalStatusAsync(
        int storeId,
        int? userId,
        CancellationToken ct = default)
    {
        var lowStock = await _db.Inventories.AsNoTracking()
            .CountAsync(i => i.FkStore == storeId && i.QuantityOnHand > 0 && i.ReorderLevel > 0 && i.QuantityOnHand <= i.ReorderLevel, ct);

        var outStock = await _db.Inventories.AsNoTracking()
            .CountAsync(i => i.FkStore == storeId && i.QuantityOnHand <= 0, ct);

        var pendingPo = await _db.Purchases.AsNoTracking()
            .CountAsync(p => p.FkStore == storeId && !p.Cancelled && (p.Status == "posted" || p.Status == "partially_received"), ct);

        var pendingOrders = await _db.Orders.AsNoTracking()
            .CountAsync(o => o.FkStore == storeId && !o.Cancelled && (o.Status == "pending" || o.Status == "placed" || o.Status == "confirmed"), ct);

        var sessionQ = from s in _db.CashSessions.AsNoTracking()
                       join u in _db.Users.AsNoTracking() on s.FkUser equals u.IdUser into uj
                       from u in uj.DefaultIfEmpty()
                       where s.FkStore == storeId && s.Status == "open"
                       select new { s.IdCashSession, s.ExpectedCash, s.OpeningFloat, CashierName = u != null ? u.FullName : "Unknown", s.FkUser, s.OpenedAt };

        if (userId.HasValue && userId.Value > 0)
            sessionQ = sessionQ.Where(s => s.FkUser == userId.Value);

        var sessions = await sessionQ.OrderByDescending(s => s.OpenedAt).ToListAsync(ct);
        var openCount = sessions.Count;
        var firstSession = sessions.FirstOrDefault();

        return (lowStock, outStock, pendingPo, pendingOrders, openCount, firstSession?.CashierName, firstSession?.ExpectedCash ?? firstSession?.OpeningFloat ?? 0m);
    }

    public async Task<(decimal? LastVariance, string? LastCashierName)> GetLastClosedSessionDifferenceAsync(
        int storeId,
        CancellationToken ct = default)
    {
        var last = await (from s in _db.CashSessions.AsNoTracking()
                          join u in _db.Users.AsNoTracking() on s.FkUser equals u.IdUser into uj
                          from u in uj.DefaultIfEmpty()
                          where s.FkStore == storeId && s.Status == "closed" && s.ClosedAt != null
                          orderby s.ClosedAt descending
                          select new { s.Variance, CashierName = u != null ? u.FullName : "Staff" })
                          .FirstOrDefaultAsync(ct);

        return (last?.Variance, last?.CashierName);
    }

    public async Task<(int PendingCount, DateTime? OldestOrderDate)> GetOldestPendingOnlineOrderAsync(
        int storeId,
        CancellationToken ct = default)
    {
        var orders = await _db.Orders.AsNoTracking()
            .Where(o => o.FkStore == storeId && !o.Cancelled && (o.Status == "pending" || o.Status == "placed" || o.Status == "confirmed"))
            .Select(o => o.OrderDate)
            .ToListAsync(ct);

        if (orders.Count == 0)
            return (0, null);

        return (orders.Count, orders.Min());
    }

    public async Task<DateTime?> GetCashierOpenSessionTimeAsync(
        int storeId,
        int userId,
        CancellationToken ct = default)
    {
        var openSession = await _db.CashSessions.AsNoTracking()
            .Where(s => s.FkStore == storeId && s.FkUser == userId && s.Status == "open")
            .OrderByDescending(s => s.OpenedAt)
            .Select(s => (DateTime?)s.OpenedAt)
            .FirstOrDefaultAsync(ct);

        return openSession;
    }

    public async Task<DateOnly?> GetLicenseExpiryDateAsync(CancellationToken ct = default)
    {
        return await _db.Licenses.AsNoTracking()
            .Where(l => !l.Cancelled && l.Status == "active")
            .OrderBy(l => l.ExpiryDate)
            .Select(l => (DateOnly?)l.ExpiryDate)
            .FirstOrDefaultAsync(ct);
    }

    #endregion

    #region Advanced Queries

    public async Task<List<OutputDashboardHourlyPoint>> ListHourlySalesAsync(
        int storeId,
        DateTime fromUtc,
        DateTime toUtcExclusive,
        TimeZoneInfo tz,
        CancellationToken ct = default)
    {
        var sales = await _db.Sales.AsNoTracking()
            .Where(s => !s.Cancelled && s.Status == "completed" && s.FkStore == storeId
                        && s.SaleDate >= fromUtc && s.SaleDate < toUtcExclusive)
            .Select(s => new { s.SaleDate, s.TotalAmount })
            .ToListAsync(ct);

        // Group into hours 8 to 22 local
        var dict = new Dictionary<int, decimal>();
        for (int h = 8; h <= 22; h++) dict[h] = 0m;

        foreach (var s in sales)
        {
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(s.SaleDate, DateTimeKind.Utc), tz);
            var hour = localTime.Hour;
            if (hour >= 8 && hour <= 22)
            {
                dict[hour] += s.TotalAmount;
            }
        }

        return dict.Select(kv => new OutputDashboardHourlyPoint
        {
            Hour = kv.Key,
            Label = (kv.Key % 12 == 0 ? 12 : kv.Key % 12) + (kv.Key < 12 ? "a" : "p"),
            Sales = kv.Value
        }).ToList();
    }

    public async Task<List<OutputDashboardDayOfWeekPoint>> ListDayOfWeekSalesAsync(
        int storeId,
        DateTime fromUtc,
        DateTime toUtcExclusive,
        TimeZoneInfo tz,
        CancellationToken ct = default)
    {
        var sales = await _db.Sales.AsNoTracking()
            .Where(s => !s.Cancelled && s.Status == "completed" && s.FkStore == storeId
                        && s.SaleDate >= fromUtc && s.SaleDate < toUtcExclusive)
            .Select(s => new { s.SaleDate, s.TotalAmount })
            .ToListAsync(ct);

        var days = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday };
        var dict = days.ToDictionary(d => d, d => 0m);

        foreach (var s in sales)
        {
            var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(s.SaleDate, DateTimeKind.Utc), tz);
            dict[localTime.DayOfWeek] += s.TotalAmount;
        }

        return days.Select(d => new OutputDashboardDayOfWeekPoint
        {
            DayName = d.ToString().Substring(0, 3),
            Sales = dict[d]
        }).ToList();
    }

    public async Task<(decimal CounterSales, decimal OnlineSales)> GetCounterVsOnlineSalesAsync(
        int storeId,
        DateTime fromUtc,
        DateTime toUtcExclusive,
        CancellationToken ct = default)
    {
        var counter = await _db.Sales.AsNoTracking()
            .Where(s => !s.Cancelled && s.Status == "completed" && s.FkStore == storeId
                        && s.SaleDate >= fromUtc && s.SaleDate < toUtcExclusive)
            .SumAsync(s => (decimal?)s.TotalAmount, ct) ?? 0m;

        var online = await _db.Orders.AsNoTracking()
            .Where(o => !o.Cancelled && o.FkStore == storeId
                        && (o.Status == "completed" || o.Status == "delivered" || o.Status == "dispatched")
                        && o.OrderDate >= fromUtc && o.OrderDate < toUtcExclusive)
            .SumAsync(o => (decimal?)o.TotalAmount, ct) ?? 0m;

        return (counter, online);
    }

    public async Task<List<OutputDashboardTopProduct>> ListTopProductsByQuantityAsync(
        int storeId,
        DateTime fromUtc,
        DateTime toUtcExclusive,
        int take,
        CancellationToken ct = default)
    {
        var q = from d in _db.SaleDetails.AsNoTracking()
                join s in _db.Sales.AsNoTracking() on d.FkSale equals s.IdSale
                join p in _db.Products.AsNoTracking() on d.FkProduct equals p.IdProduct
                where !s.Cancelled && s.Status == "completed"
                      && s.FkStore == storeId
                      && s.SaleDate >= fromUtc && s.SaleDate < toUtcExclusive
                select new { d.FkProduct, p.ProductName, d.Quantity, d.LineTotal };

        var limit = take > 0 ? take : 10;

        return await q.GroupBy(x => new { x.FkProduct, x.ProductName })
            .Select(g => new OutputDashboardTopProduct
            {
                FkProduct = g.Key.FkProduct,
                ProductName = g.Key.ProductName,
                Quantity = g.Sum(x => x.Quantity),
                Revenue = g.Sum(x => x.LineTotal)
            })
            .OrderByDescending(r => r.Quantity)
            .Take(limit)
            .ToListAsync(ct);
    }

    public async Task<List<OutputDashboardCategoryItem>> ListCategorySalesAsync(
        int storeId,
        DateTime fromUtc,
        DateTime toUtcExclusive,
        int take,
        CancellationToken ct = default)
    {
        var q = from d in _db.SaleDetails.AsNoTracking()
                join s in _db.Sales.AsNoTracking() on d.FkSale equals s.IdSale
                join p in _db.Products.AsNoTracking() on d.FkProduct equals p.IdProduct
                join c in _db.Categories.AsNoTracking() on p.FkCategory equals c.IdCategory into cj
                from c in cj.DefaultIfEmpty()
                where !s.Cancelled && s.Status == "completed" && s.FkStore == storeId
                      && s.SaleDate >= fromUtc && s.SaleDate < toUtcExclusive
                group d by new { FkCategory = p.FkCategory ?? 0, CategoryName = c != null ? c.CategoryName : "Others" } into g
                select new
                {
                    g.Key.FkCategory,
                    g.Key.CategoryName,
                    Total = g.Sum(x => x.LineTotal)
                };

        var all = await q.OrderByDescending(x => x.Total).ToListAsync(ct);
        var totalSales = all.Sum(x => x.Total);
        var pal = new[] { "var(--c1)", "var(--c2)", "var(--c3)", "var(--c4)", "var(--c5)", "var(--c6)", "var(--c7)", "var(--c8)" };

        var top = all.Take(take > 0 ? take : 8).ToList();
        var othersTotal = all.Skip(top.Count).Sum(x => x.Total);

        var result = top.Select((x, i) => new OutputDashboardCategoryItem
        {
            FkCategory = x.FkCategory,
            CategoryName = x.CategoryName,
            Sales = x.Total,
            SharePercentage = totalSales == 0 ? 0 : Math.Round(x.Total * 100m / totalSales, 1),
            Color = pal[i % pal.Length]
        }).ToList();

        if (othersTotal > 0)
        {
            result.Add(new OutputDashboardCategoryItem
            {
                FkCategory = 0,
                CategoryName = "Others",
                Sales = othersTotal,
                SharePercentage = totalSales == 0 ? 0 : Math.Round(othersTotal * 100m / totalSales, 1),
                Color = pal[7]
            });
        }

        return result;
    }

    public async Task<(int InStock, int Low, int Out, int NonMoving30d, int Total)> GetStockHealthAsync(
        int storeId,
        CancellationToken ct = default)
    {
        var items = await (from i in _db.Inventories.AsNoTracking()
                           join p in _db.Products.AsNoTracking() on i.FkProduct equals p.IdProduct
                           where i.FkStore == storeId && !p.Cancelled
                           select new { i.FkProduct, i.QuantityOnHand, i.ReorderLevel }).ToListAsync(ct);

        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var movingProductIds = await (from d in _db.SaleDetails.AsNoTracking()
                                      join s in _db.Sales.AsNoTracking() on d.FkSale equals s.IdSale
                                      where s.FkStore == storeId && !s.Cancelled && s.SaleDate >= thirtyDaysAgo
                                      select d.FkProduct).Distinct().ToListAsync(ct);

        var movingSet = new HashSet<int>(movingProductIds);

        var inStock = items.Count(i => i.QuantityOnHand > (i.ReorderLevel > 0 ? i.ReorderLevel : 0));
        var low = items.Count(i => i.ReorderLevel > 0 && i.QuantityOnHand > 0 && i.QuantityOnHand <= i.ReorderLevel);
        var outStock = items.Count(i => i.QuantityOnHand <= 0);
        var nonMoving = items.Count(i => i.QuantityOnHand > 0 && !movingSet.Contains(i.FkProduct));

        return (inStock, low, outStock, nonMoving, items.Count);
    }

    public async Task<List<(int Year, int Month, decimal Sales, decimal Purchases)>> ListSalesVsPurchases6MonthsAsync(
        int storeId,
        DateOnly currentMonthStart,
        CancellationToken ct = default)
    {
        // 6 months ending with current month
        var sixMonthsAgo = currentMonthStart.AddMonths(-5);
        var fromUtc = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var toUtc = new DateTime(currentMonthStart.Year, currentMonthStart.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1);

        var sales = await _db.Sales.AsNoTracking()
            .Where(s => !s.Cancelled && s.Status == "completed" && s.FkStore == storeId
                        && s.SaleDate >= fromUtc && s.SaleDate < toUtc)
            .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.TotalAmount) })
            .ToListAsync(ct);

        var purchases = await _db.Purchases.AsNoTracking()
            .Where(p => !p.Cancelled && (p.Status == "posted" || p.Status == "partially_received" || p.Status == "received")
                        && p.FkStore == storeId
                        && p.PurchaseDate >= sixMonthsAgo && p.PurchaseDate < currentMonthStart.AddMonths(1))
            .GroupBy(p => new { p.PurchaseDate.Year, p.PurchaseDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.TotalAmount) })
            .ToListAsync(ct);

        var salesMap = sales.ToDictionary(x => (x.Year, x.Month), x => x.Total);
        var purchasesMap = purchases.ToDictionary(x => (x.Year, x.Month), x => x.Total);

        var result = new List<(int Year, int Month, decimal Sales, decimal Purchases)>();
        for (int i = 0; i < 6; i++)
        {
            var m = sixMonthsAgo.AddMonths(i);
            salesMap.TryGetValue((m.Year, m.Month), out var sVal);
            purchasesMap.TryGetValue((m.Year, m.Month), out var pVal);
            result.Add((m.Year, m.Month, sVal, pVal));
        }

        return result;
    }

    public async Task<List<OutputDashboardCashierPerformance>> ListCashierPerformanceAsync(
        int storeId,
        DateTime fromUtc,
        DateTime toUtcExclusive,
        CancellationToken ct = default)
    {
        var cashiers = await (from s in _db.Sales.AsNoTracking()
                              join u in _db.Users.AsNoTracking() on s.CreatedBy equals u.IdUser into uj
                              from u in uj.DefaultIfEmpty()
                              where !s.Cancelled && s.Status == "completed" && s.FkStore == storeId
                                    && s.SaleDate >= fromUtc && s.SaleDate < toUtcExclusive
                              group s by new { UserId = s.CreatedBy ?? 0, Name = u != null ? u.FullName : "Staff" } into g
                              select new OutputDashboardCashierPerformance
                              {
                                  UserId = g.Key.UserId,
                                  CashierName = g.Key.Name,
                                  BillsCount = g.Count(),
                                  SalesTotal = g.Sum(x => x.TotalAmount),
                                  AvgBillValue = g.Count() == 0 ? 0 : Math.Round(g.Sum(x => x.TotalAmount) / g.Count(), 2),
                                  DiscountPercent = g.Sum(x => x.SubTotal) == 0 ? 0 : Math.Round(g.Sum(x => x.DiscountAmount) * 100m / g.Sum(x => x.SubTotal), 1)
                              })
                              .OrderByDescending(x => x.SalesTotal)
                              .ToListAsync(ct);

        return cashiers;
    }

    public async Task<List<OutputDashboardStoreComparisonItem>> ListStoreComparisonAsync(
        List<int> accessibleStoreIds,
        DateTime fromUtc,
        DateTime toUtcExclusive,
        CancellationToken ct = default)
    {
        if (accessibleStoreIds.Count < 2)
            return new List<OutputDashboardStoreComparisonItem>();

        var stores = await _db.Stores.AsNoTracking()
            .Where(s => accessibleStoreIds.Contains(s.IdStore) && s.IsActive && !s.Cancelled)
            .OrderBy(s => s.StoreName)
            .ToListAsync(ct);

        var sales = await _db.Sales.AsNoTracking()
            .Where(s => !s.Cancelled && s.Status == "completed" && accessibleStoreIds.Contains(s.FkStore)
                        && s.SaleDate >= fromUtc && s.SaleDate < toUtcExclusive)
            .GroupBy(s => s.FkStore)
            .Select(g => new { StoreId = g.Key, Total = g.Sum(x => x.TotalAmount) })
            .ToListAsync(ct);

        var salesMap = sales.ToDictionary(x => x.StoreId, x => x.Total);

        return stores.Select(s => new OutputDashboardStoreComparisonItem
        {
            StoreId = s.IdStore,
            StoreName = s.StoreName,
            SalesTotal = salesMap.TryGetValue(s.IdStore, out var t) ? t : 0m
        }).OrderByDescending(x => x.SalesTotal).ToList();
    }

    public async Task<List<int>> GetAccessibleStoreIdsAsync(int userId, bool isPlatformSuperAdmin, CancellationToken ct = default)
    {
        if (isPlatformSuperAdmin)
        {
            return await _db.Stores.AsNoTracking()
                .Where(s => s.IsActive && !s.Cancelled)
                .Select(s => s.IdStore)
                .ToListAsync(ct);
        }

        return await _db.UserStoreAccess.AsNoTracking()
            .Where(usa => usa.FkUser == userId)
            .Select(usa => usa.FkStore)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<(decimal GrossProfit, decimal NetSales, decimal MarginPercent)> GetGrossProfitAsync(
        int storeId,
        DateTime fromUtc,
        DateTime toUtcExclusive,
        CancellationToken ct = default)
    {
        var data = await (from d in _db.SaleDetails.AsNoTracking()
                          join s in _db.Sales.AsNoTracking() on d.FkSale equals s.IdSale
                          join p in _db.Products.AsNoTracking() on d.FkProduct equals p.IdProduct
                          where !s.Cancelled && s.Status == "completed" && s.FkStore == storeId
                                && s.SaleDate >= fromUtc && s.SaleDate < toUtcExclusive
                          select new
                          {
                              Net = d.LineTotal - d.TaxAmount,
                              Cost = d.Quantity * p.CostPrice
                          }).ToListAsync(ct);

        var netTotal = data.Sum(x => x.Net);
        var costTotal = data.Sum(x => x.Cost);
        var profit = netTotal - costTotal;
        var margin = netTotal == 0 ? 0 : Math.Round(profit * 100m / netTotal, 1);

        return (profit, netTotal, margin);
    }

    public async Task<List<OutputDashboardCategoryMargin>> ListCategoryMarginsAsync(
        int storeId,
        DateTime fromUtc,
        DateTime toUtcExclusive,
        int take,
        CancellationToken ct = default)
    {
        var data = await (from d in _db.SaleDetails.AsNoTracking()
                          join s in _db.Sales.AsNoTracking() on d.FkSale equals s.IdSale
                          join p in _db.Products.AsNoTracking() on d.FkProduct equals p.IdProduct
                          join c in _db.Categories.AsNoTracking() on p.FkCategory equals c.IdCategory into cj
                          from c in cj.DefaultIfEmpty()
                          where !s.Cancelled && s.Status == "completed" && s.FkStore == storeId
                                && s.SaleDate >= fromUtc && s.SaleDate < toUtcExclusive
                          select new
                          {
                              CategoryName = c != null ? c.CategoryName : "Others",
                              Net = d.LineTotal - d.TaxAmount,
                              Cost = d.Quantity * p.CostPrice
                          }).ToListAsync(ct);

        var grouped = data.GroupBy(x => x.CategoryName)
            .Select(g =>
            {
                var net = g.Sum(x => x.Net);
                var cost = g.Sum(x => x.Cost);
                var profit = net - cost;
                var margin = net == 0 ? 0 : Math.Round(profit * 100m / net, 1);
                return new OutputDashboardCategoryMargin
                {
                    CategoryName = g.Key,
                    MarginPercent = margin
                };
            })
            .OrderByDescending(x => x.MarginPercent)
            .Take(take > 0 ? take : 4)
            .ToList();

        return grouped;
    }

    #endregion

    #region Legacy Queries

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
            o => !o.Cancelled && (o.Status == "pending" || o.Status == "placed" || o.Status == "confirmed"),
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

    #endregion
}
