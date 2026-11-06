using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IDashboardService _svc;
    private readonly ICurrentUserAccess _access;

    public DashboardController(IDashboardService svc, ICurrentUserAccess access)
    {
        _svc = svc;
        _access = access;
    }

    /// <summary>
    /// Loads lightweight, high-value Essential widgets in a single fast call.
    /// Content is tailored per signed-in role (Cashier: "My day" vs Store Manager / Owner).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Essentials(string range = "today", bool refresh = false, CancellationToken ct = default)
    {
        var input = await BuildEssentialsInputAsync(range, refresh, ct);
        var data = await _svc.GetEssentialsAsync(input, ct);
        return CommonHelpers.SuccessResponse("Dashboard essentials loaded.", data);
    }

    /// <summary>
    /// Backwards compatibility snapshot endpoint. Kept so legacy callers do not break.
    /// </summary>
    [HttpGet]
    [Obsolete("Use Essentials instead. Kept for backwards compatibility.")]
    public async Task<IActionResult> Snapshot(string range = "today", bool refresh = false, CancellationToken ct = default)
    {
        var input = await BuildEssentialsInputAsync(range, refresh, ct);
        var data = await _svc.GetEssentialsAsync(input, ct);
        return CommonHelpers.SuccessResponse("Dashboard snapshot.", data);
    }

    /// <summary>
    /// Advanced: Hourly sales, day-of-week, counter vs online, top 10 products (revenue & qty).
    /// Gate: dashboard.view + reports.view
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> AdvancedSales(string range = "today", bool refresh = false, CancellationToken ct = default)
    {
        if (!await HasAdvancedAccessAsync(ct))
            return Forbidden("Advanced analytics require dashboard.view and reports.view permissions.");

        var input = await BuildAdvancedInputAsync(range, refresh, ct);
        var data = await _svc.GetAdvancedSalesAsync(input, ct);
        return CommonHelpers.SuccessResponse("Advanced sales loaded.", data);
    }

    /// <summary>
    /// Advanced: Category-wise sales (top 8 + Others).
    /// Gate: dashboard.view + reports.view
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> AdvancedCategories(string range = "today", bool refresh = false, CancellationToken ct = default)
    {
        if (!await HasAdvancedAccessAsync(ct))
            return Forbidden("Advanced analytics require dashboard.view and reports.view permissions.");

        var input = await BuildAdvancedInputAsync(range, refresh, ct);
        var data = await _svc.GetAdvancedCategoriesAsync(input, ct);
        return CommonHelpers.SuccessResponse("Advanced categories loaded.", data);
    }

    /// <summary>
    /// Advanced: Stock health counts (in stock / low / out / non-moving 30d).
    /// Gate: dashboard.view + reports.view + INVENTORY enabled
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> AdvancedStock(bool refresh = false, CancellationToken ct = default)
    {
        if (!await HasAdvancedAccessAsync(ct))
            return Forbidden("Advanced analytics require dashboard.view and reports.view permissions.");

        if (!await _access.ModuleEnabledAsync("INVENTORY", ct))
            return Forbidden("Inventory module is disabled.");

        var input = await BuildAdvancedInputAsync("today", refresh, ct);
        var data = await _svc.GetAdvancedStockAsync(input, ct);
        return CommonHelpers.SuccessResponse("Advanced stock loaded.", data);
    }

    /// <summary>
    /// Advanced: Sales vs purchases, last 6 months.
    /// Gate: dashboard.view + reports.view + PURCHASES enabled
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> AdvancedPurchases(bool refresh = false, CancellationToken ct = default)
    {
        if (!await HasAdvancedAccessAsync(ct))
            return Forbidden("Advanced analytics require dashboard.view and reports.view permissions.");

        if (!await _access.ModuleEnabledAsync("PURCHASES", ct))
            return Forbidden("Purchases module is disabled.");

        var input = await BuildAdvancedInputAsync("today", refresh, ct);
        var data = await _svc.GetAdvancedPurchasesAsync(input, ct);
        return CommonHelpers.SuccessResponse("Advanced purchases loaded.", data);
    }

    /// <summary>
    /// Advanced: Cashier performance & store comparison (if 2+ stores).
    /// Gate: dashboard.view + reports.view
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> AdvancedTeam(string range = "today", bool refresh = false, CancellationToken ct = default)
    {
        if (!await HasAdvancedAccessAsync(ct))
            return Forbidden("Advanced analytics require dashboard.view and reports.view permissions.");

        var input = await BuildAdvancedInputAsync(range, refresh, ct);
        var data = await _svc.GetAdvancedTeamAsync(input, ct);
        return CommonHelpers.SuccessResponse("Advanced team loaded.", data);
    }

    /// <summary>
    /// Advanced: Gross profit, margin %, change, margin by category.
    /// Gate: reports.profit
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> AdvancedProfit(string range = "today", bool refresh = false, CancellationToken ct = default)
    {
        if (!await _access.CanAsync("reports.profit", ct))
            return Forbidden("Viewing profit requires the reports.profit permission.");

        var input = await BuildAdvancedInputAsync(range, refresh, ct);
        input.CanSeeProfit = true;
        var data = await _svc.GetAdvancedProfitAsync(input, ct);
        return CommonHelpers.SuccessResponse("Advanced profit loaded.", data);
    }

    #region Private Helpers

    private async Task<bool> HasAdvancedAccessAsync(CancellationToken ct)
    {
        var hasDash = await _access.CanAsync("dashboard.view", ct);
        var hasReports = await _access.CanAsync("reports.view", ct);
        return hasDash && hasReports;
    }

    private static IActionResult Forbidden(string message) =>
        new ObjectResult(CommonResponseModel.Failure(message, statusCode: StatusCodes.Status403Forbidden))
        {
            StatusCode = StatusCodes.Status403Forbidden
        };

    private async Task<InputDashboardEssentials> BuildEssentialsInputAsync(string range, bool refresh, CancellationToken ct)
    {
        return new InputDashboardEssentials
        {
            ActorUserId = AuthClaims.GetUserId(User) ?? 0,
            ActorFkStore = AuthClaims.GetFkStore(User) ?? 0,
            Range = range,
            Refresh = refresh,
            IsPlatformSuperAdmin = AuthClaims.IsPlatformSuperAdminUser(User),
            HasDashboardView = await _access.CanAsync("dashboard.view", ct),
            HasReportsView = await _access.CanAsync("reports.view", ct),
            CanPosSell = await _access.CanAsync("pos.sell", ct),
            CanInventoryView = await _access.CanAsync("inventory.view", ct),
            CanPurchasesManage = await _access.CanAsync("purchases.manage", ct),
            FeatureLowStock = await _access.FeatureEnabledAsync("INVENTORY.LOW_STOCK", ct),
            FeaturePurchases = await _access.FeatureEnabledAsync("PURCHASES.ORDERS", ct),
            FeatureOnlineOrders = await _access.FeatureEnabledAsync("POS.ONLINE_ORDERS", ct),
            FeatureCashSession = await _access.FeatureEnabledAsync("POS.CASH_SESSION", ct),
            FeatureReturns = await _access.FeatureEnabledAsync("POS.RETURNS", ct),
            FeatureItemReport = await _access.FeatureEnabledAsync("REPORTS.ITEM_SALES", ct)
        };
    }

    private async Task<InputDashboardAdvanced> BuildAdvancedInputAsync(string range, bool refresh, CancellationToken ct)
    {
        return new InputDashboardAdvanced
        {
            ActorUserId = AuthClaims.GetUserId(User) ?? 0,
            ActorFkStore = AuthClaims.GetFkStore(User) ?? 0,
            Range = range,
            Refresh = refresh,
            IsPlatformSuperAdmin = AuthClaims.IsPlatformSuperAdminUser(User),
            CanSeeProfit = await _access.CanAsync("reports.profit", ct),
            FeatureOnlineOrders = await _access.FeatureEnabledAsync("POS.ONLINE_ORDERS", ct),
            FeaturePurchases = await _access.FeatureEnabledAsync("PURCHASES.ORDERS", ct),
            FeatureCategoryReport = await _access.FeatureEnabledAsync("REPORTS.CATEGORY_SALES", ct)
        };
    }

    #endregion
}
