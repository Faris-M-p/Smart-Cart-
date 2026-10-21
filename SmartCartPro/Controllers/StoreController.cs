using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Exceptions;
using SmartCartPro.Filters;
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
    /// Lists stores for the installation with a Default badge and license usage (x of y).
    /// Company admins with settings.manage open this from Administration; Super Admin can create stores.
    /// </summary>
    [HttpGet]
    [RequirePermission("settings.manage")]
    public async Task<IActionResult> Index([FromQuery] InputStoreList input, CancellationToken ct)
    {
        input.CanCreate = CanCreateStore;
        var model = await _svc.ListStoresAsync(input, ct);
        return View(model);
    }

    /// <summary>
    /// Renders the new-store form (code, name, contact, address, default and active flags).
    /// Platform Super Admin only; MaxStores from the license is enforced on save.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "PlatformSuperAdmin")]
    public IActionResult Create(string? returnUrl)
    {
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        return View(new InStoreCreateView { IsActive = true });
    }

    /// <summary>
    /// Persists a new store. The first store is always the default; later stores honour MaxStores.
    /// Platform Super Admin only after reviewing the create form.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "PlatformSuperAdmin")]
    public async Task<IActionResult> Create(InStoreCreateView model, string? returnUrl, CancellationToken ct)
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
        var previousStore = AuthClaims.GetFkStore(User) ?? 0;
        var result = await _svc.CreateStoreAsync(input, ct);

        var rememberMe = AuthClaims.WantsPersistentSession(User);
        var switched = await _authSvc.SwitchStoreAsync(ActorId, result.IdStore, rememberMe, ct);
        if (switched.Success)
        {
            _authSvc.SetAuthCookie(switched.Token!, switched.ExpiresAt, rememberMe);
        }

        var redirectUrl = previousStore <= 0
            ? Url.Action("Index", "Home")!
            : SafeReturnUrl(returnUrl) ?? Url.Action(nameof(Index))!;
        return CommonHelpers.SuccessResponse(
            "Store created.",
            new { id = result.IdStore, redirectUrl });
    }

    /// <summary>
    /// Loads one store into the edit form for code, name, address and default flag.
    /// Company admins with settings.manage open it from the store list.
    /// </summary>
    [HttpGet]
    [RequirePermission("settings.manage")]
    public async Task<IActionResult> Edit(int id, string? returnUrl, CancellationToken ct)
    {
        var store = await _svc.GetStoreAsync(new InputStoreSelect { IdStore = id }, ct);
        if (store is null)
        {
            return NotFound();
        }

        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        return View(store.ToUpdateView());
    }

    /// <summary>
    /// Updates store details. Deactivating or un-flagging the current default without promoting another is rejected.
    /// Company admins with settings.manage save from the edit form.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("settings.manage")]
    public async Task<IActionResult> Edit(int id, InStoreUpdateView model, string? returnUrl, CancellationToken ct)
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
        var listUrl = SafeReturnUrl(returnUrl) ?? Url.Action(nameof(Index))!;
        return CommonHelpers.SuccessResponse(
            "Store updated.",
            new { id = result.IdStore, redirectUrl = listUrl });
    }

    /// <summary>
    /// Activates or deactivates a store from the list. The current default cannot be deactivated.
    /// Company admins confirm the change on the store list; there is no hard delete.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("settings.manage")]
    public async Task<IActionResult> SetActive(int id, bool isActive, CancellationToken ct)
    {
        var input = new InputStoreStatusUpdate
        {
            ActorUserId = ActorId,
            IdStore = id,
            IsActive = isActive
        };
        var result = await _svc.SetStoreActiveAsync(input, ct);
        return CommonHelpers.SuccessResponse(
            result.IsActive ? "Store activated." : "Store deactivated.",
            new { id = result.IdStore, isActive = result.IsActive });
    }

    /// <summary>
    /// Switches the signed-in user's current store and re-issues the JWT cookie.
    /// Super Admin may enter any active store; other users need user_store_access.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetCurrentStore(int storeId, CancellationToken ct)
    {
        var isSuperAdmin = AuthClaims.IsPlatformSuperAdminUser(User);
        var accessInput = new InputStoreAccessibleList
        {
            IdUser = ActorId,
            AllActiveStores = isSuperAdmin
        };
        var accessible = await _svc.ListAccessibleStoresAsync(accessInput, ct);
        if (!accessible.Stores.Any(s => s.IdStore == storeId))
        {
            throw new ForbiddenException(
                "You do not have access to that store.",
                isSuperAdmin
                    ? $"Super Admin {ActorId} requested inactive or missing store {storeId}."
                    : $"User {ActorId} has no user_store_access row for store {storeId}.");
        }

        var rememberMe = AuthClaims.WantsPersistentSession(User);
        var result = await _authSvc.SwitchStoreAsync(ActorId, storeId, rememberMe, ct);
        if (!result.Success)
        {
            throw new ForbiddenException(
                result.ErrorMessage ?? "You do not have access to that store.",
                result.ErrorMessage ?? $"SwitchStoreAsync failed for user {ActorId}, store {storeId}.");
        }

        _authSvc.SetAuthCookie(result.Token!, result.ExpiresAt, rememberMe);
        return CommonHelpers.SuccessResponse(
            "Store switched.",
            new { storeId, storeName = result.StoreName });
    }

    private string? SafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl)) return null;
        if (!Url.IsLocalUrl(returnUrl)) return null;
        return returnUrl;
    }
}
