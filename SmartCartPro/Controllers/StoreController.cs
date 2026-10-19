using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

[Authorize]
public class StoreController : Controller
{
    private readonly ICompanyService _svc;
    private readonly IAuthService _authSvc;

    public StoreController(ICompanyService svc, IAuthService authSvc)
    {
        _svc = svc;
        _authSvc = authSvc;
    }

    private int ActorId => AuthClaims.GetUserId(User) ?? 0;
    private bool CanCreateStore => AuthClaims.IsPlatformSuperAdminUser(User);

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var model = await _svc.ListStoresAsync(new InputStoreList { CanCreate = CanCreateStore }, ct);
        return View(model);
    }

    [HttpGet]
    [Authorize(Policy = "PlatformSuperAdmin")]
    public IActionResult Create() => View(new InStoreCreateView { IsActive = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PlatformSuperAdmin")]
    public async Task<IActionResult> Create(InStoreCreateView model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (ActorId <= 0)
        {
            return RedirectToAction("Login", "Auth");
        }

        var result = await _svc.CreateStoreAsync(
            InputStoreCreate.From(model, ActorId, canCreate: true), ct);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.ResponseMsg);
            return View(model);
        }

        TempData["SuccessMessage"] = "Store created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var store = await _svc.GetStoreAsync(new InputStoreSelect { IdStore = id }, ct);
        if (store is null)
        {
            return NotFound();
        }

        return View(store.ToUpdateView());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, InStoreUpdateView model, CancellationToken ct)
    {
        model.IdStore = id;
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (ActorId <= 0)
        {
            return RedirectToAction("Login", "Auth");
        }

        var result = await _svc.UpdateStoreAsync(InputStoreUpdate.From(model, ActorId), ct);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.ResponseMsg);
            return View(model);
        }

        TempData["SuccessMessage"] = "Store updated.";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Switches the current user's active store context and re-issues the auth cookie.
    /// Validates that the user has access to the requested store via user_store_access.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetCurrentStore(int storeId, string? returnUrl, CancellationToken ct)
    {
        if (ActorId <= 0)
        {
            return RedirectToAction("Login", "Auth");
        }

        var rememberMe = Request.Cookies.ContainsKey(AuthClaims.TokenCookieName);

        var result = await _authSvc.SwitchStoreAsync(ActorId, storeId, rememberMe, ct);
        if (!result.Success)
        {
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Could not switch store.";
            return RedirectToAction(nameof(Index));
        }

        _authSvc.SetAuthCookie(result.Token!, result.ExpiresAt, rememberMe);

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}
