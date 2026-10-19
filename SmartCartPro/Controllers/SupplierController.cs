using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        if (!ModelState.IsValid) return View(model);
        var result = await _svc.CreateSupplierAsync(InputSupplierCreate.From(model), ct);
        if (!result.Ok) { ModelState.AddModelError(string.Empty, result.ResponseMsg); return View(model); }
        TempData["SuccessMessage"] = "Supplier created.";
        return RedirectToAction(nameof(Index));
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
        if (!ModelState.IsValid) return View(model);
        var result = await _svc.UpdateSupplierAsync(InputSupplierUpdate.From(model), ct);
        if (!result.Ok) { ModelState.AddModelError(string.Empty, result.ResponseMsg); return View(model); }
        TempData["SuccessMessage"] = "Supplier updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Lookup(CancellationToken ct)
        => Json(await _svc.GetSupplierLookupAsync(ct));
}
