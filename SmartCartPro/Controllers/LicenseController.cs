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

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var license = await _svc.GetLicenseAsync(new InputLicenseSelect(), ct);
        var view = license is not null
            ? new InLicenseUpdateView
            {
                LicenseKey = license.LicenseKey,
                LicenseType = license.LicenseType,
                Status = license.Status,
                StartDate = license.StartDate,
                ExpiryDate = license.ExpiryDate,
                MaxStores = license.MaxStores,
                Notes = license.Notes
            }
            : new InLicenseUpdateView
            {
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                ExpiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1))
            };

        return View(view);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(InLicenseUpdateView model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(nameof(Index), model);
        }

        var result = await _svc.UpdateLicenseAsync(
            InputLicenseUpdate.From(model, ActorId, canEditLicense: true), ct);

        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.ResponseMsg);
            return View(nameof(Index), model);
        }

        TempData["SuccessMessage"] = "License saved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Suspend(CancellationToken ct)
    {
        var license = await _svc.GetLicenseAsync(new InputLicenseSelect(), ct);
        if (license is null)
        {
            TempData["ErrorMessage"] = "No license found.";
            return RedirectToAction(nameof(Index));
        }

        var input = new InputLicenseUpdate
        {
            ActorUserId = ActorId,
            CanEditLicense = true,
            LicenseKey = license.LicenseKey,
            LicenseType = license.LicenseType,
            Status = "suspended",
            StartDate = license.StartDate,
            ExpiryDate = license.ExpiryDate,
            MaxStores = license.MaxStores,
            Notes = license.Notes
        };

        var result = await _svc.UpdateLicenseAsync(input, ct);
        TempData[result.Ok ? "SuccessMessage" : "ErrorMessage"] =
            result.Ok ? "License suspended." : result.ResponseMsg;

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(CancellationToken ct)
    {
        var license = await _svc.GetLicenseAsync(new InputLicenseSelect(), ct);
        if (license is null)
        {
            TempData["ErrorMessage"] = "No license found.";
            return RedirectToAction(nameof(Index));
        }

        var input = new InputLicenseUpdate
        {
            ActorUserId = ActorId,
            CanEditLicense = true,
            LicenseKey = license.LicenseKey,
            LicenseType = license.LicenseType,
            Status = "active",
            StartDate = license.StartDate,
            ExpiryDate = license.ExpiryDate,
            MaxStores = license.MaxStores,
            Notes = license.Notes
        };

        var result = await _svc.UpdateLicenseAsync(input, ct);
        TempData[result.Ok ? "SuccessMessage" : "ErrorMessage"] =
            result.Ok ? "License activated." : result.ResponseMsg;

        return RedirectToAction(nameof(Index));
    }
}
