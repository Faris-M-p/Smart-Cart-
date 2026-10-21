using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Exceptions;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>
/// Audit log viewer — read-only, company-scoped, paginated.
/// Access: Platform Super Admin or settings.manage. Write API stays on IAuditService.WriteLogAsync.
/// </summary>
[Authorize]
[ModuleAccess("SETTINGS")]
public class AuditController : Controller
{
    private readonly IAuditService _svc;

    public AuditController(IAuditService svc) => _svc = svc;

    /// <summary>
    /// Shows paginated audit logs for this company, defaulting to the last seven days.
    /// Administrators with settings.manage open this from Administration to review who changed what.
    /// </summary>
    [HttpGet]
    [RequirePermission("settings.manage")]
    public async Task<IActionResult> Index([FromQuery] InputAuditLogList input, CancellationToken ct)
    {
        ApplyScopeAndDefaultWindow(input);
        return View(await _svc.ListLogsAsync(input, ct));
    }

    /// <summary>
    /// JSON list of audit logs for export or AJAX refresh of the grid.
    /// Administrators with settings.manage call this from the audit screen.
    /// </summary>
    [HttpGet]
    [RequirePermission("settings.manage")]
    public async Task<IActionResult> ListJson([FromQuery] InputAuditLogList input, CancellationToken ct)
    {
        ApplyScopeAndDefaultWindow(input);
        var result = await _svc.ListLogsAsync(input, ct);
        return CommonHelpers.SuccessResponse("Audit logs loaded.", result);
    }

    /// <summary>
    /// Returns one audit row including the details payload for the JSON viewer modal.
    /// Administrators open a row from the audit list to inspect the recorded details.
    /// </summary>
    [HttpGet]
    [RequirePermission("settings.manage")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var select = new InputAuditLogSelect
        {
            IdAuditLog = id,
            FkCompany = CompanyIdOrNull()
        };
        var item = await _svc.GetLogAsync(select, ct);
        if (item is null)
            throw new ResourceNotFoundException("Audit entry not found.", $"Audit log {id} was not found in this company scope.");

        return CommonHelpers.SuccessResponse("Audit entry loaded.", item);
    }

    private void ApplyScopeAndDefaultWindow(InputAuditLogList input)
    {
        input.FkCompany = CompanyIdOrNull();

        var query = Request.Query;
        if (!query.ContainsKey("From") && !query.ContainsKey("To")
            && input.From is null && input.To is null)
        {
            input.From = DateTime.UtcNow.Date.AddDays(-6);
            input.To = DateTime.UtcNow.Date;
        }
    }

    private int? CompanyIdOrNull()
    {
        var companyId = AuthClaims.GetFkCompany(User);
        return companyId is > 0 ? companyId : null;
    }
}
