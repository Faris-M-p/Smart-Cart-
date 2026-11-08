using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>
/// Supplier master, ledger statement, payments, and refunds.
/// </summary>
[Authorize]
[ModuleAccess("PURCHASES")]
public class SupplierController : Controller
{
    private readonly ISupplierService _svc;
    private readonly IPurchaseService _purchaseSvc;

    private int ActorId => AuthClaims.GetUserId(User) ?? 0;
    private int StoreId => AuthClaims.GetFkStore(User) ?? 0;

    public SupplierController(ISupplierService svc, IPurchaseService purchaseSvc)
    {
        _svc = svc;
        _purchaseSvc = purchaseSvc;
    }

    #region Supplier Management

    [HttpGet]
    [RequirePermission("purchases.view")]
    public async Task<IActionResult> Index([FromQuery] InputSupplierList input, CancellationToken ct)
        => View(await _svc.ListSuppliersAsync(input, ct));

    [HttpGet]
    [RequirePermission("purchases.manage")]
    public IActionResult Create(string? returnUrl)
    {
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        return View(new InSupplierCreateView { IsActive = true });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("purchases.manage")]
    public async Task<IActionResult> Create(InSupplierCreateView model, string? returnUrl, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputSupplierCreate
        {
            SupplierCode = model.SupplierCode,
            SupplierName = model.SupplierName,
            ContactPerson = model.ContactPerson,
            Email = model.Email,
            Phone = model.Phone,
            AddressLine1 = model.AddressLine1,
            City = model.City,
            State = model.State,
            PostalCode = model.PostalCode,
            Country = model.Country,
            TaxNumber = model.TaxNumber,
            IsActive = model.IsActive
        };
        var result = await _svc.CreateSupplierAsync(input, ct);

        var listUrl = SafeReturnUrl(returnUrl) ?? Url.Action(nameof(Index))!;
        return CommonHelpers.SuccessResponse(
            "Supplier created successfully.",
            new { id = result.IdSupplier, redirectUrl = listUrl });
    }

    [HttpGet]
    [RequirePermission("purchases.manage")]
    public async Task<IActionResult> Edit(int id, string? returnUrl, CancellationToken ct)
    {
        var select = new InputSupplierSelect { IdSupplier = id };
        var item = await _svc.GetSupplierAsync(select, ct);
        if (item is null) return NotFound();

        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        return View(item.ToUpdateView());
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("purchases.manage")]
    public async Task<IActionResult> Edit(int id, InSupplierUpdateView model, string? returnUrl, CancellationToken ct)
    {
        model.IdSupplier = id;
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputSupplierUpdate
        {
            IdSupplier = model.IdSupplier,
            SupplierCode = model.SupplierCode,
            SupplierName = model.SupplierName,
            ContactPerson = model.ContactPerson,
            Email = model.Email,
            Phone = model.Phone,
            AddressLine1 = model.AddressLine1,
            City = model.City,
            State = model.State,
            PostalCode = model.PostalCode,
            Country = model.Country,
            TaxNumber = model.TaxNumber,
            IsActive = model.IsActive
        };
        var result = await _svc.UpdateSupplierAsync(input, ct);

        var listUrl = SafeReturnUrl(returnUrl) ?? Url.Action(nameof(Index))!;
        return CommonHelpers.SuccessResponse(
            "Supplier updated successfully.",
            new { id = result.IdSupplier, redirectUrl = listUrl });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("purchases.manage")]
    public async Task<IActionResult> SetActive(int id, bool isActive, CancellationToken ct)
    {
        var input = new InputSupplierSetActive { IdSupplier = id, IsActive = isActive };
        var result = await _svc.SetSupplierActiveAsync(input, ct);

        return CommonHelpers.SuccessResponse(
            isActive ? "Supplier activated." : "Supplier deactivated.",
            new { id = result.IdSupplier, isActive = result.IsActive });
    }

    [HttpGet]
    [RequirePermission("purchases.view")]
    public async Task<IActionResult> Lookup(CancellationToken ct)
        => Json(await _svc.GetSupplierLookupAsync(ct));

    [HttpGet]
    [RequirePermission("purchases.view")]
    public async Task<IActionResult> Balance(int id, CancellationToken ct)
    {
        if (StoreId <= 0) return BadRequest("Store context required.");
        var balance = await _purchaseSvc.GetSupplierBalanceAsync(id, StoreId, ct);
        return Json(balance);
    }

    #endregion

    #region Supplier Accounts & Ledger

    [HttpGet]
    [Route("Supplier/Accounts")]
    [Route("SupplierAccount")]
    [Route("SupplierAccount/Index")]
    [RequirePermission("purchases.view")]
    public async Task<IActionResult> Accounts(CancellationToken ct)
    {
        var suppliers = await _svc.GetSupplierLookupAsync(true, ct);
        return View("Accounts", suppliers);
    }

    [HttpGet]
    [Route("Supplier/Details/{id:int?}")]
    [Route("Supplier/Details")]
    [Route("SupplierAccount/Details/{id:int?}")]
    [Route("SupplierAccount/Details")]
    [RequirePermission("purchases.view")]
    public async Task<IActionResult> Details(int id, DateOnly? from, DateOnly? to, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Supplier Statement");
        if (id <= 0) return RedirectToAction(nameof(Accounts));

        var statement = await _purchaseSvc.GetSupplierStatementAsync(id, StoreId, from, to, ct);
        return View("Details", statement);
    }

    #endregion

    #region Supplier Payments & Refunds

    [HttpGet]
    [Route("Supplier/Payments")]
    [Route("SupplierAccount/Payments")]
    [RequirePermission("purchases.view")]
    public async Task<IActionResult> Payments([FromQuery] InputSupplierTransactionList filter, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Supplier Payments");

        filter.FkStore = StoreId;
        ViewBag.Suppliers = await _svc.GetSupplierLookupAsync(true, ct);
        return View("Payments", await _purchaseSvc.ListSupplierTransactionsAsync(filter, ct));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Route("Supplier/RecordMoney")]
    [Route("SupplierAccount/RecordMoney")]
    [RequirePermission("purchases.manage")]
    public async Task<IActionResult> RecordMoney([FromForm] InSupplierPaymentView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);
        await _purchaseSvc.RecordSupplierTransactionAsync(model, StoreId, ActorId, ct);
        return Saved(model.FkSupplier);
    }

    #endregion

    #region Payment Allocations

    [HttpPost, ValidateAntiForgeryToken]
    [Route("Supplier/Allocate")]
    [Route("SupplierAccount/Allocate")]
    [RequirePermission("purchases.manage")]
    public IActionResult Allocate([FromForm] InSupplierPaymentView model)
    {
        return Saved(model.FkSupplier);
    }

    #endregion

    #region Opening Balances & Reversals

    [HttpPost, ValidateAntiForgeryToken]
    [Route("Supplier/Reverse/{id:int?}")]
    [Route("SupplierAccount/Reverse/{id:int?}")]
    [RequirePermission("purchases.manage")]
    public async Task<IActionResult> Reverse(int id, int supplierId, string reason, CancellationToken ct)
    {
        await _purchaseSvc.ReverseSupplierTransactionAsync(id, StoreId, ActorId, reason, ct);
        return Saved(supplierId);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [Route("Supplier/Opening")]
    [Route("SupplierAccount/Opening")]
    [RequirePermission("purchases.manage")]
    public async Task<IActionResult> Opening(int supplierId, [FromForm] InSupplierPaymentView model, CancellationToken ct)
    {
        model.FkSupplier = supplierId;
        model.TransactionType = "Opening";
        CommonHelpers.ValidateModelState(ModelState);
        await _purchaseSvc.RecordSupplierTransactionAsync(model, StoreId, ActorId, ct);
        return Saved(supplierId);
    }

    #endregion

    #region Private Helpers

    private IActionResult Saved(int id)
        => CommonHelpers.SuccessResponse("Supplier account updated.", new { redirectUrl = Url.Action(nameof(Details), new { id }) });

    private IActionResult StoreRequiredView(string title)
    {
        ViewData["Title"] = title;
        return View("StoreRequired");
    }

    private string? SafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            return null;
        if (!Url.IsLocalUrl(returnUrl))
            return null;
        return returnUrl;
    }

    #endregion
}
