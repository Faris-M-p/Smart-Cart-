using System.Globalization;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

/// <summary>
/// Home dashboard orchestration: store KPIs, a setup page, or a welcome page.
/// Store metrics use locale.timezone business days converted to UTC for queries.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repo;
    private readonly ISettingService _settings;

    public DashboardService(
        IDashboardRepository repo,
        ISettingService settings)
    {
        _repo = repo;
        _settings = settings;
    }

    public async Task<OutputDashboardSelect> GetHomeAsync(InputDashboardSelect input, CancellationToken ct = default)
    {
        var (currency, timeZoneId, tz) = await LoadLocaleAsync(input.ActorFkStore, ct);
        var actions = BuildQuickActions(input);
        var generated = DateTime.UtcNow;

        if (input.ActorFkStore <= 0)
        {
            return new OutputDashboardSelect
            {
                Mode = input.IsPlatformSuperAdmin ? "setup" : "welcome",
                Currency = currency,
                TimeZoneId = timeZoneId,
                StoreName = input.StoreName,
                DisplayName = input.IsPlatformSuperAdmin ? "Get started" : "Welcome",
                CanRefresh = false,
                QuickActions = input.IsPlatformSuperAdmin ? SetupActions() : actions,
                GeneratedAt = generated
            };
        }

        if (!input.HasDashboardView)
        {
            return new OutputDashboardSelect
            {
                Mode = "welcome",
                Currency = currency,
                TimeZoneId = timeZoneId,
                StoreName = input.StoreName,
                DisplayName = "Welcome",
                CanRefresh = false,
                QuickActions = actions,
                GeneratedAt = generated
            };
        }

        var snapshot = await LoadStoreSnapshotAsync(input.ActorFkStore, currency, timeZoneId, tz, ct);
        return new OutputDashboardSelect
        {
            Mode = "store",
            Currency = snapshot.Currency,
            TimeZoneId = snapshot.TimeZoneId,
            StoreName = input.StoreName,
            DisplayName = input.StoreName ?? "Store",
            CanRefresh = true,
            QuickActions = actions,
            Kpis = snapshot.Kpis,
            Trend7 = snapshot.Trend7,
            Trend30 = snapshot.Trend30,
            Payments = snapshot.Payments,
            TopProducts = snapshot.TopProducts,
            RecentInvoices = snapshot.RecentInvoices,
            LowStock = snapshot.LowStock,
            GeneratedAt = snapshot.GeneratedAt
        };
    }

    public async Task<OutputDashboardSnapshot> GetSnapshotAsync(InputDashboardSnapshot input, CancellationToken ct = default)
    {
        var (currency, timeZoneId, tz) = await LoadLocaleAsync(input.ActorFkStore, ct);
        if (input.ActorFkStore <= 0 && !input.IsPlatformSuperAdmin)
        {
            return new OutputDashboardSnapshot
            {
                Currency = currency,
                TimeZoneId = timeZoneId,
                GeneratedAt = DateTime.UtcNow
            };
        }

        var storeId = input.ActorFkStore;
        if (storeId <= 0)
        {
            return new OutputDashboardSnapshot
            {
                Currency = currency,
                TimeZoneId = timeZoneId,
                GeneratedAt = DateTime.UtcNow
            };
        }

        return await LoadStoreSnapshotAsync(storeId, currency, timeZoneId, tz, ct);
    }

    private async Task<OutputDashboardSnapshot> LoadStoreSnapshotAsync(
        int fkStore,
        string currency,
        string timeZoneId,
        TimeZoneInfo tz,
        CancellationToken ct)
    {
        var storeIds = new List<int> { fkStore };
        var today = LocaleClock.LocalToday(tz);
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var trendStart = today.AddDays(-29);
        var factsFrom = trendStart < monthStart ? trendStart : monthStart;
        if (factsFrom > today.AddDays(-1))
            factsFrom = today.AddDays(-1);

        var fromUtc = LocaleClock.DayBounds(factsFrom, tz).FromUtc;
        var toUtcExclusive = LocaleClock.DayBounds(today, tz).ToUtcExclusive;
        var monthFromUtc = LocaleClock.DayBounds(monthStart, tz).FromUtc;
        var todayBounds = LocaleClock.DayBounds(today, tz);
        var yesterdayBounds = LocaleClock.DayBounds(today.AddDays(-1), tz);

        var counts = await _repo.GetOperationalCountsAsync(storeIds, ct);
        var facts = await _repo.ListSaleFactsAsync(storeIds, fromUtc, toUtcExclusive, ct);
        var payments = await _repo.ListPaymentsAsync(storeIds, monthFromUtc, toUtcExclusive, ct);
        var topProducts = await _repo.ListTopProductsAsync(storeIds, monthFromUtc, toUtcExclusive, 5, ct);
        var recent = await _repo.ListRecentInvoicesAsync(storeIds, 8, ct);
        var lowStock = await _repo.ListLowStockAsync(storeIds, 8, ct);

        var todaySales = SumFacts(facts, tz, todayBounds.FromUtc, todayBounds.ToUtcExclusive);
        var yesterdaySales = SumFacts(facts, tz, yesterdayBounds.FromUtc, yesterdayBounds.ToUtcExclusive);
        var monthSales = SumFacts(facts, tz, monthFromUtc, toUtcExclusive);
        var todayInvoices = facts.Count(f => f.SaleDate >= todayBounds.FromUtc && f.SaleDate < todayBounds.ToUtcExclusive);

        counts.SalesToday = todaySales;
        counts.SalesYesterday = yesterdaySales;
        counts.SalesMonth = monthSales;
        counts.InvoiceCountToday = todayInvoices;
        counts.TodayChangePercent = PercentChange(todaySales, yesterdaySales);

        return new OutputDashboardSnapshot
        {
            Currency = currency,
            TimeZoneId = timeZoneId,
            Kpis = counts,
            Trend7 = FillTrend(facts, tz, today.AddDays(-6), today),
            Trend30 = FillTrend(facts, tz, today.AddDays(-29), today),
            Payments = payments,
            TopProducts = topProducts,
            RecentInvoices = recent,
            LowStock = lowStock,
            GeneratedAt = DateTime.UtcNow
        };
    }

    private async Task<(string Currency, string TimeZoneId, TimeZoneInfo TimeZone)> LoadLocaleAsync(int actorFkStore, CancellationToken ct)
    {
        var fkStore = actorFkStore > 0 ? actorFkStore : (int?)null;
        var tzValue = await _settings.GetEffectiveValueAsync("locale.timezone", fkStore, ct);
        var currencyValue = await _settings.GetEffectiveValueAsync("locale.currency", fkStore, ct);
        var tz = LocaleClock.ResolveTimeZone(tzValue);
        return (LocaleClock.ResolveCurrency(currencyValue), tz.Id, tz);
    }

    private static List<OutputDashboardQuickAction> BuildQuickActions(InputDashboardSelect input)
    {
        var list = new List<OutputDashboardQuickAction>();
        if (input.CanPosSell && input.ModulePos)
            list.Add(Action("Open POS", "Sales", "Pos", "bi-cash-register"));
        if (input.CanProductsView && input.ModuleProducts)
            list.Add(Action("Products", "Product", "Index", "bi-box"));
        if (input.CanPurchasesManage && input.ModulePurchases)
            list.Add(Action("New purchase", "Purchase", "Create", "bi-truck"));
        if (input.CanInventoryView && input.ModuleInventory)
            list.Add(Action("Low stock", "Inventory", "LowStock", "bi-exclamation-triangle"));
        if (input.CanCustomersView && input.ModuleCustomers)
            list.Add(Action("Customers", "Customers", "Index", "bi-people"));
        if (input.CanReportsView && input.ModuleReports)
            list.Add(Action("Reports", "Reports", "Index", "bi-graph-up"));
        if (input.CanSettingsManage && input.ModuleSettings)
            list.Add(Action("Settings", "Setting", "Index", "bi-gear"));
        if (input.CanUsersManage && input.ModuleUsers)
            list.Add(Action("Employees", "Employee", "Index", "bi-person-badge"));
        return list;
    }

    private static List<OutputDashboardQuickAction> SetupActions() =>
        new()
        {
            Action("Company", "Company", "Index", "bi-building"),
            Action("Create store", "Store", "Create", "bi-shop"),
            Action("License", "License", "Index", "bi-award"),
            Action("Modules", "Module", "Index", "bi-grid")
        };

    private static OutputDashboardQuickAction Action(string title, string controller, string action, string icon) =>
        new()
        {
            Title = title,
            Controller = controller,
            Action = action,
            Icon = icon
        };

    private static decimal SumFacts(IEnumerable<OutputDashboardSaleFact> facts, TimeZoneInfo tz, DateTime fromUtc, DateTime toUtcExclusive)
    {
        _ = tz;
        return facts.Where(f => f.SaleDate >= fromUtc && f.SaleDate < toUtcExclusive).Sum(f => f.TotalAmount);
    }

    private static decimal PercentChange(decimal today, decimal yesterday)
    {
        if (yesterday == 0)
            return today == 0 ? 0 : 100;
        return Math.Round((today - yesterday) / yesterday * 100m, 1, MidpointRounding.AwayFromZero);
    }

    private static List<OutputDashboardTrendPoint> FillTrend(
        IReadOnlyList<OutputDashboardSaleFact> facts,
        TimeZoneInfo tz,
        DateOnly from,
        DateOnly to)
    {
        var byDay = facts
            .GroupBy(f => DateOnly.FromDateTime(LocaleClock.ToLocal(f.SaleDate, tz)))
            .ToDictionary(g => g.Key, g => new { Net = g.Sum(x => x.TotalAmount), Count = g.Count() });

        var points = new List<OutputDashboardTrendPoint>();
        for (var day = from; day <= to; day = day.AddDays(1))
        {
            byDay.TryGetValue(day, out var bucket);
            points.Add(new OutputDashboardTrendPoint
            {
                Date = day,
                Label = day.ToString("dd MMM", CultureInfo.InvariantCulture),
                NetSales = bucket?.Net ?? 0,
                InvoiceCount = bucket?.Count ?? 0
            });
        }

        return points;
    }
}
