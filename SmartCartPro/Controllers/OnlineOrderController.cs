using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Exceptions;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>
/// Admin controller for online (storefront) order management.
///
/// MODULE ACCESS DECISION: "POS" module.
/// Rationale: No dedicated "ORDERS" module exists in the seed data
/// (DASHBOARD / PRODUCTS / INVENTORY / PURCHASES / POS / CUSTOMERS / REPORTS / SETTINGS / USERS).
/// Online order fulfillment is the closest business function to POS — both deal with
/// order processing and stock movement. When an "ORDERS" module is added to seed data
/// in a future phase, update this attribute accordingly.
///
/// Status flow: pending → confirmed → packed → shipped → delivered | cancelled
///   • Confirm  : deducts stock (movement type "sale") via IStockRepository
///   • Cancel (from confirmed+): restores stock (movement type "return_in") via IStockRepository
/// </summary>
[Authorize]
[ModuleAccess("POS")]
public class OnlineOrderController : Controller
{
    private readonly IOnlineOrderService _svc;
    private int ActorId => AuthClaims.GetUserId(User) ?? 0;
    private int StoreId => AuthClaims.GetFkStore(User) ?? 0;

    public OnlineOrderController(IOnlineOrderService svc) => _svc = svc;

    // ── LIST ─────────────────────────────────────────────────────

    [HttpGet]
    [RequirePermission("pos.sell")]
    public async Task<IActionResult> Index([FromQuery] InputOnlineOrderList input, CancellationToken ct)
    {
        if (input.FkStore is null or 0 && StoreId > 0) input.FkStore = StoreId;
        input.Page     = Math.Max(1, input.Page);
        input.PageSize = input.PageSize is <= 0 or > 200 ? 50 : input.PageSize;

        var result = await _svc.ListOrdersAsync(input, ct);
        ViewBag.Filter = input;
        return View(result);
    }

    /// <summary>JSON list for AJAX / external consumers.</summary>
    [HttpGet]
    [RequirePermission("pos.sell")]
    public async Task<IActionResult> ListJson([FromQuery] InputOnlineOrderList input, CancellationToken ct)
    {
        if (input.FkStore is null or 0 && StoreId > 0) input.FkStore = StoreId;
        input.Page     = Math.Max(1, input.Page);
        input.PageSize = input.PageSize is <= 0 or > 200 ? 50 : input.PageSize;
        return Json(await _svc.ListOrdersAsync(input, ct));
    }

    // ── DETAILS ──────────────────────────────────────────────────

    [HttpGet]
    [RequirePermission("pos.sell")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var order = await _svc.GetOrderAsync(new InputOnlineOrderSelect { IdOnlineOrder = id }, ct);
        if (order is null) return NotFound();
        return View(order);
    }

    /// <summary>JSON detail for print / preview.</summary>
    [HttpGet]
    [RequirePermission("pos.sell")]
    public async Task<IActionResult> DetailsJson(int id, CancellationToken ct)
    {
        var order = await _svc.GetOrderAsync(new InputOnlineOrderSelect { IdOnlineOrder = id }, ct);
        if (order is null) return NotFound();
        return Json(order);
    }

    // ── UPDATE STATUS ────────────────────────────────────────────

    [HttpGet]
    [RequirePermission("pos.sell")]
    public async Task<IActionResult> UpdateStatus(int id, CancellationToken ct)
    {
        var order = await _svc.GetOrderAsync(new InputOnlineOrderSelect { IdOnlineOrder = id }, ct);
        if (order is null) return NotFound();

        ViewBag.Order = order;
        return View(new InOnlineOrderStatusView { IdOnlineOrder = id });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("pos.sell")]
    public async Task<IActionResult> UpdateStatus(int id, InOnlineOrderStatusView model, CancellationToken ct)
    {
        model.IdOnlineOrder = id;
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputOnlineOrderStatusUpdate
        {
            IdOnlineOrder = model.IdOnlineOrder,
            NewStatus     = model.NewStatus,
            Reason        = model.Reason,
            UpdatedBy     = ActorId
        };
        var result = await _svc.UpdateStatusAsync(input, ct);

        if (!result.Ok)
            throw new UpdateFailedException("Unable to update order status.", result.ResponseMsg ?? "Unable to update order status.");

        return CommonHelpers.SuccessResponse(
            $"Order status updated to '{result.NewStatus}'.",
            new { id, newStatus = result.NewStatus });
    }

    /// <summary>JSON status update endpoint (for AJAX callers).</summary>
    [HttpPost]
    [RequirePermission("pos.sell")]
    public async Task<IActionResult> UpdateStatusJson([FromBody] InputOnlineOrderStatusUpdate input, CancellationToken ct)
    {
        if (input is null)
            throw new ValidationException("Request body is required.", "Null body on UpdateStatusJson.");
        input.UpdatedBy = ActorId;

        var result = await _svc.UpdateStatusAsync(input, ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to update order status.", result.ResponseMsg ?? "Unable to update order status.");

        return CommonHelpers.SuccessResponse(
            $"Order status updated to '{result.NewStatus}'.",
            new { id = input.IdOnlineOrder, newStatus = result.NewStatus });
    }
}
