using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Exceptions;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

[Authorize(Policy = "PlatformSuperAdmin")]
public class LicenseController : Controller
{
    private readonly ICompanyService _svc;

    public LicenseController(ICompanyService svc) => _svc = svc;

    private int ActorId => AuthClaims.GetUserId(User) ?? 0;

    [HttpGet]
    public async Task<IActionResult> Index(string? mode, int? editId, CancellationToken ct)
    {
        var licenses = await _svc.ListLicensesAsync(ct);
        var today = licenses.FirstOrDefault()?.Today ?? await _svc.GetLicenseTodayAsync(ct);
        var stores = licenses.Count > 0
            ? licenses[0].ActiveStoreCount
            : await _svc.GetActiveStoreCountForLicenseAsync(ct);
        var users = licenses.Count > 0
            ? licenses[0].ActiveUserCount
            : await _svc.GetActiveUserCountForLicenseAsync(ct);
        var current = licenses
            .Where(x => !x.Cancelled && x.StartDate <= today && x.ExpiryDate >= today)
            .OrderByDescending(x => x.StartDate)
            .FirstOrDefault();
        var summary = current
            ?? licenses.Where(x => !x.Cancelled && x.StartDate > today).OrderBy(x => x.StartDate).FirstOrDefault()
            ?? licenses.Where(x => !x.Cancelled && x.ExpiryDate < today).OrderByDescending(x => x.ExpiryDate).FirstOrDefault()
            ?? licenses.Where(x => x.Cancelled).OrderByDescending(x => x.StartDate).FirstOrDefault();

        var page = new LicensePageViewModel
        {
            Today = today,
            ActiveStoreCount = stores,
            ActiveUserCount = users,
            Summary = summary,
            Licenses = licenses
        };

        if (editId is > 0)
        {
            var existing = await _svc.GetLicenseByIdAsync(editId.Value, ct)
                ?? throw new ResourceNotFoundException("License not found.", $"License {editId.Value} was not found.");
            if (existing.Cancelled)
                throw new ConflictException("Cancelled licenses cannot be edited.", $"License {existing.IdLicense} is cancelled.");
            page.IsEditMode = true;
            page.Editor = new InLicenseSaveView
            {
                IdLicense = existing.IdLicense,
                StartDate = existing.StartDate,
                ExpiryDate = existing.ExpiryDate,
                MaxStores = existing.MaxStores,
                Notes = existing.Notes
            };
        }
        else if (string.Equals(mode, "create", StringComparison.OrdinalIgnoreCase))
        {
            var latestExpiry = licenses.Where(x => !x.Cancelled).Select(x => (DateOnly?)x.ExpiryDate).Max();
            var startDate = latestExpiry is { } expiry && expiry >= today && expiry < DateOnly.MaxValue
                ? expiry.AddDays(1)
                : today;
            page.IsCreateMode = true;
            page.Editor = new InLicenseSaveView
            {
                StartDate = startDate,
                ExpiryDate = startDate.AddYears(1).AddDays(-1),
                MaxStores = Math.Max(1, stores)
            };
        }

        return View(page);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(InLicenseSaveView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);
        OutputLicenseUpdate result;
        if (model.IdLicense == 0)
        {
            result = await _svc.CreateLicenseAsync(new InputLicenseCreate
            {
                ActorUserId = ActorId,
                CanEditLicense = true,
                StartDate = model.StartDate,
                ExpiryDate = model.ExpiryDate,
                MaxStores = model.MaxStores,
                Notes = model.Notes,
                Reason = model.Reason
            }, ct);
        }
        else
        {
            result = await _svc.UpdateLicenseAsync(new InputLicenseUpdate
            {
                ActorUserId = ActorId,
                CanEditLicense = true,
                IdLicense = model.IdLicense,
                StartDate = model.StartDate,
                ExpiryDate = model.ExpiryDate,
                MaxStores = model.MaxStores,
                Notes = model.Notes,
                Reason = model.Reason
            }, ct);
        }

        return CommonHelpers.SuccessResponse(
            model.IdLicense == 0 ? "License created." : "License updated.",
            new { id = result.IdLicense, redirectUrl = Url.Action(nameof(Index)) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Suspend(InLicenseStatusView model, CancellationToken ct) => ChangeStatus(model, "suspend", ct);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Resume(InLicenseStatusView model, CancellationToken ct) => ChangeStatus(model, "resume", ct);

    // Keep the previous endpoint available for existing links/clients.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Activate(InLicenseStatusView model, CancellationToken ct) => ChangeStatus(model, "resume", ct);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public Task<IActionResult> Cancel(InLicenseStatusView model, CancellationToken ct) => ChangeStatus(model, "cancel", ct);

    private async Task<IActionResult> ChangeStatus(InLicenseStatusView model, string action, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);
        var result = await _svc.SetLicenseStatusAsync(new InputLicenseStatusUpdate
        {
            ActorUserId = ActorId,
            IdLicense = model.IdLicense,
            Action = action,
            Today = await _svc.GetLicenseTodayAsync(ct),
            Reason = model.Reason
        }, ct);
        var message = action switch
        {
            "suspend" => "License suspended.",
            "resume" => "License resumed. Its validity dates are unchanged.",
            _ => "Future license cancelled."
        };
        return CommonHelpers.SuccessResponse(message,
            new { id = result.IdLicense, redirectUrl = Url.Action(nameof(Index)) });
    }
}
