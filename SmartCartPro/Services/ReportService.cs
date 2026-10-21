using System.Globalization;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

/// <summary>
/// Store-scoped report orchestration: locale day bounds, store access, and CSV-ready aggregates.
/// Queries live in IReportRepository; this layer never touches DbContext.
/// </summary>
public class ReportService : IReportService
{
    private readonly IReportRepository _repo;
    private readonly ISettingService _settings;

    public ReportService(IReportRepository repo, ISettingService settings)
    {
        _repo = repo;
        _settings = settings;
    }

    public async Task<OutputReportIndex> GetIndexAsync(InputReportIndex input, CancellationToken ct = default)
    {
        var (currency, timeZoneId, _) = await LoadLocaleAsync(input.ActorFkStore, ct);
        return new OutputReportIndex
        {
            Currency = currency,
            TimeZoneId = timeZoneId,
            Cards = new List<OutputReportIndexCard>
            {
                Card("Sales", "Invoices, net, tax, returns and payments", "Sales", "bi-graph-up-arrow"),
                Card("Products", "Top sellers by quantity or revenue", "Products", "bi-bar-chart"),
                Card("Stock valuation", "On-hand quantity × product cost", "StockValuation", "bi-box-seam"),
                Card("Tax", "Tax collected by name and rate", "Tax", "bi-percent"),
                Card("Purchases", "Ordered versus received by supplier", "Purchases", "bi-truck"),
                Card("Customers", "Top spenders, visits and last purchase", "Customers", "bi-person-lines-fill"),
                new OutputReportIndexCard
                {
                    Title = "Low stock",
                    Description = "Items at or below reorder level",
                    Action = "LowStock",
                    Icon = "bi-exclamation-triangle",
                    ExternalController = "Inventory",
                    ExternalAction = "LowStock"
                }
            }
        };
    }

    public async Task<OutputReportSales> GetSalesAsync(InputReportSales input, CancellationToken ct = default)
    {
        var locale = await PrepareRangeAsync(input.ActorFkStore, input.DateFrom, input.DateTo, ct);
        input.FromUtc = locale.FromUtc;
        input.ToUtcExclusive = locale.ToUtcExclusive;
        input.StoreIds = await ResolveStoreIdsAsync(input.IsPlatformSuperAdmin, input.ActorFkStore, input.FkStore, ct);
        var groupBy = string.Equals(input.GroupBy, "month", StringComparison.OrdinalIgnoreCase) ? "month" : "day";

        var empty = EmptySales(input, locale, groupBy);
        if (input.StoreIds is { Count: 0 })
            return await WithLookupsAsync(empty, input.IsPlatformSuperAdmin, ct);

        var sales = await _repo.ListSaleFactsAsync(input, ct);
        var returns = await _repo.ListReturnFactsAsync(input, ct);
        var payments = await _repo.ListSalesPaymentsAsync(input, ct);

        var completed = sales
            .Where(s => !s.Cancelled && string.Equals(s.Status, "completed", StringComparison.OrdinalIgnoreCase))
            .ToList();
        var cancelledCount = sales.Count(s => s.Cancelled
            || string.Equals(s.Status, "cancelled", StringComparison.OrdinalIgnoreCase));

        var returnByBucket = new Dictionary<string, decimal>(StringComparer.Ordinal);
        foreach (var ret in returns)
        {
            var key = BucketKey(locale.TimeZone, ret.ReturnDate, groupBy);
            returnByBucket[key] = returnByBucket.GetValueOrDefault(key) + ret.TotalAmount;
        }

        var rows = completed
            .GroupBy(s => BucketKey(locale.TimeZone, s.SaleDate, groupBy))
            .Select(g =>
            {
                var invoices = g.Count();
                var gross = g.Sum(s => s.SubTotal + s.DiscountAmount);
                var discount = g.Sum(s => s.DiscountAmount);
                var tax = g.Sum(s => s.TaxAmount);
                var net = g.Sum(s => s.TotalAmount);
                var bucketDate = ParseBucketDate(g.Key, groupBy);
                return new OutputReportSalesRow
                {
                    BucketLabel = FormatBucket(g.Key, groupBy),
                    BucketDate = bucketDate,
                    InvoiceCount = invoices,
                    Gross = gross,
                    DiscountAmount = discount,
                    TaxAmount = tax,
                    Net = net,
                    AverageBill = invoices == 0 ? 0 : Math.Round(net / invoices, 2, MidpointRounding.AwayFromZero),
                    Returns = returnByBucket.GetValueOrDefault(g.Key)
                };
            })
            .OrderBy(r => r.BucketDate)
            .ThenBy(r => r.BucketLabel)
            .ToList();

        var invoiceCount = completed.Count;
        var grossTotal = completed.Sum(s => s.SubTotal + s.DiscountAmount);
        var discountTotal = completed.Sum(s => s.DiscountAmount);
        var taxTotal = completed.Sum(s => s.TaxAmount);
        var netTotal = completed.Sum(s => s.TotalAmount);
        var returnTotal = returns.Sum(r => r.TotalAmount);

        empty.Rows = rows;
        empty.Payments = payments;
        empty.Chart = rows.Select(r => new OutputReportChartPoint { Label = r.BucketLabel, Value = r.Net }).ToList();
        empty.CancelledCount = cancelledCount;
        empty.GrandNet = netTotal;
        empty.Kpis = new List<OutputReportKpi>
        {
            Kpi("Invoices", invoiceCount.ToString("N0", CultureInfo.InvariantCulture)),
            Kpi("Gross", LocaleClock.Money(locale.Currency, grossTotal)),
            Kpi("Discounts", LocaleClock.Money(locale.Currency, discountTotal)),
            Kpi("Tax", LocaleClock.Money(locale.Currency, taxTotal)),
            Kpi("Net", LocaleClock.Money(locale.Currency, netTotal)),
            Kpi("Avg bill", LocaleClock.Money(locale.Currency, invoiceCount == 0 ? 0 : netTotal / invoiceCount)),
            Kpi("Returns", LocaleClock.Money(locale.Currency, returnTotal)),
            Kpi("Cancelled", cancelledCount.ToString("N0", CultureInfo.InvariantCulture), "Excluded from totals")
        };

        return await WithLookupsAsync(empty, input.IsPlatformSuperAdmin, ct);
    }

    public async Task<OutputReportProducts> GetProductsAsync(InputReportProducts input, CancellationToken ct = default)
    {
        var locale = await PrepareRangeAsync(input.ActorFkStore, input.DateFrom, input.DateTo, ct);
        input.FromUtc = locale.FromUtc;
        input.ToUtcExclusive = locale.ToUtcExclusive;
        input.StoreIds = await ResolveStoreIdsAsync(input.IsPlatformSuperAdmin, input.ActorFkStore, input.FkStore, ct);
        if (input.Top is not (10 or 25 or 50 or 100))
            input.Top = 10;
        if (!string.Equals(input.RankBy, "revenue", StringComparison.OrdinalIgnoreCase))
            input.RankBy = "qty";

        var output = new OutputReportProducts
        {
            Filter = new InReportProductsView
            {
                DateFrom = locale.FromLocal.ToDateTime(TimeOnly.MinValue),
                DateTo = locale.ToLocal.ToDateTime(TimeOnly.MinValue),
                FkStore = DisplayStore(input.IsPlatformSuperAdmin, input.ActorFkStore, input.FkStore),
                FkCategory = input.FkCategory ?? 0,
                Top = input.Top,
                RankBy = input.RankBy
            },
            Currency = locale.Currency,
            TimeZoneId = locale.TimeZoneId,
            ShowStorePicker = input.IsPlatformSuperAdmin
        };

        if (input.StoreIds is { Count: 0 })
            return await WithLookupsAsync(output, ct);

        var rows = await _repo.ListTopProductsAsync(input, ct);
        output.Rows = rows;
        output.Chart = rows
            .Select(r => new OutputReportChartPoint
            {
                Label = r.ProductName,
                Value = string.Equals(input.RankBy, "revenue", StringComparison.OrdinalIgnoreCase) ? r.Revenue : r.QuantitySold
            })
            .ToList();
        output.Kpis = new List<OutputReportKpi>
        {
            Kpi("Products", rows.Count.ToString("N0", CultureInfo.InvariantCulture)),
            Kpi("Quantity", LocaleClock.Number(rows.Sum(r => r.QuantitySold))),
            Kpi("Revenue", LocaleClock.Money(locale.Currency, rows.Sum(r => r.Revenue)))
        };

        return await WithLookupsAsync(output, ct);
    }

    public async Task<OutputReportStockValuation> GetStockValuationAsync(InputReportStockValuation input, CancellationToken ct = default)
    {
        var (currency, _, _) = await LoadLocaleAsync(input.ActorFkStore, ct);
        input.StoreIds = await ResolveStoreIdsAsync(input.IsPlatformSuperAdmin, input.ActorFkStore, input.FkStore, ct);

        var output = new OutputReportStockValuation
        {
            Filter = new InReportStockValuationView
            {
                FkStore = DisplayStore(input.IsPlatformSuperAdmin, input.ActorFkStore, input.FkStore),
                FkCategory = input.FkCategory ?? 0,
                IncludeZeroNegative = input.IncludeZeroNegative
            },
            Currency = currency,
            ShowStorePicker = input.IsPlatformSuperAdmin
        };

        if (input.StoreIds is { Count: 0 })
            return await WithLookupsAsync(output, ct);

        var rows = await _repo.ListStockValuationAsync(input, ct);
        output.Rows = rows;
        output.CategoryTotals = rows
            .GroupBy(r => r.CategoryName)
            .Select(g => new OutputReportStockValuationGroup
            {
                CategoryName = g.Key,
                QuantityOnHand = g.Sum(x => x.QuantityOnHand),
                StockValue = g.Sum(x => x.StockValue)
            })
            .OrderByDescending(g => g.StockValue)
            .ToList();
        output.TotalStockValue = rows.Sum(r => r.StockValue);
        output.Kpis = new List<OutputReportKpi>
        {
            Kpi("Lines", rows.Count.ToString("N0", CultureInfo.InvariantCulture)),
            Kpi("On hand", LocaleClock.Number(rows.Sum(r => r.QuantityOnHand))),
            Kpi("Stock value", LocaleClock.Money(currency, output.TotalStockValue))
        };

        return await WithLookupsAsync(output, ct);
    }

    public async Task<OutputReportTax> GetTaxAsync(InputReportTax input, CancellationToken ct = default)
    {
        var locale = await PrepareRangeAsync(input.ActorFkStore, input.DateFrom, input.DateTo, ct);
        input.FromUtc = locale.FromUtc;
        input.ToUtcExclusive = locale.ToUtcExclusive;
        input.StoreIds = await ResolveStoreIdsAsync(input.IsPlatformSuperAdmin, input.ActorFkStore, input.FkStore, ct);

        var output = new OutputReportTax
        {
            Filter = new InReportTaxView
            {
                DateFrom = locale.FromLocal.ToDateTime(TimeOnly.MinValue),
                DateTo = locale.ToLocal.ToDateTime(TimeOnly.MinValue),
                FkStore = DisplayStore(input.IsPlatformSuperAdmin, input.ActorFkStore, input.FkStore)
            },
            Currency = locale.Currency,
            TimeZoneId = locale.TimeZoneId,
            ShowStorePicker = input.IsPlatformSuperAdmin
        };

        if (input.StoreIds is { Count: 0 })
            return await WithLookupsAsync(output, input.IsPlatformSuperAdmin, ct);

        var rows = await _repo.ListTaxSummaryAsync(input, ct);
        output.Rows = rows;
        output.TotalTaxCollected = rows.Sum(r => r.TaxCollected);
        output.Chart = rows
            .Select(r => new OutputReportChartPoint { Label = r.TaxName + " " + r.TaxRate.ToString("0.##", CultureInfo.InvariantCulture) + "%", Value = r.TaxCollected })
            .ToList();
        output.Kpis = new List<OutputReportKpi>
        {
            Kpi("Tax types", rows.Count.ToString("N0", CultureInfo.InvariantCulture)),
            Kpi("Net sales", LocaleClock.Money(locale.Currency, rows.Sum(r => r.NetSales))),
            Kpi("Tax collected", LocaleClock.Money(locale.Currency, output.TotalTaxCollected))
        };

        return await WithLookupsAsync(output, input.IsPlatformSuperAdmin, ct);
    }

    public async Task<OutputReportPurchases> GetPurchasesAsync(InputReportPurchases input, CancellationToken ct = default)
    {
        var locale = await PrepareRangeAsync(input.ActorFkStore, input.DateFrom, input.DateTo, ct);
        input.FromLocal = locale.FromLocal;
        input.ToLocal = locale.ToLocal;
        input.StoreIds = await ResolveStoreIdsAsync(input.IsPlatformSuperAdmin, input.ActorFkStore, input.FkStore, ct);

        var output = new OutputReportPurchases
        {
            Filter = new InReportPurchasesView
            {
                DateFrom = locale.FromLocal.ToDateTime(TimeOnly.MinValue),
                DateTo = locale.ToLocal.ToDateTime(TimeOnly.MinValue),
                FkStore = DisplayStore(input.IsPlatformSuperAdmin, input.ActorFkStore, input.FkStore),
                ExcludeCancelled = input.ExcludeCancelled
            },
            Currency = locale.Currency,
            TimeZoneId = locale.TimeZoneId,
            ShowStorePicker = input.IsPlatformSuperAdmin
        };

        if (input.StoreIds is { Count: 0 })
            return await WithLookupsAsync(output, input.IsPlatformSuperAdmin, ct);

        var rows = await _repo.ListPurchasesBySupplierAsync(input, ct);
        output.Rows = rows;
        output.Chart = rows
            .Select(r => new OutputReportChartPoint { Label = r.SupplierName, Value = r.OrderedAmount })
            .ToList();
        output.Kpis = new List<OutputReportKpi>
        {
            Kpi("Suppliers", rows.Count.ToString("N0", CultureInfo.InvariantCulture)),
            Kpi("Orders", rows.Sum(r => r.PurchaseCount).ToString("N0", CultureInfo.InvariantCulture)),
            Kpi("Ordered", LocaleClock.Number(rows.Sum(r => r.OrderedQty))),
            Kpi("Received", LocaleClock.Number(rows.Sum(r => r.ReceivedQty))),
            Kpi("Amount", LocaleClock.Money(locale.Currency, rows.Sum(r => r.OrderedAmount)))
        };

        return await WithLookupsAsync(output, input.IsPlatformSuperAdmin, ct);
    }

    public async Task<OutputReportCustomers> GetCustomersAsync(InputReportCustomers input, CancellationToken ct = default)
    {
        var locale = await PrepareRangeAsync(input.ActorFkStore, input.DateFrom, input.DateTo, ct);
        input.FromUtc = locale.FromUtc;
        input.ToUtcExclusive = locale.ToUtcExclusive;
        input.StoreIds = await ResolveStoreIdsAsync(input.IsPlatformSuperAdmin, input.ActorFkStore, input.FkStore, ct);
        if (input.Top is not (10 or 25 or 50 or 100))
            input.Top = 25;

        var output = new OutputReportCustomers
        {
            Filter = new InReportCustomersView
            {
                DateFrom = locale.FromLocal.ToDateTime(TimeOnly.MinValue),
                DateTo = locale.ToLocal.ToDateTime(TimeOnly.MinValue),
                FkStore = DisplayStore(input.IsPlatformSuperAdmin, input.ActorFkStore, input.FkStore),
                Top = input.Top
            },
            Currency = locale.Currency,
            TimeZoneId = locale.TimeZoneId,
            ShowStorePicker = input.IsPlatformSuperAdmin
        };

        if (input.StoreIds is { Count: 0 })
            return await WithLookupsAsync(output, input.IsPlatformSuperAdmin, ct);

        var rows = await _repo.ListTopCustomersAsync(input, ct);
        output.Rows = rows;
        output.Chart = rows
            .Select(r => new OutputReportChartPoint { Label = r.CustomerName, Value = r.TotalSpend })
            .ToList();
        output.Kpis = new List<OutputReportKpi>
        {
            Kpi("Customers", rows.Count.ToString("N0", CultureInfo.InvariantCulture)),
            Kpi("Visits", rows.Sum(r => r.VisitCount).ToString("N0", CultureInfo.InvariantCulture)),
            Kpi("Spend", LocaleClock.Money(locale.Currency, rows.Sum(r => r.TotalSpend)))
        };

        return await WithLookupsAsync(output, input.IsPlatformSuperAdmin, ct);
    }

    private async Task<(string Currency, string TimeZoneId, TimeZoneInfo TimeZone)> LoadLocaleAsync(int actorFkStore, CancellationToken ct)
    {
        var fkStore = actorFkStore > 0 ? actorFkStore : (int?)null;
        var tzValue = await _settings.GetEffectiveValueAsync("locale.timezone", fkStore, ct);
        var currencyValue = await _settings.GetEffectiveValueAsync("locale.currency", fkStore, ct);
        var tz = LocaleClock.ResolveTimeZone(tzValue);
        return (LocaleClock.ResolveCurrency(currencyValue), tz.Id, tz);
    }

    private async Task<PreparedRange> PrepareRangeAsync(int actorFkStore, DateTime? from, DateTime? to, CancellationToken ct)
    {
        var (currency, timeZoneId, tz) = await LoadLocaleAsync(actorFkStore, ct);
        var range = LocaleClock.ResolveInclusiveRange(from, to, tz);
        return new PreparedRange(currency, timeZoneId, tz, range.FromUtc, range.ToUtcExclusive, range.FromLocal, range.ToLocal);
    }

    private async Task<List<int>?> ResolveStoreIdsAsync(
        bool isPlatformSuperAdmin,
        int actorFkStore,
        int requestedFkStore,
        CancellationToken ct)
    {
        if (isPlatformSuperAdmin)
        {
            if (requestedFkStore <= 0)
                return null;

            return await _repo.ActiveStoreExistsAsync(requestedFkStore, ct)
                ? new List<int> { requestedFkStore }
                : null;
        }

        return actorFkStore > 0 ? new List<int> { actorFkStore } : new List<int>();
    }

    private async Task<OutputReportSales> WithLookupsAsync(OutputReportSales output, bool showStores, CancellationToken ct)
    {
        if (showStores)
            output.Stores = await _repo.ListActiveStoresAsync(ct);
        return output;
    }

    private async Task<OutputReportProducts> WithLookupsAsync(OutputReportProducts output, CancellationToken ct)
    {
        if (output.ShowStorePicker)
            output.Stores = await _repo.ListActiveStoresAsync(ct);
        output.Categories = await _repo.ListActiveCategoriesAsync(ct);
        return output;
    }

    private async Task<OutputReportStockValuation> WithLookupsAsync(OutputReportStockValuation output, CancellationToken ct)
    {
        if (output.ShowStorePicker)
            output.Stores = await _repo.ListActiveStoresAsync(ct);
        output.Categories = await _repo.ListActiveCategoriesAsync(ct);
        return output;
    }

    private async Task<T> WithLookupsAsync<T>(T output, bool showStores, CancellationToken ct)
        where T : class
    {
        if (!showStores)
            return output;

        var stores = await _repo.ListActiveStoresAsync(ct);
        switch (output)
        {
            case OutputReportTax tax:
                tax.Stores = stores;
                break;
            case OutputReportPurchases purchases:
                purchases.Stores = stores;
                break;
            case OutputReportCustomers customers:
                customers.Stores = stores;
                break;
        }

        return output;
    }

    private OutputReportSales EmptySales(InputReportSales input, PreparedRange locale, string groupBy) =>
        new()
        {
            Filter = new InReportSalesView
            {
                DateFrom = locale.FromLocal.ToDateTime(TimeOnly.MinValue),
                DateTo = locale.ToLocal.ToDateTime(TimeOnly.MinValue),
                FkStore = DisplayStore(input),
                GroupBy = groupBy
            },
            Currency = locale.Currency,
            TimeZoneId = locale.TimeZoneId,
            ShowStorePicker = input.IsPlatformSuperAdmin
        };

    private static int DisplayStore(InputReportSales input) =>
        DisplayStore(input.IsPlatformSuperAdmin, input.ActorFkStore, input.FkStore);

    private static int DisplayStore(bool isSuperAdmin, int actorFkStore, int requested) =>
        isSuperAdmin ? (requested > 0 ? requested : 0) : actorFkStore;

    private static OutputReportKpi Kpi(string label, string value, string? hint = null) =>
        new() { Label = label, Value = value, Hint = hint };

    private static OutputReportIndexCard Card(string title, string description, string action, string icon) =>
        new() { Title = title, Description = description, Action = action, Icon = icon };

    private static string BucketKey(TimeZoneInfo tz, DateTime utc, string groupBy)
    {
        var local = LocaleClock.ToLocal(utc, tz);
        return string.Equals(groupBy, "month", StringComparison.OrdinalIgnoreCase)
            ? local.ToString("yyyy-MM", CultureInfo.InvariantCulture)
            : local.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }

    private static DateOnly ParseBucketDate(string key, string groupBy)
    {
        if (string.Equals(groupBy, "month", StringComparison.OrdinalIgnoreCase)
            && DateOnly.TryParseExact(key + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var month))
        {
            return month;
        }

        return DateOnly.TryParseExact(key, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var day)
            ? day
            : DateOnly.MinValue;
    }

    private static string FormatBucket(string key, string groupBy)
    {
        var date = ParseBucketDate(key, groupBy);
        if (date == DateOnly.MinValue)
            return key;

        return string.Equals(groupBy, "month", StringComparison.OrdinalIgnoreCase)
            ? date.ToString("MMM yyyy", CultureInfo.InvariantCulture)
            : date.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
    }

    private sealed record PreparedRange(
        string Currency,
        string TimeZoneId,
        TimeZoneInfo TimeZone,
        DateTime FromUtc,
        DateTime ToUtcExclusive,
        DateOnly FromLocal,
        DateOnly ToLocal);
}
