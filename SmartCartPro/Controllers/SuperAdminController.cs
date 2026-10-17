using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>
/// Platform Super Admin section. Every action requires the PlatformSuperAdmin policy
/// which checks the is_platform_super_admin = "1" JWT claim server-side.
/// Company Admin users cannot access these routes.
/// </summary>
[Authorize(Policy = "PlatformSuperAdmin")]
public class SuperAdminController : Controller
{
    private readonly ISuperAdminService _svc;
    private readonly ILogger<SuperAdminController> _logger;

    public SuperAdminController(ISuperAdminService svc, ILogger<SuperAdminController> logger)
    {
        _svc    = svc;
        _logger = logger;
    }

    private int ActorId => AuthClaims.GetUserId(User) ?? 0;

    // ── Dashboard ─────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Dashboard(CancellationToken ct)
    {
        var vm = await _svc.GetDashboardAsync(ct);
        return View(vm);
    }

    // ── Wizard — Step 1: Company Profile ─────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Step1(CancellationToken ct)
    {
        var model = await _svc.GetStep1ModelAsync(ct);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Step1(WizardStep1Model model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        var (ok, error) = await _svc.SaveStep1Async(model, ct);
        if (!ok) { ModelState.AddModelError(string.Empty, error!); return View(model); }
        TempData["WizardSuccess"] = "Company profile saved.";
        return RedirectToAction(nameof(Step2));
    }

    // ── Wizard — Step 2: Regional ─────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Step2(CancellationToken ct)
    {
        var model = await _svc.GetStep2ModelAsync(ct);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Step2(WizardStep2Model model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        var (ok, error) = await _svc.SaveStep2Async(model, ct);
        if (!ok) { ModelState.AddModelError(string.Empty, error!); return View(model); }
        TempData["WizardSuccess"] = "Regional settings saved.";
        return RedirectToAction(nameof(Step3));
    }

    // ── Wizard — Step 3: Modules ──────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Step3(CancellationToken ct)
    {
        var model = await _svc.GetStep3ModelAsync(ct);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Step3(WizardStep3Model model, CancellationToken ct)
    {
        // Re-load catalog for display (not posted)
        var fresh = await _svc.GetStep3ModelAsync(ct);
        model.AllModules = fresh.AllModules;
        model.Presets    = fresh.Presets;

        var (ok, error) = await _svc.SaveStep3Async(model, ct);
        if (!ok) { ModelState.AddModelError(string.Empty, error!); return View(model); }
        TempData["WizardSuccess"] = "Module selection saved.";
        return RedirectToAction(nameof(Step4));
    }

    // ── Wizard — Step 4: Theme ────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Step4(CancellationToken ct)
    {
        var model = await _svc.GetStep4ModelAsync(ct);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Step4(WizardStep4Model model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        var (ok, error) = await _svc.SaveStep4Async(model, ct);
        if (!ok) { ModelState.AddModelError(string.Empty, error!); return View(model); }
        TempData["WizardSuccess"] = "Theme saved.";
        return RedirectToAction(nameof(Step5));
    }

    // ── Wizard — Step 5: License ──────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Step5(CancellationToken ct)
    {
        var model = await _svc.GetStep5ModelAsync(ct);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Step5(WizardStep5Model model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        var (ok, error) = await _svc.SaveStep5Async(model, ct);
        if (!ok) { ModelState.AddModelError(string.Empty, error!); return View(model); }
        TempData["WizardSuccess"] = "License configuration saved.";
        return RedirectToAction(nameof(Step6));
    }

    // ── Wizard — Step 6: Store ────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Step6(CancellationToken ct)
    {
        var model = await _svc.GetStep6ModelAsync(ct);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Step6(WizardStep6Model model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        var (ok, error) = await _svc.SaveStep6Async(model, ct);
        if (!ok) { ModelState.AddModelError(string.Empty, error!); return View(model); }
        TempData["WizardSuccess"] = "Store created.";
        return RedirectToAction(nameof(Step7));
    }

    // ── Wizard — Step 7: Admin ────────────────────────────────────────────────

    [HttpGet]
    public IActionResult Step7() => View(new WizardStep7Model());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Step7(WizardStep7Model model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        var (ok, error) = await _svc.SaveStep7Async(ActorId, model, ct);

        if (!ok)
        {
            ModelState.AddModelError(string.Empty, error!);
            return View(model);
        }

        // Partial success: invitation created but email may have failed
        if (error is not null)
            TempData["WizardWarning"] = error;
        else
            TempData["WizardSuccess"] = "Company Admin invitation created and sent.";

        return RedirectToAction(nameof(Review));
    }

    // ── Wizard — Step 8: Review & Activate ───────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Review(CancellationToken ct)
    {
        var model = await _svc.GetReviewModelAsync(ct);
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(CancellationToken ct)
    {
        var (ok, error) = await _svc.ActivateAsync(ActorId, ct);
        if (!ok)
        {
            TempData["WizardError"] = error;
            return RedirectToAction(nameof(Review));
        }
        TempData["WizardSuccess"] = "Installation activated successfully!";
        return RedirectToAction(nameof(Dashboard));
    }

    // ── License Management ────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> License(CancellationToken ct)
    {
        var vm = await _svc.GetLicenseViewModelAsync(ct);
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> EditLicense(CancellationToken ct)
    {
        var vm = await _svc.GetLicenseViewModelAsync(ct);
        if (vm is null) return RedirectToAction(nameof(Step5));

        var input = new EditLicenseInput
        {
            LicenseKey  = vm.LicenseKey,
            LicenseType = vm.LicenseType,
            Status      = vm.Status,
            StartDate   = vm.StartDate,
            ExpiryDate  = vm.ExpiryDate,
            MaxStores   = vm.MaxStores,
            Notes       = vm.Notes,
        };
        return View(input);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditLicense(EditLicenseInput model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(model);
        var (ok, error) = await _svc.UpdateLicenseAsync(ActorId, model, ct);
        if (!ok) { ModelState.AddModelError(string.Empty, error!); return View(model); }
        TempData["SuccessMessage"] = "License updated.";
        return RedirectToAction(nameof(License));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SuspendLicense(CancellationToken ct)
    {
        var (ok, error) = await _svc.SuspendLicenseAsync(ActorId, ct);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "License suspended." : error;
        return RedirectToAction(nameof(License));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ReactivateLicense(CancellationToken ct)
    {
        var (ok, error) = await _svc.ReactivateLicenseAsync(ActorId, ct);
        TempData[ok ? "SuccessMessage" : "ErrorMessage"] = ok ? "License reactivated." : error;
        return RedirectToAction(nameof(License));
    }

    // ── Audit Log ─────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> AuditLog(AuditLogFilterInput filter, CancellationToken ct)
    {
        var vm = await _svc.GetAuditLogsAsync(filter, ct);
        return View(vm);
    }
}
