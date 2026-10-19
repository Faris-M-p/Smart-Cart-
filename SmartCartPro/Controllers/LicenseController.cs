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

        if (!result.Ok)
            throw new UpdateFailedException("Unable to save license.", result.ResponseMsg ?? "Unable to save license.");

        return CommonHelpers.SuccessResponse("License saved.", new { id = result.IdLicense });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Suspend(CancellationToken ct)
    {
        var license = await _svc.GetLicenseAsync(new InputLicenseSelect(), ct);
        if (license is null)
            throw new ResourceNotFoundException("No license found.");

        var input = new InputLicenseUpdate
        {
            ActorUserId = ActorId,
            CanEditLicense = true,
            LicenseKey = license.LicenseKey,
            LicenseType = license.LicenseType,
            Status = (int)CommonModel.LicenseStatus.Suspended,
            StartDate = license.StartDate,
            ExpiryDate = license.ExpiryDate,
            MaxStores = license.MaxStores,
            Notes = license.Notes
        };

        var result = await _svc.UpdateLicenseAsync(input, ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to suspend license.", result.ResponseMsg ?? "Unable to suspend license.");

        return CommonHelpers.SuccessResponse("License suspended.", new { id = result.IdLicense });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(CancellationToken ct)
    {
        var license = await _svc.GetLicenseAsync(new InputLicenseSelect(), ct);
        if (license is null)
            throw new ResourceNotFoundException("No license found.");

        var input = new InputLicenseUpdate
        {
            ActorUserId = ActorId,
            CanEditLicense = true,
            LicenseKey = license.LicenseKey,
            LicenseType = license.LicenseType,
            Status = (int)CommonModel.LicenseStatus.Active,
            StartDate = license.StartDate,
            ExpiryDate = license.ExpiryDate,
            MaxStores = license.MaxStores,
            Notes = license.Notes
        };

        var result = await _svc.UpdateLicenseAsync(input, ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to activate license.", result.ResponseMsg ?? "Unable to activate license.");

        return CommonHelpers.SuccessResponse("License activated.", new { id = result.IdLicense });
    }
}
