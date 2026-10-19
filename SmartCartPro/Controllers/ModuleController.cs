using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

[Authorize(Policy = "PlatformSuperAdmin")]
public class ModuleController : Controller
{
    private readonly IModuleService _svc;

    public ModuleController(IModuleService svc) => _svc = svc;

    private int ActorId => AuthClaims.GetUserId(User) ?? 0;

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var data = await _svc.GetModuleListAsync(ct);

        var model = new InModuleAssignView
        {
            AllModules = data.Modules,
            Presets = data.Presets,
            EnabledModuleIds = data.EnabledModuleIds
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(InModuleAssignView model, CancellationToken ct)
    {
        var result = await _svc.AssignModulesAsync(
            InputModuleAssign.From(model, ActorId), ct);

        if (!result.Ok)
        {
            TempData["ErrorMessage"] = result.ResponseMsg;
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = $"Module assignment saved ({result.EnabledCount} modules enabled).";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Apply a preset and immediately save it.</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyPreset(InModuleAssignView model, CancellationToken ct)
    {
        var data = await _svc.GetModuleListAsync(ct);

        var preset = data.Presets.FirstOrDefault(p =>
            string.Equals(p.PresetCode, model.SelectedPresetCode, StringComparison.OrdinalIgnoreCase));

        if (preset is null)
        {
            TempData["ErrorMessage"] = "Preset not found.";
            return RedirectToAction(nameof(Index));
        }

        var input = new InputModuleAssign
        {
            ActorUserId = ActorId,
            EnabledModuleIds = preset.ModuleIds
        };

        var result = await _svc.AssignModulesAsync(input, ct);
        TempData[result.Ok ? "SuccessMessage" : "ErrorMessage"] =
            result.Ok ? $"Preset '{preset.PresetName}' applied ({result.EnabledCount} modules enabled)." : result.ResponseMsg;

        return RedirectToAction(nameof(Index));
    }
}
