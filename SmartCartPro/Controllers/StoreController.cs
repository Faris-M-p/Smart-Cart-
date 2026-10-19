using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Exceptions;
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

    /// <summary>
    /// Lists stores for the current company with create affordances when allowed.
    /// Used by authenticated users managing multi-store operations.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var model = await _svc.ListStoresAsync(new InputStoreList { CanCreate = CanCreateStore }, ct);
        return View(model);
    }

    /// <summary>
    /// Renders the new-store creation form.
    /// Platform super admins only.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "PlatformSuperAdmin")]
    public IActionResult Create() => View(new InStoreCreateView { IsActive = true });

    /// <summary>
    /// Persists a new store from submitted details.
    /// Platform super admins only.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PlatformSuperAdmin")]
    public async Task<IActionResult> Create(InStoreCreateView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputStoreCreate
        {
            ActorUserId = ActorId,
            CanCreate = true,
            StoreCode = model.StoreCode,
            StoreName = model.StoreName,
            Email = model.Email,
            Phone = model.Phone,
            AddressLine1 = model.AddressLine1,
            AddressLine2 = model.AddressLine2,
            City = model.City,
            State = model.State,
            PostalCode = model.PostalCode,
            Country = model.Country,
            IsDefault = model.IsDefault,
            IsActive = model.IsActive
        };
        var result = await _svc.CreateStoreAsync(input, ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to create store.", result.ResponseMsg ?? "Unable to create store.");

        return CommonHelpers.SuccessResponse("Store created.", new { id = result.IdStore });
    }

    /// <summary>
    /// Loads a store into the edit form by identifier.
    /// Used by authenticated users with store access.
    /// </summary>
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

    /// <summary>
    /// Updates store details from the edit form.
    /// Used by authenticated users with store access.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, InStoreUpdateView model, CancellationToken ct)
    {
        model.IdStore = id;
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputStoreUpdate
        {
            ActorUserId = ActorId,
            IdStore = model.IdStore,
            StoreCode = model.StoreCode ?? string.Empty,
            StoreName = model.StoreName ?? string.Empty,
            Email = model.Email,
            Phone = model.Phone,
            AddressLine1 = model.AddressLine1,
            AddressLine2 = model.AddressLine2,
            City = model.City,
            State = model.State,
            PostalCode = model.PostalCode,
            Country = model.Country,
            IsDefault = model.IsDefault,
            IsActive = model.IsActive
        };
        var result = await _svc.UpdateStoreAsync(input, ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to update store.", result.ResponseMsg ?? "Unable to update store.");

        return CommonHelpers.SuccessResponse("Store updated.", new { id = result.IdStore });
    }

    /// <summary>
    /// Switches the current user's active store context and re-issues the auth cookie.
    /// Validates that the user has access to the requested store via user_store_access.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetCurrentStore(int storeId, string? returnUrl, CancellationToken ct)
    {
        var rememberMe = Request.Cookies.ContainsKey(AuthClaims.TokenCookieName);

        var result = await _authSvc.SwitchStoreAsync(ActorId, storeId, rememberMe, ct);
        if (!result.Success)
            throw new UpdateFailedException(
                "Could not switch store.",
                result.ErrorMessage ?? "Could not switch store.");

        _authSvc.SetAuthCookie(result.Token!, result.ExpiresAt, rememberMe);

        var redirectUrl = !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : Url.Action("Index", "Home")!;

        return CommonHelpers.SuccessResponse(
            "Store switched successfully.",
            new { storeId, storeName = result.StoreName, redirectUrl });
    }
}
