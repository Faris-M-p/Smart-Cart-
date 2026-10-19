using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Exceptions;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>
/// Company and store settings for the known keys (views in ~/Views/Setting/).
/// Company scope holds the defaults; store scope overrides the keys that allow it, for the signed-in store only.
/// ModuleAccess: SETTINGS | Permission: settings.manage
/// </summary>
[Authorize]
[ModuleAccess("SETTINGS")]
[RequirePermission("settings.manage")]
public class SettingController : Controller
{
    private readonly ISettingService _svc;

    public SettingController(ISettingService svc) => _svc = svc;

    private int ActorId => AuthClaims.GetUserId(User) ?? 0;

    /// <summary>
    /// Shows the known settings grouped by purpose with their saved, company and effective values.
    /// Administrators open it from the menu; the store tab applies to the store they are signed in to.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(string? scope, CancellationToken ct)
    {
        var fkStore = AuthClaims.GetFkStore(User) ?? 0;
        var hasStore = fkStore > 0;
        var wantsStore = string.Equals(scope, "store", StringComparison.OrdinalIgnoreCase);

        ViewBag.HasStore = hasStore;
        ViewBag.StoreName = AuthClaims.GetStoreName(User);

        if (wantsStore && !hasStore)
        {
            // Platform super admin has no store: the view explains it instead of erroring.
            return View(new OutputSettingSelect { Scope = "store" });
        }

        var selectInput = new InputSettingSelect
        {
            Scope = wantsStore ? "store" : "company",
            FkStore = wantsStore ? fkStore : null
        };
        var model = await _svc.GetSettingsAsync(selectInput, ct);
        return View(model);
    }

    /// <summary>
    /// Saves the posted values of the known keys for the company or the signed-in store.
    /// Administrators submit the settings form; unknown keys and invalid values are rejected with field errors.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(InSettingUpsertView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var isStore = string.Equals(model.Scope, "store", StringComparison.OrdinalIgnoreCase);
        var fkStore = AuthClaims.GetFkStore(User) ?? 0;
        if (isStore && fkStore <= 0)
        {
            throw new ValidationException("Sign in as a store user to edit store settings.");
        }

        var input = new InputSettingUpsert
        {
            ActorUserId = ActorId,
            Scope = isStore ? "store" : "company",
            FkStore = isStore ? fkStore : null,
            Values = model.Values
        };
        var result = await _svc.UpsertSettingsAsync(input, ct);

        var redirectUrl = Url.Action(nameof(Index), new { scope = result.Scope })!;
        var message = result.ChangedKeys.Count == 0
            ? "No changes to save."
            : "Settings saved.";
        return CommonHelpers.SuccessResponse(
            message,
            new { changed = result.ChangedKeys.Count, redirectUrl });
    }
}
