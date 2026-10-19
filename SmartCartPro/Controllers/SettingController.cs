using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Exceptions;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>
/// Settings controller — manages known company-level configuration keys.
/// Known keys:
///   pos.invoice_prefix    (string)
///   pos.allow_negative_stock  (bool)
///   default_tax_code          (string)
///   locale.currency           (string)
///   locale.timezone           (string)
///
/// ModuleAccess: SETTINGS | Permission: settings.manage
/// </summary>
[Authorize]
[ModuleAccess("SETTINGS")]
[RequirePermission("settings.manage")]
public class SettingController : Controller
{
    private readonly ISettingService _svc;
    private int ActorId => AuthClaims.GetUserId(User) ?? 0;

    public SettingController(ISettingService svc) => _svc = svc;

    private static readonly string[] KnownKeys =
    [
        "pos.invoice_prefix",
        "pos.allow_negative_stock",
        "default_tax_code",
        "locale.currency",
        "locale.timezone"
    ];

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var list = await _svc.ListSettingsAsync(new InputSettingList { Scope = "company" }, ct);
        var map  = list.Items.ToDictionary(s => s.SettingKey, s => s.SettingValue);

        var model = new InSettingView
        {
            InvoicePrefix       = map.GetValueOrDefault("pos.invoice_prefix"),
            AllowNegativeStock  = map.GetValueOrDefault("pos.allow_negative_stock") == "true",
            DefaultTaxCode      = map.GetValueOrDefault("default_tax_code"),
            Currency            = map.GetValueOrDefault("locale.currency"),
            Timezone            = map.GetValueOrDefault("locale.timezone")
        };

        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(InSettingView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var values = new Dictionary<string, string?>
        {
            ["pos.invoice_prefix"]       = model.InvoicePrefix,
            ["pos.allow_negative_stock"] = model.AllowNegativeStock ? "true" : "false",
            ["default_tax_code"]         = model.DefaultTaxCode,
            ["locale.currency"]          = model.Currency,
            ["locale.timezone"]          = model.Timezone
        };

        var result = await _svc.SaveSettingsAsync(new InputSettingSave
        {
            Scope     = "company",
            Values    = values,
            UpdatedBy = ActorId
        }, ct);

        if (!result.Ok)
            throw new UpdateFailedException("Unable to save settings.", result.ResponseMsg ?? "Unable to save settings.");

        return CommonHelpers.SuccessResponse("Settings saved successfully.");
    }
}
