using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Exceptions;
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

    /// <summary>
    /// Shows module toggles, descriptions and Basic / Premium / Custom presets for the installation.
    /// Platform Super Admin configures which licensed features appear in the admin menu.
    /// </summary>
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

    /// <summary>
    /// Saves the selected enabled modules. Core modules from the Basic preset stay on.
    /// Platform Super Admin submits the toggles after reviewing descriptions.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(InModuleAssignView model, CancellationToken ct)
    {
        var input = new InputModuleAssign
        {
            ActorUserId = ActorId,
            EnabledModuleIds = model.EnabledModuleIds ?? new List<int>()
        };
        var result = await _svc.AssignModulesAsync(input, ct);
        return CommonHelpers.SuccessResponse(
            $"Module assignment saved ({result.EnabledCount} modules enabled).",
            new { enabledCount = result.EnabledCount, redirectUrl = Url.Action(nameof(Index)) });
    }

    /// <summary>
    /// Applies a Basic or Premium preset from seed data and saves it immediately.
    /// Platform Super Admin uses the preset buttons on the modules page.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyPreset(InModuleAssignView model, CancellationToken ct)
    {
        var data = await _svc.GetModuleListAsync(ct);
        var preset = data.Presets.FirstOrDefault(p =>
            string.Equals(p.PresetCode, model.SelectedPresetCode, StringComparison.OrdinalIgnoreCase));

        if (preset is null)
        {
            throw new ResourceNotFoundException(
                "Preset not found.",
                $"PresetCode={model.SelectedPresetCode}");
        }

        var input = new InputModuleAssign
        {
            ActorUserId = ActorId,
            EnabledModuleIds = preset.ModuleIds
        };
        var result = await _svc.AssignModulesAsync(input, ct);
        return CommonHelpers.SuccessResponse(
            $"Preset '{preset.PresetName}' applied ({result.EnabledCount} modules enabled).",
            new { enabledCount = result.EnabledCount, presetCode = preset.PresetCode, redirectUrl = Url.Action(nameof(Index)) });
    }
}
