using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Filters;
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
    /// Returns live store dashboard KPIs, charts and tables as a JSON envelope.
    /// The home page refresh button and 60-second poll call this when the tab is visible.
    /// </summary>
    [HttpGet]
    [ModuleAccess("DASHBOARD")]
    [RequirePermission("dashboard.view")]
    public async Task<IActionResult> Snapshot(CancellationToken ct)
    {
        var input = new InputDashboardSnapshot
        {
            ActorUserId = AuthClaims.GetUserId(User) ?? 0,
            ActorFkStore = AuthClaims.GetFkStore(User) ?? 0,
            IsPlatformSuperAdmin = AuthClaims.IsPlatformSuperAdminUser(User),
            ModulePos = await _access.FeatureEnabledAsync("POS.BILLING", ct),
            FeatureLowStock = await _access.FeatureEnabledAsync("INVENTORY.LOW_STOCK", ct),
            FeatureOnlineOrders = await _access.FeatureEnabledAsync("POS.ONLINE_ORDERS", ct),
            FeatureCashSession = await _access.FeatureEnabledAsync("POS.CASH_SESSION", ct),
            FeaturePurchases = await _access.FeatureEnabledAsync("PURCHASES.ORDERS", ct)
        };
        var data = await _svc.GetSnapshotAsync(input, ct);
        return CommonHelpers.SuccessResponse("Dashboard snapshot.", data);
    }
}
