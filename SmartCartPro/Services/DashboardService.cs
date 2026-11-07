using System.Globalization;
using Microsoft.Extensions.Caching.Memory;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

/// <summary>
/// Dashboard orchestration service for SmartCart Pro.
/// Implements fast Essential view loading and lazy-loaded Advanced analytics with IMemoryCache.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repo;
    private readonly ISettingService _settings;
    private readonly IMemoryCache _cache;

    private static readonly TimeSpan EssentialsTtl = TimeSpan.FromSeconds(60);
    private static readonly TimeSpan AdvancedTtl = TimeSpan.FromMinutes(5);

    public DashboardService(
        IDashboardRepository repo,
        ISettingService settings,
        IMemoryCache cache)
    {
        _repo = repo;
        _settings = settings;
        _cache = cache;
    }

    #region Shell (Page Load)

    public async Task<OutputDashboardSelect> GetHomeAsync(InputDashboardSelect input, CancellationToken ct = default)
    {
        var (currency, timeZoneId, tz) = await LoadLocaleAsync(input.ActorFkStore, ct);
        var actions = BuildShellQuickActions(input);
        var generated = DateTime.UtcNow;

        if (input.ActorFkStore <= 0)
        {
            return new OutputDashboardSelect
            {
                Mode = input.IsPlatformSuperAdmin ? "setup" : "welcome",
                Role = input.IsPlatformSuperAdmin ? "owner" : "cashier",
                Currency = currency,
                TimeZoneId = timeZoneId,
                StoreName = input.StoreName,
                StoreId = 0,
                DisplayName = input.IsPlatformSuperAdmin ? "Get started" : "Welcome",
                CanRefresh = false,
                CanSeeAdvanced = false,
                CanSeeProfit = false,
                QuickActions = input.IsPlatformSuperAdmin ? SetupActions() : actions,
                GeneratedAt = generated
            };
        }

        var isCashierOnly = !input.HasDashboardView && input.CanPosSell;
        var role = isCashierOnly ? "cashier" : (input.CanReportsProfit ? "owner" : "manager");

        return new OutputDashboardSelect
        {
            Mode = isCashierOnly ? "cashier" : "store",
            Role = role,
            Currency = currency,
            TimeZoneId = timeZoneId,
            StoreName = input.StoreName,
            StoreId = input.ActorFkStore,
            DisplayName = isCashierOnly ? "My day" : (input.StoreName ?? "Dashboard"),
            CanRefresh = true,
            CanSeeAdvanced = !isCashierOnly,
            CanSeeProfit = input.CanReportsProfit,
            ShowPosMetrics = input.ModulePos,
            ShowLowStock = input.FeatureLowStock,
            ShowPurchases = input.FeaturePurchases,
            ShowOnlineOrders = input.FeatureOnlineOrders,
            ShowCashSession = input.FeatureCashSession,
            QuickActions = actions,
            GeneratedAt = generated
        };
    }

    #endregion

    #region Essentials

    public async Task<OutputDashboardEssentials> GetEssentialsAsync(InputDashboardEssentials input, CancellationToken ct = default)
    {
        var storeId = input.ActorFkStore;
        var rKey = (input.Range ?? "today").ToLowerInvariant().Trim();
        var isCashier = !input.HasDashboardView && input.CanPosSell;
        var scope = isCashier ? $"user:{input.ActorUserId}" : "store";
        var cacheKey = $"dash:essentials:{storeId}:{rKey}:{scope}";

        if (!input.Refresh && _cache.TryGetValue(cacheKey, out OutputDashboardEssentials? cached) && cached is not null)
        {
            return cached;
        }

        var (currency, timeZoneId, tz) = await LoadLocaleAsync(storeId, ct);
        var (curFromUtc, curToUtc, prevFromUtc, prevToUtc, rangeLabel, cmpLabel, daysCount) = ResolveRangeBounds(input.Range, tz);
        int? actorUserId = isCashier ? input.ActorUserId : null;

        // 1. Sales & Returns for current and previous period
        var (curSales, curBills) = await _repo.GetSalesSummaryAsync(storeId, actorUserId, curFromUtc, curToUtc, ct);
        var (curReturns, curRetCount) = await _repo.GetReturnsSummaryAsync(storeId, actorUserId, curFromUtc, curToUtc, ct);
        var (prevSales, prevBills) = await _repo.GetSalesSummaryAsync(storeId, actorUserId, prevFromUtc, prevToUtc, ct);
        var (prevReturns, prevRetCount) = await _repo.GetReturnsSummaryAsync(storeId, actorUserId, prevFromUtc, prevToUtc, ct);

        var netSales = Math.Max(0m, curSales - curReturns);
        var netSalesPrev = Math.Max(0m, prevSales - prevReturns);
        var netSalesChange = PercentChange(netSales, netSalesPrev);

        var billCount = curBills;
        var billCountPrev = prevBills;
        var billChange = PercentChange(billCount, billCountPrev);

        var avgBill = billCount > 0 ? Math.Round(netSales / billCount, 2) : 0m;
        var avgBillPrev = billCountPrev > 0 ? Math.Round(netSalesPrev / billCountPrev, 2) : 0m;
        var avgBillChange = PercentChange(avgBill, avgBillPrev);

        var retChange = PercentChange(curReturns, prevReturns);

        // 2. Operational counts & open session
        var opStatus = await _repo.GetOperationalStatusAsync(storeId, actorUserId, ct);
        var drawerCash = opStatus.OpenCashSessions > 0 ? opStatus.OpenExpectedCash : 0m;
        var drawerStatus = opStatus.OpenCashSessions > 0 ? "Open" : "Closed";

        // 3. Sparklines (last 7 days)
        var today = LocaleClock.LocalToday(tz);
        var sparkFrom = today.AddDays(-6);
        var sparkFromUtc = LocaleClock.DayBounds(sparkFrom, tz).FromUtc;
        var sparkToUtc = LocaleClock.DayBounds(today, tz).ToUtcExclusive;
        var sparkFacts = await _repo.ListSaleFactsAsync(storeId, actorUserId, sparkFromUtc, sparkToUtc, ct);
        var (salesSpark, billsSpark) = BuildSparklines(sparkFacts, tz, sparkFrom, today);

        // 4. KPI Cards
        var kpiCards = new List<OutputDashboardKpiCard>();
        if (isCashier)
        {
            kpiCards.Add(new OutputDashboardKpiCard
            {
                Key = "sales",
                Title = "My sales",
                Icon = "rupee",
                RawValue = netSales,
                FormattedValue = FormatInr(netSales),
                Subtitle = cmpLabel,
                DeltaPercent = netSalesChange,
                Sparkline = salesSpark
            });
            kpiCards.Add(new OutputDashboardKpiCard
            {
                Key = "bills",
                Title = "My bills",
                Icon = "receipt",
                RawValue = billCount,
                FormattedValue = FormatCount(billCount),
                Subtitle = $"Avg bill {FormatInr(avgBill)}",
                DeltaPercent = billChange,
                Sparkline = billsSpark
            });
            kpiCards.Add(new OutputDashboardKpiCard
            {
                Key = "returns",
                Title = "My returns",
                Icon = "ret",
                RawValue = curReturns,
                FormattedValue = FormatInr(curReturns),
                Subtitle = $"{curRetCount} return{(curRetCount == 1 ? "" : "s")}",
                DeltaPercent = retChange,
                InvertDelta = true
            });
            kpiCards.Add(new OutputDashboardKpiCard
            {
                Key = "drawer",
                Title = "Cash in drawer",
                Icon = "cash",
                RawValue = drawerCash,
                FormattedValue = FormatInr(drawerCash),
                Subtitle = opStatus.OpenCashSessions > 0 ? "Session active" : "Session closed",
                LinkText = opStatus.OpenCashSessions > 0 ? "Close session" : "Open session",
                LinkUrl = "/Sales/CashSession"
            });
        }
        else
        {
            kpiCards.Add(new OutputDashboardKpiCard
            {
                Key = "sales",
                Title = "Net sales",
                Icon = "rupee",
                RawValue = netSales,
                FormattedValue = FormatInr(netSales),
                Subtitle = cmpLabel,
                DeltaPercent = netSalesChange,
                Sparkline = salesSpark
            });
            kpiCards.Add(new OutputDashboardKpiCard
            {
                Key = "bills",
                Title = "Bills",
                Icon = "receipt",
                RawValue = billCount,
                FormattedValue = FormatCount(billCount),
                Subtitle = $"Avg bill {FormatInr(avgBill)}",
                DeltaPercent = billChange,
                Sparkline = billsSpark
            });
            kpiCards.Add(new OutputDashboardKpiCard
            {
                Key = "returns",
                Title = "Returns",
                Icon = "ret",
                RawValue = curReturns,
                FormattedValue = FormatInr(curReturns),
                Subtitle = $"{curRetCount} return{(curRetCount == 1 ? "" : "s")}",
                DeltaPercent = retChange,
                InvertDelta = true
            });
            kpiCards.Add(new OutputDashboardKpiCard
            {
                Key = "lowstock",
                Title = "Low / out of stock",
                Icon = "alert",
                RawValue = opStatus.LowStockCount + opStatus.OutOfStockCount,
                FormattedValue = $"{opStatus.LowStockCount + opStatus.OutOfStockCount} <span style=\"font-size:15px;color:var(--down)\">· {opStatus.OutOfStockCount} out</span>",
                Subtitle = "Below reorder level",
                LinkText = "View low stock",
                LinkUrl = "/Inventory/LowStock"
            });
            kpiCards.Add(new OutputDashboardKpiCard
            {
                Key = "online",
                Title = "Online orders",
                Icon = "globe",
                RawValue = opStatus.PendingOnlineOrders,
                FormattedValue = $"{opStatus.PendingOnlineOrders} pending",
                Subtitle = opStatus.PendingOnlineOrders > 0 ? "Active orders" : "No orders waiting",
                LinkText = "View orders",
                LinkUrl = "/OnlineOrder/Index"
            });
            kpiCards.Add(new OutputDashboardKpiCard
            {
                Key = "session",
                Title = "Cash session",
                Icon = "cash",
                RawValue = drawerCash,
                FormattedValue = drawerStatus,
                Subtitle = opStatus.OpenCashSessions > 0 ? $"{opStatus.OpenCashierName ?? "Staff"} · expected {FormatInr(drawerCash)}" : "No open drawer",
                LinkText = "Session details",
                LinkUrl = "/Sales/CashSession"
            });
        }

        // 5. Sales Trend (Current & Previous)
        var curFacts = await _repo.ListSaleFactsAsync(storeId, actorUserId, curFromUtc, curToUtc, ct);
        var prevFacts = await _repo.ListSaleFactsAsync(storeId, actorUserId, prevFromUtc, prevToUtc, ct);

        List<OutputDashboardTrendPoint> trendCur, trendPrev;
        if (rKey == "today")
        {
            trendCur = BuildHourlyTrend(curFacts, tz, today);
            trendPrev = BuildHourlyTrend(prevFacts, tz, today.AddDays(-1));
        }
        else
        {
            var curStart = today.AddDays(-(daysCount - 1));
            var prevStart = curStart.AddDays(-daysCount);
            if (rKey == "month")
            {
                curStart = new DateOnly(today.Year, today.Month, 1);
                var prevMonth = today.AddMonths(-1);
                prevStart = new DateOnly(prevMonth.Year, prevMonth.Month, 1);
            }
            trendCur = BuildDailyTrend(curFacts, tz, curStart, daysCount);
            trendPrev = BuildDailyTrend(prevFacts, tz, prevStart, daysCount);
        }

        var salesTrend = new OutputDashboardSalesTrend
        {
            Title = isCashier ? "My sales trend" : "Sales trend",
            Subtitle = rangeLabel,
            CurrentPeriodLabel = rangeLabel,
            PreviousPeriodLabel = cmpLabel,
            Labels = trendCur.Select(t => t.Label).ToList(),
            CurrentValues = trendCur.Select(t => t.NetSales).ToList(),
            PreviousValues = trendPrev.Select(t => t.NetSales).ToList()
        };

        // 6. Payment modes
        var rawPayments = await _repo.ListPaymentsAsync(storeId, actorUserId, curFromUtc, curToUtc, ct);
        foreach (var p in rawPayments)
        {
            p.FormattedAmount = FormatInr(p.Amount);
        }
        var totalCollected = rawPayments.Sum(p => p.Amount);
        var paymentModes = new OutputDashboardPaymentModes
        {
            TotalCollected = totalCollected,
            FormattedTotal = FormatInrShort(totalCollected),
            PeriodLabel = rangeLabel,
            Items = rawPayments
        };

        // 7. Top 5 Products (empty for cashier)
        var topProducts = isCashier
            ? new List<OutputDashboardTopProduct>()
            : await _repo.ListTopProductsAsync(storeId, actorUserId, curFromUtc, curToUtc, 5, ct);

        foreach (var tp in topProducts)
        {
            tp.FormattedRevenue = FormatInrShort(tp.Revenue);
            tp.FormattedQuantity = FormatCount(tp.Quantity) + " units";
        }

        // 8. Low Stock (empty for cashier or if feature disabled)
        var lowStock = (isCashier || !input.FeatureLowStock)
            ? new List<OutputDashboardLowStockItem>()
            : await _repo.ListLowStockAsync(storeId, 5, ct);

        foreach (var ls in lowStock)
        {
            ls.CanCreatePo = input.CanPurchasesManage && input.FeaturePurchases;
            ls.CreatePoUrl = $"/Purchase/Create?fkProduct={ls.FkProduct}";
        }

        // 9. Recent Invoices
        var recentInvoices = await _repo.ListRecentInvoicesAsync(storeId, actorUserId, 5, ct);
        foreach (var inv in recentInvoices)
        {
            inv.FormattedAmount = FormatInr(inv.Amount);
        }

        // 10. Alerts
        var alerts = new List<OutputDashboardAlert>();

        if (!isCashier && input.FeatureLowStock && input.CanInventoryView && opStatus.OutOfStockCount > 0)
        {
            alerts.Add(new OutputDashboardAlert
            {
                Type = "red",
                Icon = "alert",
                Text = $"{opStatus.OutOfStockCount} item{(opStatus.OutOfStockCount == 1 ? " is" : "s are")} out of stock",
                LinkUrl = "/Inventory/LowStock",
                LinkText = "View"
            });
        }

        if (input.FeatureCashSession && input.CanPosSell)
        {
            var (lastVariance, cashierName) = await _repo.GetLastClosedSessionDifferenceAsync(storeId, ct);
            if (lastVariance.HasValue && lastVariance.Value != 0)
            {
                var diffAbs = Math.Abs(lastVariance.Value);
                alerts.Add(new OutputDashboardAlert
                {
                    Type = "amber",
                    Icon = "cash",
                    Text = $"Cash difference of {FormatInr(diffAbs)} at last session close ({cashierName ?? "Staff"})",
                    LinkUrl = "/Sales/CashSession",
                    LinkText = "Details"
                });
            }
        }

        if (input.FeatureOnlineOrders && input.CanPosSell)
        {
            var (pendingCount, oldestOrder) = await _repo.GetOldestPendingOnlineOrderAsync(storeId, ct);
            if (pendingCount > 0 && oldestOrder.HasValue && (DateTime.UtcNow - oldestOrder.Value).TotalHours >= 2)
            {
                alerts.Add(new OutputDashboardAlert
                {
                    Type = "amber",
                    Icon = "globe",
                    Text = $"{pendingCount} online order{(pendingCount == 1 ? "" : "s")} waiting over 2 hours",
                    LinkUrl = "/OnlineOrder/Index",
                    LinkText = "Open"
                });
            }
        }

        if (isCashier && input.FeatureCashSession)
        {
            var openTime = await _repo.GetCashierOpenSessionTimeAsync(storeId, input.ActorUserId, ct);
            if (openTime.HasValue && (DateTime.UtcNow - openTime.Value).TotalHours >= 8)
            {
                alerts.Add(new OutputDashboardAlert
                {
                    Type = "amber",
                    Icon = "clock",
                    Text = "Your cash session has been open for more than 8 hours",
                    LinkUrl = "/Sales/CashSession",
                    LinkText = "Close session"
                });
            }
        }

        if (input.IsPlatformSuperAdmin)
        {
            var licExpiry = await _repo.GetLicenseExpiryDateAsync(today, ct);
            if (licExpiry.HasValue)
            {
                var daysRemaining = licExpiry.Value.DayNumber - today.DayNumber;
                if (daysRemaining <= 15)
                {
                    alerts.Add(new OutputDashboardAlert
                    {
                        Type = daysRemaining <= 3 ? "red" : "amber",
                        Icon = "award",
                        Text = daysRemaining <= 0 ? "License has expired" : $"License expiring in {daysRemaining} day{(daysRemaining == 1 ? "" : "s")}",
                        LinkUrl = "/License/Index",
                        LinkText = "Renew"
                    });
                }
            }
        }

        // 11. Quick Actions
        var quickActions = BuildEssentialsQuickActions(input, isCashier);

        var result = new OutputDashboardEssentials
        {
            RangeKey = rKey,
            RangeLabel = rangeLabel,
            ComparisonLabel = cmpLabel,
            UpdatedTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz).ToString("hh:mm tt"),
            Role = isCashier ? "cashier" : "owner",
            Currency = currency,
            StoreName = input.StoreName ?? "Store",
            Alerts = alerts.Take(4).ToList(),
            QuickActions = quickActions,
            Kpis = kpiCards,
            SalesTrend = salesTrend,
            Payments = paymentModes,
            TopProducts = topProducts,
            ItemSalesReportUrl = "/Reports/ItemSales",
            LowStock = lowStock,
            AllLowStockUrl = "/Inventory/LowStock",
            RecentInvoices = recentInvoices,
            AllInvoicesUrl = "/Sales/Invoices"
        };

        _cache.Set(cacheKey, result, EssentialsTtl);
        return result;
    }

    public async Task<OutputDashboardSnapshot> GetSnapshotAsync(InputDashboardSnapshot input, CancellationToken ct = default)
    {
        var essentialsInput = new InputDashboardEssentials
        {
            ActorFkStore = input.ActorFkStore,
            ActorUserId = input.ActorUserId,
            Range = "today",
            Refresh = false,
            IsPlatformSuperAdmin = input.IsPlatformSuperAdmin,
            HasDashboardView = true,
            HasReportsView = true,
            CanPosSell = true,
            CanInventoryView = true,
            CanPurchasesManage = true,
            FeatureLowStock = input.FeatureLowStock,
            FeaturePurchases = input.FeaturePurchases,
            FeatureOnlineOrders = input.FeatureOnlineOrders,
            FeatureCashSession = input.FeatureCashSession
        };

        var essentials = await GetEssentialsAsync(essentialsInput, ct);

        return new OutputDashboardSnapshot
        {
            Currency = essentials.Currency,
            TimeZoneId = "Asia/Kolkata",
            Payments = essentials.Payments.Items,
            TopProducts = essentials.TopProducts.Select(p => new OutputDashboardProductPoint
            {
                FkProduct = p.FkProduct,
                ProductName = p.ProductName,
                Quantity = p.Quantity,
                Revenue = p.Revenue
            }).ToList(),
            RecentInvoices = essentials.RecentInvoices.Select(r => new OutputDashboardRecentInvoice
            {
                IdSale = r.IdSale,
                InvoiceNumber = r.InvoiceNumber,
                CustomerName = r.CustomerName,
                TotalAmount = r.Amount,
                SaleDate = DateTime.UtcNow
            }).ToList(),
            LowStock = essentials.LowStock.Select(l => new OutputDashboardLowStockRow
            {
                ProductName = l.ProductName,
                ProductCode = l.ProductCode,
                QuantityOnHand = l.QuantityOnHand,
                ReorderLevel = l.ReorderLevel
            }).ToList(),
            GeneratedAt = DateTime.UtcNow
        };
    }

    #endregion

    #region Advanced Endpoints

    public async Task<OutputDashboardAdvancedSales> GetAdvancedSalesAsync(InputDashboardAdvanced input, CancellationToken ct = default)
    {
        var storeId = input.ActorFkStore;
        var rKey = (input.Range ?? "today").ToLowerInvariant().Trim();
        var cacheKey = $"dash:sales:{storeId}:{rKey}";

        if (!input.Refresh && _cache.TryGetValue(cacheKey, out OutputDashboardAdvancedSales? cached) && cached is not null)
            return cached;

        var (_, _, tz) = await LoadLocaleAsync(storeId, ct);
        var (fromUtc, toUtc, _, _, rangeLabel, _, _) = ResolveRangeBounds(input.Range, tz);

        var today = LocaleClock.LocalToday(tz);
        var dowFromUtc = LocaleClock.DayBounds(today.AddDays(-27), tz).FromUtc;

        var hourly = await _repo.ListHourlySalesAsync(storeId, fromUtc, toUtc, tz, ct);
        var dow = await _repo.ListDayOfWeekSalesAsync(storeId, dowFromUtc, toUtc, tz, ct);
        var (counter, online) = await _repo.GetCounterVsOnlineSalesAsync(storeId, fromUtc, toUtc, ct);
        var top10Rev = await _repo.ListTopProductsAsync(storeId, null, fromUtc, toUtc, 10, ct);
        var top10Qty = await _repo.ListTopProductsByQuantityAsync(storeId, fromUtc, toUtc, 10, ct);

        foreach (var p in top10Rev)
        {
            p.FormattedRevenue = FormatInrShort(p.Revenue);
            p.FormattedQuantity = FormatCount(p.Quantity) + " units";
        }
        foreach (var p in top10Qty)
        {
            p.FormattedRevenue = FormatInrShort(p.Revenue);
            p.FormattedQuantity = FormatCount(p.Quantity) + " units";
        }

        var total = counter + online;
        var channels = new List<OutputDashboardChannelItem>
        {
            new()
            {
                Channel = "Counter (POS)",
                Sales = counter,
                FormattedSales = FormatInrShort(counter),
                Percentage = total == 0 ? 0 : Math.Round(counter * 100m / total, 1),
                Color = "var(--c1)"
            },
            new()
            {
                Channel = "Online",
                Sales = online,
                FormattedSales = FormatInrShort(online),
                Percentage = total == 0 ? 0 : Math.Round(online * 100m / total, 1),
                Color = "var(--c2)"
            }
        };

        var result = new OutputDashboardAdvancedSales
        {
            HourlyLabels = hourly.Select(h => h.Label).ToList(),
            HourlyValues = hourly.Select(h => h.Sales).ToList(),
            DayOfWeekLabels = dow.Select(d => d.DayName).ToList(),
            DayOfWeekValues = dow.Select(d => d.Sales).ToList(),
            Channels = channels,
            TotalSales = total,
            FormattedTotalSales = FormatInrShort(total),
            Top10Revenue = top10Rev,
            Top10Quantity = top10Qty
        };

        _cache.Set(cacheKey, result, AdvancedTtl);
        return result;
    }

    public async Task<OutputDashboardAdvancedCategories> GetAdvancedCategoriesAsync(InputDashboardAdvanced input, CancellationToken ct = default)
    {
        var storeId = input.ActorFkStore;
        var rKey = (input.Range ?? "today").ToLowerInvariant().Trim();
        var cacheKey = $"dash:categories:{storeId}:{rKey}";

        if (!input.Refresh && _cache.TryGetValue(cacheKey, out OutputDashboardAdvancedCategories? cached) && cached is not null)
            return cached;

        var (_, _, tz) = await LoadLocaleAsync(storeId, ct);
        var (fromUtc, toUtc, _, _, rangeLabel, _, _) = ResolveRangeBounds(input.Range, tz);

        var categories = await _repo.ListCategorySalesAsync(storeId, fromUtc, toUtc, 8, ct);
        foreach (var c in categories)
        {
            c.FormattedSales = FormatInrShort(c.Sales);
        }

        var result = new OutputDashboardAdvancedCategories
        {
            Categories = categories,
            CategoryReportUrl = "/Reports/CategorySales"
        };

        _cache.Set(cacheKey, result, AdvancedTtl);
        return result;
    }

    public async Task<OutputDashboardAdvancedStock> GetAdvancedStockAsync(InputDashboardAdvanced input, CancellationToken ct = default)
    {
        var storeId = input.ActorFkStore;
        var cacheKey = $"dash:stock:{storeId}";

        if (!input.Refresh && _cache.TryGetValue(cacheKey, out OutputDashboardAdvancedStock? cached) && cached is not null)
            return cached;

        var (inStock, low, outStock, nonMoving, total) = await _repo.GetStockHealthAsync(storeId, ct);

        var health = new List<OutputDashboardStockHealthItem>
        {
            new() { Status = "In stock", Count = inStock, Color = "var(--c7)" },
            new() { Status = "Low", Count = low, Color = "var(--c3)" },
            new() { Status = "Out of stock", Count = outStock, Color = "var(--c5)" },
            new() { Status = "Non-moving 30d", Count = nonMoving, Color = "var(--c8)" }
        };

        var result = new OutputDashboardAdvancedStock
        {
            TotalTracked = total,
            HealthItems = health
        };

        _cache.Set(cacheKey, result, AdvancedTtl);
        return result;
    }

    public async Task<OutputDashboardAdvancedPurchases> GetAdvancedPurchasesAsync(InputDashboardAdvanced input, CancellationToken ct = default)
    {
        var storeId = input.ActorFkStore;
        var (_, _, tz) = await LoadLocaleAsync(storeId, ct);
        var today = LocaleClock.LocalToday(tz);
        var monthStart = new DateOnly(today.Year, today.Month, 1);

        var cacheKey = $"dash:purchases:{storeId}:{monthStart}";

        if (!input.Refresh && _cache.TryGetValue(cacheKey, out OutputDashboardAdvancedPurchases? cached) && cached is not null)
            return cached;

        var raw = await _repo.ListSalesVsPurchases6MonthsAsync(storeId, monthStart, ct);

        var result = new OutputDashboardAdvancedPurchases
        {
            MonthLabels = raw.Select(m => new DateTime(m.Year, m.Month, 1).ToString("MMM")).ToList(),
            SalesValues = raw.Select(m => m.Sales).ToList(),
            PurchaseValues = raw.Select(m => m.Purchases).ToList()
        };

        _cache.Set(cacheKey, result, AdvancedTtl);
        return result;
    }

    public async Task<OutputDashboardAdvancedTeam> GetAdvancedTeamAsync(InputDashboardAdvanced input, CancellationToken ct = default)
    {
        var storeId = input.ActorFkStore;
        var rKey = (input.Range ?? "today").ToLowerInvariant().Trim();
        var cacheKey = $"dash:team:{storeId}:{rKey}";

        if (!input.Refresh && _cache.TryGetValue(cacheKey, out OutputDashboardAdvancedTeam? cached) && cached is not null)
            return cached;

        var (_, _, tz) = await LoadLocaleAsync(storeId, ct);
        var (fromUtc, toUtc, _, _, rangeLabel, _, _) = ResolveRangeBounds(input.Range, tz);

        var accessibleStoreIds = await _repo.GetAccessibleStoreIdsAsync(input.ActorUserId, input.IsPlatformSuperAdmin, ct);

        var cashiers = await _repo.ListCashierPerformanceAsync(storeId, fromUtc, toUtc, ct);
        foreach (var c in cashiers)
        {
            c.FormattedSales = FormatInr(c.SalesTotal);
            c.FormattedAvgBill = FormatInr(c.AvgBillValue);
        }

        var storeComparison = await _repo.ListStoreComparisonAsync(accessibleStoreIds, fromUtc, toUtc, ct);
        foreach (var s in storeComparison)
        {
            s.FormattedSales = FormatInr(s.SalesTotal);
        }

        var result = new OutputDashboardAdvancedTeam
        {
            Cashiers = cashiers,
            StoreComparison = storeComparison
        };

        _cache.Set(cacheKey, result, AdvancedTtl);
        return result;
    }

    public async Task<OutputDashboardAdvancedProfit> GetAdvancedProfitAsync(InputDashboardAdvanced input, CancellationToken ct = default)
    {
        if (!input.CanSeeProfit)
        {
            return new OutputDashboardAdvancedProfit();
        }

        var storeId = input.ActorFkStore;
        var rKey = (input.Range ?? "today").ToLowerInvariant().Trim();
        var cacheKey = $"dash:profit:{storeId}:{rKey}:profit";

        if (!input.Refresh && _cache.TryGetValue(cacheKey, out OutputDashboardAdvancedProfit? cached) && cached is not null)
            return cached;

        var (_, _, tz) = await LoadLocaleAsync(storeId, ct);
        var (fromUtc, toUtc, prevFromUtc, prevToUtc, rangeLabel, _, _) = ResolveRangeBounds(input.Range, tz);

        var (profit, net, margin) = await _repo.GetGrossProfitAsync(storeId, fromUtc, toUtc, ct);
        var (_, _, prevMargin) = await _repo.GetGrossProfitAsync(storeId, prevFromUtc, prevToUtc, ct);
        var categoryMargins = await _repo.ListCategoryMarginsAsync(storeId, fromUtc, toUtc, 4, ct);

        var result = new OutputDashboardAdvancedProfit
        {
            GrossProfit = profit,
            FormattedGrossProfit = FormatInr(profit),
            MarginPercent = margin,
            DeltaMarginPercent = Math.Round(margin - prevMargin, 1),
            CategoryMargins = categoryMargins
        };

        _cache.Set(cacheKey, result, AdvancedTtl);
        return result;
    }

    #endregion

    #region Helpers

    private async Task<(string Currency, string TimeZoneId, TimeZoneInfo TimeZone)> LoadLocaleAsync(int actorFkStore, CancellationToken ct)
    {
        var fkStore = actorFkStore > 0 ? actorFkStore : (int?)null;
        var tzValue = await _settings.GetEffectiveValueAsync("locale.timezone", fkStore, ct);
        var currencyValue = await _settings.GetEffectiveValueAsync("locale.currency", fkStore, ct);
        var tz = LocaleClock.ResolveTimeZone(tzValue);
        return (LocaleClock.ResolveCurrency(currencyValue), tz.Id, tz);
    }

    private static (DateTime FromUtc, DateTime ToUtcExclusive, DateTime PrevFromUtc, DateTime PrevToUtcExclusive, string RangeLabel, string ComparisonLabel, int DaysCount)
        ResolveRangeBounds(string? range, TimeZoneInfo tz)
    {
        var r = (range ?? "today").ToLowerInvariant().Trim();
        var today = LocaleClock.LocalToday(tz);

        DateOnly curStart, curEnd, prevStart, prevEnd;
        string rangeLabel, comparisonLabel;
        int daysCount;

        switch (r)
        {
            case "d7":
                curStart = today.AddDays(-6);
                curEnd = today;
                prevStart = today.AddDays(-13);
                prevEnd = today.AddDays(-7);
                rangeLabel = "Last 7 days";
                comparisonLabel = "vs previous 7 days";
                daysCount = 7;
                break;
            case "d30":
                curStart = today.AddDays(-29);
                curEnd = today;
                prevStart = today.AddDays(-59);
                prevEnd = today.AddDays(-30);
                rangeLabel = "Last 30 days";
                comparisonLabel = "vs previous 30 days";
                daysCount = 30;
                break;
            case "month":
                curStart = new DateOnly(today.Year, today.Month, 1);
                curEnd = today;
                var prevMonthDate = today.AddMonths(-1);
                prevStart = new DateOnly(prevMonthDate.Year, prevMonthDate.Month, 1);
                var prevMaxDays = DateTime.DaysInMonth(prevMonthDate.Year, prevMonthDate.Month);
                var prevDay = Math.Min(today.Day, prevMaxDays);
                prevEnd = new DateOnly(prevMonthDate.Year, prevMonthDate.Month, prevDay);
                rangeLabel = "This month";
                comparisonLabel = "vs same days last month";
                daysCount = today.Day;
                break;
            case "today":
            default:
                curStart = today;
                curEnd = today;
                prevStart = today.AddDays(-1);
                prevEnd = today.AddDays(-1);
                rangeLabel = "Today";
                comparisonLabel = "vs yesterday";
                daysCount = 1;
                break;
        }

        var fromUtc = LocaleClock.DayBounds(curStart, tz).FromUtc;
        var toUtcExclusive = LocaleClock.DayBounds(curEnd, tz).ToUtcExclusive;
        var prevFromUtc = LocaleClock.DayBounds(prevStart, tz).FromUtc;
        var prevToUtcExclusive = LocaleClock.DayBounds(prevEnd, tz).ToUtcExclusive;

        return (fromUtc, toUtcExclusive, prevFromUtc, prevToUtcExclusive, rangeLabel, comparisonLabel, daysCount);
    }

    private static decimal PercentChange(decimal current, decimal previous)
    {
        if (previous == 0)
            return current == 0 ? 0 : 100m;
        return Math.Round((current - previous) / previous * 100m, 1, MidpointRounding.AwayFromZero);
    }

    private static (List<decimal> Sales, List<decimal> Bills) BuildSparklines(
        IReadOnlyList<OutputDashboardSaleFact> facts,
        TimeZoneInfo tz,
        DateOnly from,
        DateOnly to)
    {
        var byDay = facts
            .GroupBy(f => DateOnly.FromDateTime(LocaleClock.ToLocal(f.SaleDate, tz)))
            .ToDictionary(g => g.Key, g => new { Net = g.Sum(x => x.TotalAmount), Count = g.Count() });

        var sales = new List<decimal>();
        var bills = new List<decimal>();

        for (var day = from; day <= to; day = day.AddDays(1))
        {
            byDay.TryGetValue(day, out var bucket);
            sales.Add(bucket?.Net ?? 0m);
            bills.Add(bucket?.Count ?? 0);
        }

        return (sales, bills);
    }

    private static List<OutputDashboardTrendPoint> BuildHourlyTrend(
        IReadOnlyList<OutputDashboardSaleFact> facts,
        TimeZoneInfo tz,
        DateOnly day)
    {
        var byHour = facts
            .GroupBy(f => LocaleClock.ToLocal(f.SaleDate, tz).Hour)
            .ToDictionary(g => g.Key, g => new { Net = g.Sum(x => x.TotalAmount), Count = g.Count() });

        var points = new List<OutputDashboardTrendPoint>();
        for (var h = 8; h <= 22; h++)
        {
            byHour.TryGetValue(h, out var bucket);
            var label = (h % 12 == 0 ? 12 : h % 12) + (h < 12 ? "am" : "pm");
            points.Add(new OutputDashboardTrendPoint
            {
                Date = day,
                Label = label,
                NetSales = bucket?.Net ?? 0m,
                InvoiceCount = bucket?.Count ?? 0
            });
        }
        return points;
    }

    private static List<OutputDashboardTrendPoint> BuildDailyTrend(
        IReadOnlyList<OutputDashboardSaleFact> facts,
        TimeZoneInfo tz,
        DateOnly startDay,
        int count)
    {
        var byDay = facts
            .GroupBy(f => DateOnly.FromDateTime(LocaleClock.ToLocal(f.SaleDate, tz)))
            .ToDictionary(g => g.Key, g => new { Net = g.Sum(x => x.TotalAmount), Count = g.Count() });

        var points = new List<OutputDashboardTrendPoint>();
        for (var i = 0; i < count; i++)
        {
            var day = startDay.AddDays(i);
            byDay.TryGetValue(day, out var bucket);
            points.Add(new OutputDashboardTrendPoint
            {
                Date = day,
                Label = day.ToString("dd MMM", CultureInfo.InvariantCulture),
                NetSales = bucket?.Net ?? 0m,
                InvoiceCount = bucket?.Count ?? 0
            });
        }
        return points;
    }

    private static string FormatInr(decimal v)
    {
        var culture = new CultureInfo("en-IN");
        return v.ToString("C0", culture);
    }

    private static string FormatInrShort(decimal v)
    {
        var a = Math.Abs(v);
        if (a >= 10000000m) return "₹" + (v / 10000000m).ToString("0.00") + "Cr";
        if (a >= 100000m) return "₹" + (v / 100000m).ToString("0.00") + "L";
        if (a >= 1000m) return "₹" + (v / 1000m).ToString("0.0") + "K";
        return "₹" + Math.Round(v).ToString("N0", new CultureInfo("en-IN"));
    }

    private static string FormatCount(decimal v)
    {
        return v.ToString("N0", new CultureInfo("en-IN"));
    }

    private static List<OutputDashboardQuickAction> BuildShellQuickActions(InputDashboardSelect input)
    {
        var list = new List<OutputDashboardQuickAction>();
        var isCashier = !input.HasDashboardView && input.CanPosSell;

        if (input.CanPosSell && input.ModulePos)
            list.Add(Action("New sale", "Sales", "Pos", "cart"));
        if (!isCashier && input.CanPurchasesManage && input.FeaturePurchases)
            list.Add(Action("New purchase", "Purchase", "Create", "truck"));
        if (!isCashier && input.CanProductsView && input.ModuleProducts)
            list.Add(Action("Add product", "Product", "Create", "plus"));
        if (!isCashier && input.CanInventoryView && input.FeatureLowStock)
            list.Add(Action("Receive stock", "Inventory", "Index", "download"));
        if (input.CanPosSell && input.ModulePos)
            list.Add(Action("Invoices", "Sales", "Invoices", "receipt"));
        if (input.FeatureCashSession && input.CanPosSell)
            list.Add(Action("Cash session", "Sales", "CashSession", "cash"));

        return list;
    }

    private static List<OutputDashboardQuickAction> BuildEssentialsQuickActions(InputDashboardEssentials input, bool isCashier)
    {
        var list = new List<OutputDashboardQuickAction>();

        if (input.CanPosSell)
            list.Add(Action("New sale", "Sales", "Pos", "cart"));
        if (!isCashier && input.CanPurchasesManage && input.FeaturePurchases)
            list.Add(Action("New purchase", "Purchase", "Create", "truck"));
        if (!isCashier && input.CanInventoryView)
            list.Add(Action("Add product", "Product", "Create", "plus"));
        if (!isCashier && input.CanInventoryView && input.FeatureLowStock)
            list.Add(Action("Receive stock", "Inventory", "Index", "download"));
        if (input.CanPosSell)
            list.Add(Action("Invoices", "Sales", "Invoices", "receipt"));
        if (input.FeatureCashSession && input.CanPosSell)
            list.Add(Action("Cash session", "Sales", "CashSession", "cash"));

        return list;
    }

    private static List<OutputDashboardQuickAction> SetupActions() =>
        new()
        {
            Action("Company", "Company", "Index", "building"),
            Action("Create store", "Store", "Create", "shop"),
            Action("License", "License", "Index", "award"),
            Action("Modules", "Module", "Index", "grid")
        };

    private static OutputDashboardQuickAction Action(string title, string controller, string action, string icon) =>
        new()
        {
            Title = title,
            Controller = controller,
            Action = action,
            Icon = icon
        };

    #endregion
}
