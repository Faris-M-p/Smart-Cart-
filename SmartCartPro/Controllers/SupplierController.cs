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

    [HttpGet]
    [RequirePermission("purchasing.view")]
    public async Task<IActionResult> Index([FromQuery] InputSupplierList input, CancellationToken ct)
    {
        input.Page = Math.Max(1, input.Page);
        return View(await _svc.ListSuppliersAsync(input, ct));
    }

    [HttpGet]
    [RequirePermission("purchasing.manage")]
    public IActionResult Create() => View(new InSupplierCreateView { IsActive = true });

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

    [HttpGet]
    [RequirePermission("purchasing.manage")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var item = await _svc.GetSupplierAsync(new InputSupplierSelect { IdSupplier = id }, ct);
        if (item is null) return NotFound();
        return View(item.ToUpdateView());
    }

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

    [HttpGet]
    public async Task<IActionResult> Lookup(CancellationToken ct)
        => Json(await _svc.GetSupplierLookupAsync(ct));
}
