using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>Supplier master: list, create, edit and activate / deactivate (company-wide, not store-scoped).</summary>
[Authorize]
[ModuleAccess("PURCHASES")]
public class SupplierController : Controller
{
    private readonly ISupplierService _svc;

    public SupplierController(ISupplierService svc) => _svc = svc;

    /// <summary>
    /// Lists vendors used when raising purchase orders and receiving stock, with search, status filter and paging.
    /// Staff with purchases.view open this from the Suppliers menu entry.
    /// </summary>
    [HttpGet]
    [RequirePermission("purchases.view")]
    public async Task<IActionResult> Index([FromQuery] InputSupplierList input, CancellationToken ct)
        => View(await _svc.ListSuppliersAsync(input, ct));

    /// <summary>
    /// Shows the form to onboard a supplier with contact, address, and tax details.
    /// Buyers with purchases.manage use it before creating purchase orders.
    /// </summary>
    [HttpGet]
    [RequirePermission("purchases.manage")]
    public IActionResult Create(string? returnUrl)
    {
        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        return View(new InSupplierCreateView { IsActive = true });
    }

    /// <summary>
    /// Saves a new supplier available on purchase documents.
    /// Buyers submit the create supplier form during vendor setup.
    /// </summary>
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

    /// <summary>
    /// Loads one supplier for editing contacts, address, or tax registration.
    /// Buyers with purchases.manage navigate here from the supplier list for corrections.
    /// </summary>
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

    /// <summary>
    /// Persists supplier updates shown on purchase orders and print-outs.
    /// Buyers submit the edit supplier form after revising vendor master data.
    /// </summary>
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

    /// <summary>
    /// Activates or deactivates a supplier from the list without opening the edit form.
    /// Inactive suppliers stay on existing documents but cannot be chosen on new purchase orders.
    /// </summary>
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

    /// <summary>
    /// Returns active suppliers as JSON for vendor pickers.
    /// Purchasing screens call this via AJAX when a vendor must be selected.
    /// </summary>
    [HttpGet]
    [RequirePermission("purchases.view")]
    public async Task<IActionResult> Lookup(CancellationToken ct)
        => Json(await _svc.GetSupplierLookupAsync(ct));

    private string? SafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            return null;
        if (!Url.IsLocalUrl(returnUrl))
            return null;
        return returnUrl;
    }
}
