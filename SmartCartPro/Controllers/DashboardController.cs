using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Helpers;
using SmartCartPro.Models;

namespace SmartCartPro.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly SmartCartDbContext _db;

    public DashboardController(SmartCartDbContext db) => _db = db;

    private int? FkStore => AuthClaims.GetFkStore(User);

    /// <summary>
    /// Returns a lightweight summary for the dashboard card widgets.
    /// Sales today is 0 until the sales module is fully wired.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Summary(CancellationToken ct)
    {
        var storeId = FkStore ?? 0;

        // Low stock: inventory rows where quantity_on_hand <= reorder_level
        // (reorder_level column falls back to 0 if not present in this phase)
        int lowStockCount = 0;
        if (storeId > 0)
        {
            lowStockCount = await _db.Inventories
                .AsNoTracking()
                .CountAsync(i => i.FkStore == storeId && i.QuantityOnHand <= 0, ct);
        }

        var summary = new OutputDashboardSummary
        {
            SalesToday = 0,              // not wired yet
            LowStockCount = lowStockCount,
            PendingPurchaseOrders = 0,   // not wired yet
            GeneratedAt = DateTime.UtcNow
        };

        return Json(summary);
    }
}
