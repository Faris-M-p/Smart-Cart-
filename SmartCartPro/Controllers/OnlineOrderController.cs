using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Exceptions;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>
/// Admin controller for online (storefront) order fulfillment.
/// Module: POS — online fulfillment shares order and stock movement with the register.
/// Status flow lives on the output model; confirm deducts stock and cancel restores it in one transaction.
/// </summary>
[Authorize]
[ModuleAccess("POS")]
public class OnlineOrderController : Controller
{
    private readonly IOnlineOrderService _svc;
    private readonly ICurrentUserAccess _access;

    private int ActorId => AuthClaims.GetUserId(User) ?? 0;
    private int StoreId => AuthClaims.GetFkStore(User) ?? 0;

    public OnlineOrderController(IOnlineOrderService svc, ICurrentUserAccess access)
    {
        _svc = svc;
        _access = access;
    }

    /// <summary>
    /// Paged list of storefront orders for the signed-in store, with status, date and search filters.
    /// Staff with pos.sell open this from the online orders queue. Platform accounts must sign in as a store user.
    /// </summary>
    [HttpGet]
    [RequirePermission("pos.sell")]
    public async Task<IActionResult> Index([FromQuery] InputOnlineOrderList input, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Online orders");

        input.FkStore = StoreId;
        return View(await _svc.ListOrdersAsync(input, ct));
    }

    /// <summary>
    /// JSON list of storefront orders for AJAX refresh of the fulfillment queue.
    /// Staff with pos.sell call this from the orders screen.
    /// </summary>
    [HttpGet]
    [RequirePermission("pos.sell")]
    public async Task<IActionResult> ListJson([FromQuery] InputOnlineOrderList input, CancellationToken ct)
    {
        if (StoreId <= 0)
            throw new ValidationException("Sign in as a store user to use this screen.", "OnlineOrder ListJson without fk_store.");

        input.FkStore = StoreId;
        var result = await _svc.ListOrdersAsync(input, ct);
        return CommonHelpers.SuccessResponse("Orders loaded.", result);
    }

    /// <summary>
    /// Full online order detail with lines, customer, and allowed next statuses.
    /// Staff open an order from the list to confirm, pack, ship, deliver, or cancel.
    /// </summary>
    [HttpGet]
    [RequirePermission("pos.sell")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Online order");

        var select = new InputOnlineOrderSelect { IdOnlineOrder = id, FkStore = StoreId };
        var order = await _svc.GetOrderAsync(select, ct);
        if (order is null) return NotFound();

        await ApplyCancelPermissionAsync(order, ct);
        return View(order);
    }

    /// <summary>
    /// JSON detail of one online order, including allowed next statuses, for print or AJAX preview.
    /// Staff with pos.sell call this from the order detail screen.
    /// </summary>
    [HttpGet]
    [RequirePermission("pos.sell")]
    public async Task<IActionResult> DetailsJson(int id, CancellationToken ct)
    {
        if (StoreId <= 0)
            throw new ValidationException("Sign in as a store user to use this screen.", "OnlineOrder DetailsJson without fk_store.");

        var select = new InputOnlineOrderSelect { IdOnlineOrder = id, FkStore = StoreId };
        var order = await _svc.GetOrderAsync(select, ct);
        if (order is null)
            throw new ResourceNotFoundException("Order not found.", $"Online order {id} was not found for store {StoreId}.");

        await ApplyCancelPermissionAsync(order, ct);
        return CommonHelpers.SuccessResponse("Order loaded.", order);
    }

    /// <summary>
    /// Redirects the old status-change form to the order detail, which now drives allowed next statuses.
    /// Staff who still have a bookmark land on the same fulfillment screen.
    /// </summary>
    [HttpGet]
    [RequirePermission("pos.sell")]
    public IActionResult UpdateStatus(int id) => RedirectToAction(nameof(Details), new { id });

    /// <summary>
    /// Applies a status change and related stock effects (deduct on confirm, restore on cancel).
    /// Staff with pos.sell advance fulfillment; cancelling also requires pos.cancel.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("pos.sell")]
    public async Task<IActionResult> UpdateStatus(int id, InOnlineOrderStatusView model, CancellationToken ct)
    {
        if (StoreId <= 0)
            throw new ValidationException("Sign in as a store user to use this screen.", "OnlineOrder UpdateStatus without fk_store.");

        model.IdOnlineOrder = id;
        CommonHelpers.ValidateModelState(ModelState);

        if (string.Equals(model.NewStatus, "cancelled", StringComparison.OrdinalIgnoreCase)
            && !await _access.CanAsync("pos.cancel", ct))
        {
            throw new ForbiddenException(
                "You do not have permission to cancel orders.",
                $"User {ActorId} attempted cancel on online order {id} without pos.cancel.");
        }

        var input = new InputOnlineOrderStatusUpdate
        {
            IdOnlineOrder = model.IdOnlineOrder,
            FkStore = StoreId,
            NewStatus = model.NewStatus,
            Reason = model.Reason,
            UpdatedBy = ActorId
        };
        var result = await _svc.UpdateStatusAsync(input, ct);

        return CommonHelpers.SuccessResponse(
            $"Order status updated to '{result.NewStatus}'.",
            new { id, newStatus = result.NewStatus, redirectUrl = Url.Action(nameof(Details), new { id }) });
    }

    /// <summary>
    /// JSON status update for AJAX callers. Same stock rules and cancel permission as the form post.
    /// Staff with pos.sell (and pos.cancel for cancellations) call this from the detail screen.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("pos.sell")]
    public async Task<IActionResult> UpdateStatusJson([FromBody] InputOnlineOrderStatusUpdate input, CancellationToken ct)
    {
        if (StoreId <= 0)
            throw new ValidationException("Sign in as a store user to use this screen.", "OnlineOrder UpdateStatusJson without fk_store.");
        if (input is null)
            throw new ValidationException("Request body is required.", "Null body on UpdateStatusJson.");

        input.UpdatedBy = ActorId;
        input.FkStore = StoreId;

        if (string.Equals(input.NewStatus, "cancelled", StringComparison.OrdinalIgnoreCase)
            && !await _access.CanAsync("pos.cancel", ct))
        {
            throw new ForbiddenException(
                "You do not have permission to cancel orders.",
                $"User {ActorId} attempted cancel on online order {input.IdOnlineOrder} without pos.cancel.");
        }

        var result = await _svc.UpdateStatusAsync(input, ct);
        return CommonHelpers.SuccessResponse(
            $"Order status updated to '{result.NewStatus}'.",
            new { id = input.IdOnlineOrder, newStatus = result.NewStatus });
    }

    private async Task ApplyCancelPermissionAsync(OutputOnlineOrderSelect order, CancellationToken ct)
    {
        if (await _access.CanAsync("pos.cancel", ct)) return;
        order.AllowedNextStatuses.RemoveAll(s => string.Equals(s, "cancelled", StringComparison.OrdinalIgnoreCase));
    }

    private IActionResult StoreRequiredView(string title)
    {
        ViewData["Title"] = title;
        return View("StoreRequired");
    }
}
