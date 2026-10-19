using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Exceptions;
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
    private bool CanBootstrap => AuthClaims.IsPlatformSuperAdminUser(User);

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var output = await _svc.GetCompanyAsync(new InputCompanySelect { CanCreate = CanBootstrap }, ct);
        return View(output.ToView());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(InCompanyUpdateView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        if (model.RequiresBootstrap || model.IdCompany <= 0)
        {
            var input = new InputCompanyCreate
            {
                ActorUserId = ActorId,
                CanCreate = CanBootstrap,
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
            if (!created.Ok)
                throw new UpdateFailedException("Unable to save company.", created.ResponseMsg ?? "Unable to save company.");

            return CommonHelpers.SuccessResponse("Company profile saved.", new { id = created.IdCompany });
        }
        else
        {
            var input = new InputCompanyUpdate
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
            var updated = await _svc.UpdateCompanyAsync(input, ct);
            if (!updated.Ok)
                throw new UpdateFailedException("Unable to save company.", updated.ResponseMsg ?? "Unable to save company.");

            return CommonHelpers.SuccessResponse("Company profile saved.", new { id = updated.IdCompany });
        }
    }
}
