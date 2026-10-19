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
        var input = new InputModuleAssign
        {
            ActorUserId = ActorId,
            EnabledModuleIds = model.EnabledModuleIds ?? new List<int>()
        };
        var result = await _svc.AssignModulesAsync(input, ct);

        if (!result.Ok)
            throw new UpdateFailedException("Unable to save module assignment.", result.ResponseMsg ?? "Unable to save module assignment.");

        return CommonHelpers.SuccessResponse(
            $"Module assignment saved ({result.EnabledCount} modules enabled).",
            new { enabledCount = result.EnabledCount });
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
            throw new ResourceNotFoundException("Preset not found.");

        var input = new InputModuleAssign
        {
            ActorUserId = ActorId,
            EnabledModuleIds = preset.ModuleIds
        };

        var result = await _svc.AssignModulesAsync(input, ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to apply preset.", result.ResponseMsg ?? "Unable to apply preset.");

        return CommonHelpers.SuccessResponse(
            $"Preset '{preset.PresetName}' applied ({result.EnabledCount} modules enabled).",
            new { enabledCount = result.EnabledCount, presetCode = preset.PresetCode });
    }
}
