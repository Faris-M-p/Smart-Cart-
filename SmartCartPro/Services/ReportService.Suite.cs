using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Services;

public partial class ReportService
{
    public async Task<OutputReportScreen> GetScreenAsync(InputReportQuery input, CancellationToken ct = default)
    {
        var def = ReportCatalog.Find(input.ReportKey) ?? ReportCatalog.All[0];
        var (currency, timeZoneId, tz) = await LoadLocaleAsync(input.ActorFkStore, ct);
        input.Currency = currency;
        input.TimeZone = tz;
        input.TimeZoneId = timeZoneId;

        var stores = await _suite.AllowedStoresAsync(input.ActorUserId, input.IsPlatformSuperAdmin, input.ActorFkStore, ct);
        var allowed = stores.Select(s => s.IdStore).ToList();
        string? notice = null;

        if (input.FkStore > 0 && !allowed.Contains(input.FkStore) && !input.IsPlatformSuperAdmin)
        {
            notice = "That store is not available for this sign-in.";
            input.FkStore = allowed.Contains(input.ActorFkStore) ? input.ActorFkStore : allowed.FirstOrDefault();
        }

        if (input.IsPlatformSuperAdmin && input.FkStore <= 0)
            input.StoreIds = null;
        else if (input.FkStore > 0)
            input.StoreIds = new List<int> { input.FkStore };
        else if (allowed.Count <= 1)
        {
            input.FkStore = allowed.FirstOrDefault();
            input.StoreIds = allowed;
        }
        else
            input.StoreIds = input.IsPlatformSuperAdmin ? null : allowed;

        if (string.IsNullOrWhiteSpace(input.GroupBy) && def.Group.Count > 0)
            input.GroupBy = def.Group[0].Value;
        if (string.IsNullOrWhiteSpace(input.Level) && def.Level.Count > 0)
            input.Level = def.Level[0].Value;
        if (string.IsNullOrWhiteSpace(input.Status) && def.Status.Count > 0)
            input.Status = def.Status[0].Value;
        if (input.Days is null or <= 0 && def.Days.Count > 0 && int.TryParse(def.Days[0].Value, out var days))
            input.Days = days;

        var screen = new OutputReportScreen
        {
            Key = def.Key,
            Action = def.Action,
            Section = def.Section,
            Title = def.Title,
            Description = def.Description,
            Currency = currency,
            TimeZoneId = timeZoneId,
            PrintedBy = input.PrintedBy,
            PrintedAtLocal = LocaleClock.ToLocal(DateTime.UtcNow, tz),
            CompanyName = await _suite.CompanyNameAsync(ct),
            Stores = stores,
            ShowDates = def.Filters.Contains("dates"),
            ShowStore = def.Filters.Contains("store") && stores.Count > 1,
            ShowCashier = def.Filters.Contains("cashier"),
            ShowCustomer = def.Filters.Contains("customer"),
            ShowPayment = def.Filters.Contains("payment"),
            ShowStatus = def.Filters.Contains("status"),
            ShowCategory = def.Filters.Contains("category"),
            ShowSubCategory = def.Filters.Contains("subcategory"),
            ShowBrand = def.Filters.Contains("brand"),
            ShowSupplier = def.Filters.Contains("supplier"),
            ShowSearch = def.Filters.Contains("search"),
            ShowLevel = def.Filters.Contains("level"),
            ShowGroup = def.Filters.Contains("group"),
            ShowMovement = def.Filters.Contains("movement"),
            ShowDays = def.Filters.Contains("days"),
            ShowZero = def.Filters.Contains("zero"),
            ShowNegative = def.Filters.Contains("negative"),
            ShowInStock = def.Filters.Contains("instock"),
            SearchLabel = def.SearchLabel,
            StatusOptions = def.Status.Select(c => new OutputReportChoice { Value = c.Value, Label = c.Label }).ToList(),
            LevelOptions = def.Level.Select(c => new OutputReportChoice { Value = c.Value, Label = c.Label }).ToList(),
            GroupOptions = def.Group.Select(c => new OutputReportChoice { Value = c.Value, Label = c.Label }).ToList(),
            MovementOptions = def.Movement.Select(c => new OutputReportChoice { Value = c.Value, Label = c.Label }).ToList(),
            DayOptions = def.Days.Select(c => new OutputReportChoice { Value = c.Value, Label = c.Label }).ToList(),
            Notice = notice
        };

        screen.StoreLabel = input.FkStore > 0
            ? stores.FirstOrDefault(s => s.IdStore == input.FkStore)?.StoreName ?? "Store"
            : stores.Count > 1 ? "All stores" : stores.FirstOrDefault()?.StoreName ?? "Store";

        if (def.Range != "none")
        {
            var today = LocaleClock.LocalToday(tz);
            DateOnly? from = input.DateFrom is { } f && f != default ? DateOnly.FromDateTime(f) : null;
            DateOnly? to = input.DateTo is { } t && t != default ? DateOnly.FromDateTime(t) : null;
            if (from is null && to is null)
            {
                if (def.Range == "month")
                    from = new DateOnly(today.Year, today.Month, 1);
                else
                    from = today;
                to = today;
            }

            from ??= to;
            to ??= from;
            if (to < from)
                screen.Error = "From date must be on or before the to date.";
            else if (to!.Value.DayNumber - from!.Value.DayNumber > LocaleClock.MaxRangeDays)
                screen.Error = "Date range cannot exceed 366 days.";
            else
            {
                input.FromUtc = LocaleClock.DayBounds(from!.Value, tz).FromUtc;
                input.ToUtcExclusive = LocaleClock.DayBounds(to!.Value, tz).ToUtcExclusive;
                input.FromLocal = from.Value;
                input.ToLocal = to.Value;
            }

            screen.QuickPicks = QuickPicks(today);
        }

        var pageSize = input.ForExport ? 20000 : input.PageSize is < 10 or > 100 ? 25 : input.PageSize;
        var page = input.ForExport || input.Page < 1 ? 1 : input.Page;
        input.Page = page;
        input.PageSize = pageSize;

        screen.Filter = new InReportQueryView
        {
            DateFrom = input.FromLocal == default ? input.DateFrom : input.FromLocal.ToDateTime(TimeOnly.MinValue),
            DateTo = input.ToLocal == default ? input.DateTo : input.ToLocal.ToDateTime(TimeOnly.MinValue),
            FkStore = input.FkStore,
            FkCategory = input.FkCategory,
            FkSubCategory = input.FkSubCategory,
            FkBrand = input.FkBrand,
            FkSupplier = input.FkSupplier,
            FkCustomer = input.FkCustomer,
            FkUser = input.FkUser,
            FkProduct = input.FkProduct,
            FkProductVariant = input.FkProductVariant,
            Search = input.Search,
            PaymentMode = input.PaymentMode,
            Status = input.Status,
            Level = input.Level,
            GroupBy = input.GroupBy,
            MovementType = input.MovementType,
            Days = input.Days,
            ShowZero = input.ShowZero,
            OnlyNegative = input.OnlyNegative,
            OnlyInStock = input.OnlyInStock,
            SortBy = input.SortBy,
            SortDir = input.SortDir,
            Page = page,
            PageSize = pageSize
        };
        screen.SortBy = input.SortBy ?? "";
        screen.SortDir = string.Equals(input.SortDir, "asc", StringComparison.OrdinalIgnoreCase) ? "asc" : "desc";
        screen.Route = RouteOf(screen.Filter);
        screen.Page = page;
        screen.PageSize = pageSize;

        if (string.IsNullOrEmpty(screen.Error))
        {
            if (def.ProfitOnly && !input.CanSeeProfit)
            {
                screen.Error = "You do not have permission to view cost and profit.";
            }
            else
            {
                var table = await _suite.QueryAsync(input, ct);
                screen.Columns = table.Columns;
                screen.Rows = table.Rows;
                screen.Totals = table.Totals;
                screen.Kpis = table.Kpis;
                screen.TotalCount = table.TotalCount;
                if (!string.IsNullOrWhiteSpace(table.Notice))
                    screen.Notice = string.IsNullOrWhiteSpace(screen.Notice) ? table.Notice : screen.Notice + " " + table.Notice;
                if (input.ForExport && table.TotalCount > table.Rows.Count)
                    screen.Notice = (screen.Notice ?? "") + " Export is limited to the first 20,000 rows.";
            }
        }

        screen.PageCount = screen.PageSize <= 0
            ? 1
            : Math.Max(1, (int)Math.Ceiling(screen.TotalCount / (double)Math.Max(screen.PageSize, 1)));
        await _suite.FillLookupsAsync(screen, ct);
        return screen;
    }

    private static List<OutputReportIndexCard> ReportCards(bool canSeeProfit) =>
        ReportCatalog.All
            .Where(d => !d.ProfitOnly || canSeeProfit)
            .Select(d => new OutputReportIndexCard
            {
                Section = d.Section,
                Title = d.Title,
                Description = d.Description,
                Action = d.Action,
                Icon = d.Icon,
                RequiresProfit = d.ProfitOnly
            })
            .ToList();

    private static List<OutputReportQuickPick> QuickPicks(DateOnly today)
    {
        var yesterday = today.AddDays(-1);
        var weekStart = today.AddDays(-(((int)today.DayOfWeek + 6) % 7));
        var monthStart = new DateOnly(today.Year, today.Month, 1);
        var lastMonthEnd = monthStart.AddDays(-1);
        var lastMonthStart = new DateOnly(lastMonthEnd.Year, lastMonthEnd.Month, 1);
        return new List<OutputReportQuickPick>
        {
            Pick("Today", today, today),
            Pick("Yesterday", yesterday, yesterday),
            Pick("This week", weekStart, today),
            Pick("This month", monthStart, today),
            Pick("Last month", lastMonthStart, lastMonthEnd)
        };
    }

    private static OutputReportQuickPick Pick(string label, DateOnly from, DateOnly to) =>
        new()
        {
            Label = label,
            From = from.ToString("yyyy-MM-dd"),
            To = to.ToString("yyyy-MM-dd")
        };

    private static Dictionary<string, string> RouteOf(InReportQueryView filter)
    {
        var route = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (filter.DateFrom is DateTime from)
            route["dateFrom"] = from.ToString("yyyy-MM-dd");
        if (filter.DateTo is DateTime to)
            route["dateTo"] = to.ToString("yyyy-MM-dd");
        if (filter.FkStore > 0) route["fkStore"] = filter.FkStore.ToString();
        if (filter.FkCategory is > 0) route["fkCategory"] = filter.FkCategory.Value.ToString();
        if (filter.FkSubCategory is > 0) route["fkSubCategory"] = filter.FkSubCategory.Value.ToString();
        if (filter.FkBrand is > 0) route["fkBrand"] = filter.FkBrand.Value.ToString();
        if (filter.FkSupplier is > 0) route["fkSupplier"] = filter.FkSupplier.Value.ToString();
        if (filter.FkCustomer is > 0) route["fkCustomer"] = filter.FkCustomer.Value.ToString();
        if (filter.FkUser is > 0) route["fkUser"] = filter.FkUser.Value.ToString();
        if (filter.FkProduct is > 0) route["fkProduct"] = filter.FkProduct.Value.ToString();
        if (filter.FkProductVariant is > 0) route["fkProductVariant"] = filter.FkProductVariant.Value.ToString();
        if (!string.IsNullOrWhiteSpace(filter.Search)) route["search"] = filter.Search.Trim();
        if (!string.IsNullOrWhiteSpace(filter.PaymentMode)) route["paymentMode"] = filter.PaymentMode;
        if (!string.IsNullOrWhiteSpace(filter.Status)) route["status"] = filter.Status;
        if (!string.IsNullOrWhiteSpace(filter.Level)) route["level"] = filter.Level;
        if (!string.IsNullOrWhiteSpace(filter.GroupBy)) route["groupBy"] = filter.GroupBy;
        if (!string.IsNullOrWhiteSpace(filter.MovementType)) route["movementType"] = filter.MovementType;
        if (filter.Days is > 0) route["days"] = filter.Days.Value.ToString();
        if (filter.ShowZero) route["showZero"] = "true";
        if (filter.OnlyNegative) route["onlyNegative"] = "true";
        if (!filter.OnlyInStock) route["onlyInStock"] = "false";
        if (!string.IsNullOrWhiteSpace(filter.SortBy)) route["sortBy"] = filter.SortBy;
        if (!string.IsNullOrWhiteSpace(filter.SortDir)) route["sortDir"] = filter.SortDir;
        return route;
    }
}
