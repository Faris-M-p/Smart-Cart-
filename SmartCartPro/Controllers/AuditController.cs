using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Filters;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>
/// Audit log viewer — read-only, paginated.
/// Access: PlatformSuperAdmin (bypasses all permission checks) OR settings.manage permission.
/// The RequirePermission filter already short-circuits for PlatformSuperAdmin users,
/// so a single [RequirePermission("settings.manage")] covers both cases.
/// </summary>
[Authorize]
public class AuditController : Controller
{
    private readonly IAuditService _svc;

    public AuditController(IAuditService svc) => _svc = svc;

    [HttpGet]
    [RequirePermission("settings.manage")]
    public async Task<IActionResult> Index([FromQuery] InputAuditLogList input, CancellationToken ct)
    {
        input.Page     = Math.Max(1, input.Page);
        input.PageSize = input.PageSize is <= 0 or > 200 ? 50 : input.PageSize;

        var result = await _svc.ListLogsAsync(input, ct);
        ViewBag.Filter = input;
        return View(result);
    }

    /// <summary>JSON export for the audit grid.</summary>
    [HttpGet]
    [RequirePermission("settings.manage")]
    public async Task<IActionResult> ListJson([FromQuery] InputAuditLogList input, CancellationToken ct)
    {
        input.Page     = Math.Max(1, input.Page);
        input.PageSize = input.PageSize is <= 0 or > 200 ? 50 : input.PageSize;
        return Json(await _svc.ListLogsAsync(input, ct));
    }
}
