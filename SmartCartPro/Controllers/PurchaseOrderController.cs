using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>
/// Simple internal product requirement / purchase order lists.
/// Non-financial: does not affect stock or supplier balances.
/// </summary>
[Authorize]
[ModuleAccess("PURCHASES")]
public class PurchaseOrderController : Controller
{
    private readonly IPurchaseService _service;
    private readonly ISupplierService _supplierSvc;

    private int ActorId => AuthClaims.GetUserId(User) ?? 0;
    private int StoreId => AuthClaims.GetFkStore(User) ?? 0;

    public PurchaseOrderController(IPurchaseService service, ISupplierService supplierSvc)
    {
        _service = service;
        _supplierSvc = supplierSvc;
    }

    #region Purchase Orders

    [HttpGet]
    [RequirePermission("purchaseorders.view")]
    public async Task<IActionResult> Index([FromQuery] InputPurchaseOrderList input, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Purchase Orders");

        input.FkStore = StoreId;
        ViewBag.Suppliers = await _supplierSvc.GetSupplierLookupAsync(true, ct);
        return View(await _service.ListOrdersAsync(input, ct));
    }

    [HttpGet]
    [RequirePermission("purchaseorders.manage")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("New Purchase Order Requirement");

        ViewBag.Suppliers = await _supplierSvc.GetSupplierLookupAsync(ct);
        return View(new InPurchaseOrderView());
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("purchaseorders.manage")]
    public async Task<IActionResult> Create([FromBody] InPurchaseOrderView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var id = await _service.SaveOrderAsync(model, StoreId, ActorId, ct);

        return CommonHelpers.SuccessResponse(
            "Purchase order requirement saved.",
            new { id, redirectUrl = Url.Action(nameof(Details), new { id }) });
    }

    [HttpGet]
    [RequirePermission("purchaseorders.view")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Purchase Order Details");

        var item = await _service.GetOrderAsync(id, StoreId, ct);
        if (item is null) return NotFound();

        return View(item);
    }

    [HttpGet]
    [RequirePermission("purchaseorders.manage")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Edit Purchase Order Requirement");

        var item = await _service.GetOrderAsync(id, StoreId, ct);
        if (item is null) return NotFound();

        ViewBag.Order = item;
        ViewBag.Suppliers = await _supplierSvc.GetSupplierLookupAsync(true, ct);
        return View(item.ToView());
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("purchaseorders.manage")]
    public async Task<IActionResult> Edit(int id, [FromBody] InPurchaseOrderView model, CancellationToken ct)
    {
        model.IdPurchaseOrder = id;
        CommonHelpers.ValidateModelState(ModelState);

        var savedId = await _service.SaveOrderAsync(model, StoreId, ActorId, ct);

        return CommonHelpers.SuccessResponse(
            "Purchase order requirement updated.",
            new { id = savedId, redirectUrl = Url.Action(nameof(Details), new { id = savedId }) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("purchaseorders.manage")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Delete Purchase Order");

        await _service.DeleteOrderAsync(id, StoreId, ct);

        return CommonHelpers.SuccessResponse(
            "Purchase order deleted.",
            new { id, redirectUrl = Url.Action(nameof(Index)) });
    }

    [HttpGet]
    [RequirePermission("purchaseorders.view")]
    public async Task<IActionResult> Print(int id, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Purchase Order Print");

        var item = await _service.GetOrderAsync(id, StoreId, ct);
        if (item is null) return NotFound();

        return View(item);
    }

    #endregion

    #region Private Helpers

    private IActionResult StoreRequiredView(string title)
    {
        ViewData["Title"] = title;
        return View("StoreRequired");
    }

    #endregion
}

