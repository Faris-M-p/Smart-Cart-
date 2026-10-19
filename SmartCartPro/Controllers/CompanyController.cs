using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        if (!ModelState.IsValid)
        {
            return View(nameof(Index), model);
        }

        if (ActorId <= 0)
        {
            return RedirectToAction("Login", "Auth");
        }

        if (model.RequiresBootstrap || model.IdCompany <= 0)
        {
            var created = await _svc.CreateCompanyAsync(
                InputCompanyCreate.From(model, ActorId, CanBootstrap), ct);
            if (!created.Ok)
            {
                ModelState.AddModelError(string.Empty, created.ResponseMsg);
                return View(nameof(Index), model);
            }
        }
        else
        {
            var updated = await _svc.UpdateCompanyAsync(
                InputCompanyUpdate.From(model, ActorId), ct);
            if (!updated.Ok)
            {
                ModelState.AddModelError(string.Empty, updated.ResponseMsg);
                return View(nameof(Index), model);
            }
        }

        TempData["SuccessMessage"] = "Company profile saved.";
        return RedirectToAction(nameof(Index));
    }
}
