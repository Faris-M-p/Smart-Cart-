using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

[Authorize]
public class CompanyController : Controller
{
    private readonly ICompanyService _svc;

    public CompanyController(ICompanyService svc) => _svc = svc;

    private int ActorId => AuthClaims.GetUserId(User) ?? 0;
    private bool IsPlatformAdmin => AuthClaims.IsPlatformSuperAdminUser(User);

    /// <summary>
    /// Shows the company profile form and a read-only license summary for platform administrators.
    /// Company admins edit their own company; the platform Super Admin creates one when none exists.
    /// </summary>
    [HttpGet]
    [RequirePermission("settings.manage")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var output = await _svc.GetCompanyAsync(new InputCompanySelect { CanCreate = IsPlatformAdmin }, ct);
        ViewBag.IsPlatformAdmin = IsPlatformAdmin;
        ViewBag.License = await _svc.GetLicenseAsync(new InputLicenseSelect(), ct);
        return View(output.ToView());
    }

    /// <summary>
    /// Creates or updates the company legal name, tax and registration numbers, contact and address.
    /// Platform Super Admin bootstraps the first company; company admins save changes to the existing row.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("settings.manage")]
    public async Task<IActionResult> Save(InCompanyUpdateView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        if (model.RequiresBootstrap || model.IdCompany <= 0)
        {
            var input = new InputCompanyCreate
            {
                ActorUserId = ActorId,
                CanCreate = IsPlatformAdmin,
                CompanyName = model.CompanyName,
                CompanyCode = model.CompanyCode,
                LegalName = model.LegalName,
                TaxNumber = model.TaxNumber,
                RegistrationNo = model.RegistrationNo,
                Email = model.Email,
                Phone = model.Phone,
                Website = model.Website,
                AddressLine1 = model.AddressLine1,
                AddressLine2 = model.AddressLine2,
                City = model.City,
                State = model.State,
                PostalCode = model.PostalCode,
                Country = model.Country,
                CurrencyCode = model.CurrencyCode,
                TimeZone = model.TimeZone,
                Locale = model.Locale,
                IsActive = model.IsActive
            };
            var created = await _svc.CreateCompanyAsync(input, ct);
            return CommonHelpers.SuccessResponse(
                "Company profile saved.",
                new { id = created.IdCompany, redirectUrl = Url.Action(nameof(Index)) });
        }

        var updateInput = new InputCompanyUpdate
        {
            ActorUserId = ActorId,
            IdCompany = model.IdCompany,
            CompanyName = model.CompanyName,
            CompanyCode = model.CompanyCode,
            LegalName = model.LegalName,
            TaxNumber = model.TaxNumber,
            RegistrationNo = model.RegistrationNo,
            Email = model.Email,
            Phone = model.Phone,
            Website = model.Website,
            AddressLine1 = model.AddressLine1,
            AddressLine2 = model.AddressLine2,
            City = model.City,
            State = model.State,
            PostalCode = model.PostalCode,
            Country = model.Country,
            CurrencyCode = model.CurrencyCode,
            TimeZone = model.TimeZone,
            Locale = model.Locale,
            IsActive = model.IsActive
        };
        var updated = await _svc.UpdateCompanyAsync(updateInput, ct);
        return CommonHelpers.SuccessResponse(
            "Company profile saved.",
            new { id = updated.IdCompany, redirectUrl = Url.Action(nameof(Index)) });
    }
}
