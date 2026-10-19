using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Exceptions;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>Inventory management: stock balances, opening stock, adjustments, transfers, history, low-stock.</summary>
[Authorize]
[ModuleAccess("INVENTORY")]
public class InventoryController : Controller
{
    private readonly IStockService _svc;
    private int ActorId => AuthClaims.GetUserId(User) ?? 0;

    /// <summary>The signed-in user's store. 0 for platform users (no store), who get the "sign in as a store user" panel.</summary>
    private int StoreId => AuthClaims.GetFkStore(User) ?? 0;

    public InventoryController(IStockService svc) => _svc = svc;

    // ═══════════════════════════════════════════════════════════════
    // STOCK BALANCE LIST
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Sends the bare /Inventory address to the stock balance list.
    /// Keeps old bookmarks and the default MVC route working.
    /// </summary>
    [HttpGet]
    [RequirePermission("inventory.view")]
    public IActionResult Index()
        => RedirectToAction(nameof(Stock));

    /// <summary>
    /// Paginated on-hand stock of the signed-in store with search, category and low or out-of-stock filters.
    /// Store staff open this from the sidebar to see what is on the shelves.
    /// </summary>
    [HttpGet]
    [RequirePermission("inventory.view")]
    public async Task<IActionResult> Stock([FromQuery] InputInventoryList input, CancellationToken ct)
    {
        if (StoreId <= 0) return View("NoStore", "Inventory");

        // Always the signed-in user's own store; a store id sent from the browser is ignored.
        input.FkStore = StoreId;
        input.Page = Math.Max(1, input.Page);
        return View(await _svc.ListInventoryAsync(input, ct));
    }

    // ═══════════════════════════════════════════════════════════════
    // OPENING STOCK
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Form to record the first stock quantity of a product at the signed-in store.
    /// Used when onboarding a store or adding a new product to the shelves.
    /// </summary>
    [HttpGet]
    [RequirePermission("inventory.adjust")]
    public async Task<IActionResult> Opening(int? productId, int? variantId, string? returnUrl, CancellationToken ct)
    {
        if (StoreId <= 0) return View("NoStore", "Opening stock");

        OutputInventorySelect? prefill = null;
        if (productId is > 0)
        {
            var select = new InputInventorySelect
            {
                FkStore = StoreId,
                FkProduct = productId.Value,
                FkProductVariant = (variantId is > 0) ? variantId : null
            };
            prefill = await _svc.GetStockLevelAsync(select, ct);
        }

        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        ViewBag.Prefill = prefill;
        return View(new InOpeningStockView
        {
            FkProduct = prefill?.FkProduct ?? 0,
            FkProductVariant = prefill?.FkProductVariant
        });
    }

    /// <summary>
    /// Records opening stock for one product and writes the opening movement.
    /// Managers submit the opening stock form; a second opening for the same product is refused.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("inventory.adjust")]
    public async Task<IActionResult> Opening(InOpeningStockView model, string? returnUrl, CancellationToken ct)
    {
        if (StoreId <= 0)
            throw new ForbiddenException("Sign in as a store user to change stock.", "Platform user without fk_store.");

        // The store comes from the signed-in user, never from the form.
        ModelState.Remove(nameof(model.FkStore));
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputStockOpening
        {
            FkStore = StoreId,
            FkProduct = model.FkProduct,
            FkProductVariant = (model.FkProductVariant is > 0) ? model.FkProductVariant : null,
            Quantity = model.Quantity,
            Notes = model.Notes,
            CreatedBy = ActorId
        };
        var result = await _svc.ApplyOpeningStockAsync(input, ct);
        if (!result.Ok)
        {
            if (result.StatusCode == 409)
                throw new ConflictException(result.ResponseMsg, result.ResponseMsg);

            throw new UpdateFailedException(result.ResponseMsg, result.ResponseMsg);
        }

        var listUrl = SafeReturnUrl(returnUrl) ?? Url.Action(nameof(Stock))!;
        return CommonHelpers.SuccessResponse(
            $"Opening stock saved. New balance: {result.NewQuantityOnHand:0.###}",
            new { newQuantityOnHand = result.NewQuantityOnHand, redirectUrl = listUrl });
    }

    // ═══════════════════════════════════════════════════════════════
    // ADJUSTMENT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Form to add or remove stock of a product with a reason.
    /// Used for damage, expiry, found stock and count corrections.
    /// </summary>
    [HttpGet]
    [RequirePermission("inventory.adjust")]
    public async Task<IActionResult> Adjust(int? productId, int? variantId, string? returnUrl, CancellationToken ct)
    {
        if (StoreId <= 0) return View("NoStore", "Stock adjustment");

        OutputInventorySelect? prefill = null;
        if (productId is > 0)
        {
            var select = new InputInventorySelect
            {
                FkStore = StoreId,
                FkProduct = productId.Value,
                FkProductVariant = (variantId is > 0) ? variantId : null
            };
            prefill = await _svc.GetStockLevelAsync(select, ct);
        }

        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        ViewBag.Prefill = prefill;
        return View(new InStockAdjustView
        {
            FkProduct = prefill?.FkProduct ?? 0,
            FkProductVariant = prefill?.FkProductVariant
        });
    }

    /// <summary>
    /// Applies a stock adjustment and updates the on-hand balance.
    /// Managers submit the adjust stock form; removing more than is on hand is refused.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("inventory.adjust")]
    public async Task<IActionResult> Adjust(InStockAdjustView model, string? returnUrl, CancellationToken ct)
    {
        if (StoreId <= 0)
            throw new ForbiddenException("Sign in as a store user to change stock.", "Platform user without fk_store.");

        // The store comes from the signed-in user, never from the form.
        ModelState.Remove(nameof(model.FkStore));
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputStockAdjust
        {
            FkStore = StoreId,
            FkProduct = model.FkProduct,
            FkProductVariant = (model.FkProductVariant is > 0) ? model.FkProductVariant : null,
            Quantity = model.Quantity,
            Direction = model.Direction,
            Reason = model.Reason,
            Notes = model.Notes,
            CreatedBy = ActorId
        };
        var result = await _svc.AdjustStockAsync(input, ct);
        if (!result.Ok)
            throw new UpdateFailedException(result.ResponseMsg, result.ResponseMsg);

        var listUrl = SafeReturnUrl(returnUrl) ?? Url.Action(nameof(Stock))!;
        return CommonHelpers.SuccessResponse(
            $"Stock adjusted. New balance: {result.NewQuantityOnHand:0.###}",
            new { newQuantityOnHand = result.NewQuantityOnHand, redirectUrl = listUrl });
    }

    // ═══════════════════════════════════════════════════════════════
    // TRANSFER
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Document-style form to move several products from this store to another store of the company.
    /// Used for replenishing a branch from the store that has spare stock.
    /// </summary>
    [HttpGet]
    [RequirePermission("inventory.adjust")]
    public async Task<IActionResult> Transfer(int? productId, int? variantId, string? returnUrl, CancellationToken ct)
    {
        if (StoreId <= 0) return View("NoStore", "Transfer stock");

        OutputInventorySelect? prefill = null;
        if (productId is > 0)
        {
            var select = new InputInventorySelect
            {
                FkStore = StoreId,
                FkProduct = productId.Value,
                FkProductVariant = (variantId is > 0) ? variantId : null
            };
            prefill = await _svc.GetStockLevelAsync(select, ct);
        }

        ViewBag.ReturnUrl = SafeReturnUrl(returnUrl);
        ViewBag.Prefill = prefill;
        ViewBag.Stores = await _svc.GetTransferStoresAsync(StoreId, ct);
        return View(new InStockTransferCreateView());
    }

    /// <summary>
    /// Moves every line out of this store and into the chosen store in one database transaction.
    /// Managers submit the transfer document as JSON; if one line is short on stock nothing moves.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("inventory.adjust")]
    public async Task<IActionResult> Transfer([FromBody] InStockTransferCreateView model, CancellationToken ct)
    {
        if (StoreId <= 0)
            throw new ForbiddenException("Sign in as a store user to transfer stock.", "Platform user without fk_store.");

        if (model is null || model.Lines is null || model.Lines.Count == 0)
            throw new ValidationException("Add at least one product to transfer.");

        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputStockTransferCreate
        {
            FromStoreId = StoreId,
            ToStoreId = model.ToStoreId,
            Notes = model.Notes,
            CreatedBy = ActorId,
            Lines = model.Lines
                .Where(l => l is not null)
                .Select(l => new InputStockTransferLine
                {
                    FkProduct = l.FkProduct,
                    FkProductVariant = (l.FkProductVariant is > 0) ? l.FkProductVariant : null,
                    Quantity = l.Quantity
                })
                .ToList()
        };
        var result = await _svc.CreateStockTransferAsync(input, ct);
        if (!result.Ok)
            throw new UpdateFailedException(result.ResponseMsg, result.ResponseMsg);

        return CommonHelpers.SuccessResponse(
            "Stock transfer completed.",
            new { lineCount = result.LineCount, redirectUrl = Url.Action(nameof(History))! });
    }

    // ═══════════════════════════════════════════════════════════════
    // HISTORY
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Audit trail of stock movements for the signed-in store, filtered by product, type and date range.
    /// Used to trace sales, receipts, transfers and adjustments.
    /// </summary>
    [HttpGet]
    [RequirePermission("inventory.view")]
    public async Task<IActionResult> History([FromQuery] InputStockMovementList input, CancellationToken ct)
    {
        if (StoreId <= 0) return View("NoStore", "Movement history");

        input.FkStore = StoreId;
        input.Page = Math.Max(1, input.Page);
        return View(await _svc.GetMovementHistoryAsync(input, ct));
    }

    // ═══════════════════════════════════════════════════════════════
    // LOW STOCK
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Products at or below their reorder level with a suggested order quantity.
    /// Used for replenishment alerts and purchasing.
    /// </summary>
    [HttpGet]
    [RequirePermission("inventory.view")]
    public async Task<IActionResult> LowStock([FromQuery] InputLowStockList input, CancellationToken ct)
    {
        if (StoreId <= 0) return View("NoStore", "Low stock");

        input.FkStore = StoreId;
        input.Page = Math.Max(1, input.Page);
        return View(await _svc.GetLowStockAsync(input, ct));
    }

    // ═══════════════════════════════════════════════════════════════
    // PRODUCT PICKER (JSON)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Searches products and variants by name, code, SKU or barcode and returns them with the on-hand quantity at this store.
    /// The product pickers on the stock screens call this while the user types.
    /// </summary>
    [HttpGet]
    [RequirePermission("inventory.view")]
    public async Task<IActionResult> ProductPicker(string? term, CancellationToken ct)
    {
        var input = new InputStockProductSearch
        {
            FkStore = StoreId,
            Term = term,
            Limit = 15
        };
        var result = await _svc.SearchProductsAsync(input, ct);
        return CommonHelpers.SuccessResponse("OK", result.Items);
    }

    private string? SafeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            return null;
        if (!Url.IsLocalUrl(returnUrl))
            return null;
        return returnUrl;
    }
}
