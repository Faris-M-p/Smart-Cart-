using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Exceptions;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>Supplier master CRUD.</summary>
[Authorize]
[ModuleAccess("PURCHASES")]
public class SupplierController : Controller
{
    private readonly ISupplierService _svc;
    private int ActorId => AuthClaims.GetUserId(User) ?? 0;

    public SupplierController(ISupplierService svc) => _svc = svc;

    /// <summary>
    /// Lists vendors used when raising purchase orders and receiving stock into the store.
    /// Staff with purchasing.view open this from the Purchases supplier master screen.
    /// </summary>
    [HttpGet]
    [RequirePermission("purchasing.view")]
    public async Task<IActionResult> Index([FromQuery] InputSupplierList input, CancellationToken ct)
    {
        input.Page = Math.Max(1, input.Page);
        return View(await _svc.ListSuppliersAsync(input, ct));
    }

    /// <summary>
    /// Shows the form to onboard a supplier with contact, address, and tax details.
    /// Buyers with purchasing.manage use it before creating POs or GRN receipts.
    /// </summary>
    [HttpGet]
    [RequirePermission("purchasing.manage")]
    public IActionResult Create() => View(new InSupplierCreateView { IsActive = true });

    /// <summary>
    /// Saves a new supplier available on purchase documents and payable tracking.
    /// Buyers submit the create supplier form during vendor setup for the store.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("purchasing.manage")]
    public async Task<IActionResult> Create(InSupplierCreateView model, CancellationToken ct)
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
        if (!result.Ok)
            throw new UpdateFailedException("Unable to create supplier.", result.ResponseMsg ?? "Unable to create supplier.");

        return CommonHelpers.SuccessResponse("Supplier created.", new { id = result.IdSupplier });
    }

    /// <summary>
    /// Loads one supplier for editing contacts, remit-to address, or tax registration.
    /// Buyers with purchasing.manage navigate here from the supplier list for corrections.
    /// </summary>
    [HttpGet]
    [RequirePermission("purchasing.manage")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var item = await _svc.GetSupplierAsync(new InputSupplierSelect { IdSupplier = id }, ct);
        if (item is null) return NotFound();
        return View(item.ToUpdateView());
    }

    /// <summary>
    /// Persists supplier updates reflected on open POs and receiving workflows.
    /// Buyers submit the edit supplier form after revising vendor master data.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("purchasing.manage")]
    public async Task<IActionResult> Edit(int id, InSupplierUpdateView model, CancellationToken ct)
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
        if (!result.Ok)
            throw new UpdateFailedException("Unable to update supplier.", result.ResponseMsg ?? "Unable to update supplier.");

        return CommonHelpers.SuccessResponse("Supplier updated.", new { id = result.IdSupplier });
    }

    /// <summary>
    /// Returns active suppliers as JSON for purchase order and GRN vendor pickers.
    /// Purchasing screens call this via AJAX when selecting a vendor on a document.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Lookup(CancellationToken ct)
        => Json(await _svc.GetSupplierLookupAsync(ct));
}
