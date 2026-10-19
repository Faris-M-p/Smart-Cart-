using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Exceptions;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>Inventory management: opening stock, adjustments, transfers, history, low-stock.</summary>
[Authorize]
[ModuleAccess("INVENTORY")]
public class InventoryController : Controller
{
    private readonly IStockService _svc;
    private int ActorId => AuthClaims.GetUserId(User) ?? 0;
    private int StoreId => AuthClaims.GetFkStore(User) ?? 0;

    public InventoryController(IStockService svc) => _svc = svc;

    // ═══════════════════════════════════════════════════════════════
    // STOCK BALANCE LIST
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Paginated on-hand stock balances by product and store.
    /// Used from the inventory dashboard and stock lookup.
    /// </summary>
    [HttpGet]
    [RequirePermission("inventory.view")]
    public async Task<IActionResult> Index([FromQuery] InputInventoryList input, CancellationToken ct)
    {
        if (input.FkStore == 0) input.FkStore = StoreId;
        input.Page = Math.Max(1, input.Page);
        return View(await _svc.ListInventoryAsync(input, ct));
    }

    // ═══════════════════════════════════════════════════════════════
    // OPENING STOCK
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Form to set initial opening quantities for a SKU.
    /// Used once when onboarding stock or new locations.
    /// </summary>
    [HttpGet]
    [RequirePermission("inventory.manage")]
    public IActionResult Opening()
        => View(new InOpeningStockView { FkStore = StoreId });

    /// <summary>
    /// Applies opening stock and writes the opening movement.
    /// Used on submit of the opening stock form.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("inventory.manage")]
    public async Task<IActionResult> Opening(InOpeningStockView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var result = await _svc.ApplyOpeningStockAsync(model, ActorId, ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to save opening stock.", result.ResponseMsg ?? "Unable to save opening stock.");

        return CommonHelpers.SuccessResponse(
            $"Opening stock saved. New balance: {result.NewQuantityOnHand}",
            new { newQuantityOnHand = result.NewQuantityOnHand });
    }

    // ═══════════════════════════════════════════════════════════════
    // ADJUSTMENT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Form to increase or decrease stock with a reason.
    /// Used for shrinkage, corrections, and cycle counts.
    /// </summary>
    [HttpGet]
    [RequirePermission("inventory.manage")]
    public IActionResult Adjust()
        => View(new InStockAdjustView { FkStore = StoreId });

    /// <summary>
    /// Posts a stock adjustment and updates on-hand balance.
    /// Used on submit of the adjust stock form.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("inventory.manage")]
    public async Task<IActionResult> Adjust(InStockAdjustView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var result = await _svc.AdjustStockAsync(model, ActorId, ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to adjust stock.", result.ResponseMsg ?? "Unable to adjust stock.");

        return CommonHelpers.SuccessResponse(
            $"Stock adjusted. New balance: {result.NewQuantityOnHand}",
            new { newQuantityOnHand = result.NewQuantityOnHand });
    }

    // ═══════════════════════════════════════════════════════════════
    // TRANSFER
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Form to move quantity between stores or warehouses.
    /// Used for inter-location stock replenishment.
    /// </summary>
    [HttpGet]
    [RequirePermission("inventory.manage")]
    public IActionResult Transfer()
        => View(new InStockTransferView { FromStoreId = StoreId });

    /// <summary>
    /// Executes a store-to-store transfer in one transaction.
    /// Used on submit of the transfer stock form.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("inventory.manage")]
    public async Task<IActionResult> Transfer(InStockTransferView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputStockTransfer
        {
            FromStoreId = model.FromStoreId,
            ToStoreId = model.ToStoreId,
            FkProduct = model.FkProduct,
            FkProductVariant = model.FkProductVariant,
            Quantity = model.Quantity,
            Notes = model.Notes,
            CreatedBy = ActorId
        };
        var result = await _svc.TransferStockAsync(input, ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to transfer stock.", result.ResponseMsg ?? "Unable to transfer stock.");

        return CommonHelpers.SuccessResponse("Stock transfer completed.", new { fromStoreId = model.FromStoreId, toStoreId = model.ToStoreId });
    }

    // ═══════════════════════════════════════════════════════════════
    // HISTORY
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Audit trail of stock movements with filters.
    /// Used to trace sales, receipts, and adjustments.
    /// </summary>
    [HttpGet]
    [RequirePermission("inventory.view")]
    public async Task<IActionResult> History([FromQuery] InputStockMovementList input, CancellationToken ct)
    {
        if (input.FkStore is null or 0) input.FkStore = StoreId;
        input.Page = Math.Max(1, input.Page);
        return View(await _svc.GetMovementHistoryAsync(input, ct));
    }

    // ═══════════════════════════════════════════════════════════════
    // LOW STOCK
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Lists SKUs at or below reorder thresholds.
    /// Used for replenishment alerts and purchasing.
    /// </summary>
    [HttpGet]
    [RequirePermission("inventory.view")]
    public async Task<IActionResult> LowStock([FromQuery] InputLowStockList input, CancellationToken ct)
    {
        if (input.FkStore == 0) input.FkStore = StoreId;
        input.Page = Math.Max(1, input.Page);
        return View(await _svc.GetLowStockAsync(input, ct));
    }
}
