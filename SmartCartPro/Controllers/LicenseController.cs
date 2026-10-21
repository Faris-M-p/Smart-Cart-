using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    /// <summary>
    /// Displays the platform license plan, status, dates, remaining days and usage against MaxStores.
    /// Used by the Platform Super Admin to view and manage the installation license.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var license = await _svc.GetLicenseAsync(new InputLicenseSelect(), ct);
        InLicenseUpdateView view;
        if (license is not null)
        {
            view = new InLicenseUpdateView
            {
                LicenseKey = license.LicenseKey,
                LicenseType = license.LicenseType,
                Status = license.Status,
                StartDate = license.StartDate,
                ExpiryDate = license.ExpiryDate,
                MaxStores = license.MaxStores,
                Notes = license.Notes,
                ActiveStoreCount = license.ActiveStoreCount,
                ActiveUserCount = license.ActiveUserCount,
                DaysRemaining = license.DaysRemaining,
                IsExpired = license.IsExpired,
                HasLicense = true
            };
        }
        else
        {
            var stores = await _svc.ListStoresAsync(new InputStoreList(), ct);
            view = new InLicenseUpdateView
            {
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1)),
                ActiveStoreCount = stores.CurrentStoreCount,
                HasLicense = false
            };
        }

        return View(view);
    }

    /// <summary>
    /// Creates or updates the license key, plan, dates, store limit and notes.
    /// Expiry must be on or after start; MaxStores cannot fall below current active store usage.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(InLicenseUpdateView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputLicenseUpdate
        {
            ActorUserId = ActorId,
            CanEditLicense = true,
            LicenseKey = model.LicenseKey,
            LicenseType = model.LicenseType,
            Status = model.Status,
            StartDate = model.StartDate,
            ExpiryDate = model.ExpiryDate,
            MaxStores = model.MaxStores,
            Notes = model.Notes
        };
        var result = await _svc.UpdateLicenseAsync(input, ct);
        return CommonHelpers.SuccessResponse(
            "License saved.",
            new { id = result.IdLicense, redirectUrl = Url.Action(nameof(Index)) });
    }

    /// <summary>
    /// Suspends the license so LicenseGateFilter returns 403 for every non–super-admin request.
    /// Platform Super Admin confirms with a written reason from the license page.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Suspend(InLicenseStatusView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputLicenseStatusUpdate
        {
            ActorUserId = ActorId,
            Activate = false,
            Reason = model.Reason
        };
        var result = await _svc.SetLicenseStatusAsync(input, ct);
        return CommonHelpers.SuccessResponse(
            "License suspended.",
            new { id = result.IdLicense, redirectUrl = Url.Action(nameof(Index)) });
    }

    /// <summary>
    /// Reactivates a suspended license. Expired dates still leave writes blocked until the expiry is extended.
    /// Platform Super Admin confirms with a written reason from the license page.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(InLicenseStatusView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputLicenseStatusUpdate
        {
            ActorUserId = ActorId,
            Activate = true,
            Reason = model.Reason
        };
        var result = await _svc.SetLicenseStatusAsync(input, ct);
        return CommonHelpers.SuccessResponse(
            "License activated.",
            new { id = result.IdLicense, redirectUrl = Url.Action(nameof(Index)) });
    }
}
