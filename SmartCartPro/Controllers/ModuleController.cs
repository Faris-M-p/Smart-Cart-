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
    /// Shows the module and feature tree for this installation.
    /// Platform Super Admin configures which screens the company can open.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var tree = await _svc.GetFeatureTreeAsync(ct);
        var model = new InModuleFeatureTreeView
        {
            Modules = tree.Modules,
            Presets = tree.Presets,
            EnabledFeatureCodes = tree.EnabledFeatureCodes
        };
        return View(model);
    }

    /// <summary>
    /// Saves the enabled leaf features and rebuilds installation_modules from them.
    /// Platform Super Admin submits the Modules page after reviewing the tree.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([FromBody] InModuleFeatureSaveView model, CancellationToken ct)
    {
        var input = new InputModuleFeatureAssign
        {
            ActorUserId = ActorId,
            EnabledFeatureCodes = model?.EnabledFeatureCodes ?? new List<string>()
        };
        var result = await _svc.AssignFeaturesAsync(input, ct);
        return CommonHelpers.SuccessResponse(
            $"Feature assignment saved ({result.EnabledCount} features enabled).",
            result);
    }

    /// <summary>
    /// Applies a Basic or Premium preset and saves it immediately.
    /// Kept for callers that post a preset code. The Modules page fills the tree first and saves separately.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyPreset(InModuleAssignView model, CancellationToken ct)
    {
        var tree = await _svc.GetFeatureTreeAsync(ct);
        var preset = tree.Presets.FirstOrDefault(p =>
            string.Equals(p.PresetCode, model.SelectedPresetCode, StringComparison.OrdinalIgnoreCase));

        if (preset is null)
        {
            throw new ResourceNotFoundException(
                "Preset not found.",
                $"PresetCode={model.SelectedPresetCode}");
        }

        var input = new InputModuleFeatureAssign
        {
            ActorUserId = ActorId,
            EnabledFeatureCodes = preset.FeatureCodes
        };
        var result = await _svc.AssignFeaturesAsync(input, ct);
        return CommonHelpers.SuccessResponse(
            $"Preset '{preset.PresetName}' applied ({result.EnabledCount} features enabled).",
            result);
    }
}
