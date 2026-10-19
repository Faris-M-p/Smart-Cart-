using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>Purchase orders of the signed-in user's store: list, create, edit, print, receive goods (GRN) and cancel.</summary>
[Authorize]
[ModuleAccess("PURCHASES")]
public class PurchaseController : Controller
{
    private readonly IPurchaseService _svc;
    private readonly ISupplierService _supplierSvc;

    private int ActorId => AuthClaims.GetUserId(User) ?? 0;

    /// <summary>The user's store from the sign-in claims. 0 for platform accounts, which have no store.</summary>
    private int StoreId => AuthClaims.GetFkStore(User) ?? 0;

    public PurchaseController(IPurchaseService svc, ISupplierService supplierSvc)
    {
        _svc = svc;
        _supplierSvc = supplierSvc;
    }

    // ═══════════════════════════════════════════════════════════════
    // LIST
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Paged purchase order list of the current store with status, supplier and date filters.
    /// Staff with purchases.view open this from the Purchases menu entry.
    /// </summary>
    [HttpGet]
    [RequirePermission("purchases.view")]
    public async Task<IActionResult> Index([FromQuery] InputPurchaseList input, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Purchases");

        input.FkStore = StoreId;
        ViewBag.Suppliers = await _supplierSvc.GetSupplierLookupAsync(true, ct);
        return View(await _svc.ListPurchasesAsync(input, ct));
    }

    // ═══════════════════════════════════════════════════════════════
    // CREATE
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Shows the new purchase order form with header fields and the product line grid.
    /// Buyers with purchases.manage start a purchase order here.
    /// </summary>
    [HttpGet]
    [RequirePermission("purchases.manage")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("New purchase order");

        ViewBag.Suppliers = await _supplierSvc.GetSupplierLookupAsync(ct);
        return View(new InPurchaseCreateView());
    }

    /// <summary>
    /// Saves a new purchase order sent as JSON; tax and totals are recomputed on the server.
    /// The page script posts the document here and then opens the saved order.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("purchases.manage")]
    public async Task<IActionResult> Create([FromBody] InPurchaseCreateView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputPurchaseCreate
        {
            FkStore = StoreId,
            FkSupplier = model.FkSupplier,
            PurchaseDate = model.PurchaseDate,
            InvoiceNumber = model.InvoiceNumber,
            Notes = model.Notes,
            SaveAsDraft = model.SaveAsDraft,
            CreatedBy = ActorId,
            Lines = model.Lines.Select(l => new InputPurchaseLine
            {
                FkProduct = l.FkProduct,
                FkProductVariant = l.FkProductVariant,
                Quantity = l.Quantity,
                UnitCost = l.UnitCost,
                DiscountAmount = l.DiscountAmount
            }).ToList()
        };
        var result = await _svc.CreatePurchaseAsync(input, ct);

        return CommonHelpers.SuccessResponse(
            model.SaveAsDraft ? "Draft purchase order saved." : "Purchase order created.",
            new { id = result.IdPurchase, redirectUrl = Url.Action(nameof(Details), new { id = result.IdPurchase }) });
    }

    // ═══════════════════════════════════════════════════════════════
    // DETAILS / PRINT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Shows one purchase order with lines, received quantities, receipts and the actions still allowed.
    /// Opened from the list; guard redirects from edit, receive and cancel also land here with a notice.
    /// </summary>
    [HttpGet]
    [RequirePermission("purchases.view")]
    public async Task<IActionResult> Details(int id, string? notice, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Purchase order");

        var select = new InputPurchaseSelect { IdPurchase = id, FkStore = StoreId };
        var item = await _svc.GetPurchaseAsync(select, ct);
        if (item is null) return NotFound();

        ViewBag.Notice = NoticeText(notice);
        return View(item);
    }

    /// <summary>
    /// Print-friendly purchase order sheet that opens the browser print dialog.
    /// Opened in a new tab from the purchase order details page.
    /// </summary>
    [HttpGet]
    [RequirePermission("purchases.view")]
    public async Task<IActionResult> Print(int id, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Purchase order");

        var select = new InputPurchaseSelect { IdPurchase = id, FkStore = StoreId };
        var item = await _svc.GetPurchaseAsync(select, ct);
        if (item is null) return NotFound();

        return View(item);
    }

    // ═══════════════════════════════════════════════════════════════
    // EDIT
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Opens a draft or posted purchase order without receipts for editing; anything else goes back to the details page.
    /// Buyers with purchases.manage reach it from the details page.
    /// </summary>
    [HttpGet]
    [RequirePermission("purchases.manage")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Edit purchase order");

        var select = new InputPurchaseSelect { IdPurchase = id, FkStore = StoreId };
        var item = await _svc.GetPurchaseAsync(select, ct);
        if (item is null) return NotFound();
        if (!item.CanEdit)
            return RedirectToAction(nameof(Details), new { id, notice = "edit-locked" });

        ViewBag.Purchase = item;
        ViewBag.Suppliers = await _supplierSvc.GetSupplierLookupAsync(true, ct);
        return View(item.ToUpdateView());
    }

    /// <summary>
    /// Saves changes to a draft or posted purchase order sent as JSON; totals are recomputed on the server.
    /// The page script posts the document here and then returns to the details page.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("purchases.manage")]
    public async Task<IActionResult> Edit(int id, [FromBody] InPurchaseUpdateView model, CancellationToken ct)
    {
        model.IdPurchase = id;
        ModelState.Remove(nameof(InPurchaseUpdateView.IdPurchase));
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputPurchaseUpdate
        {
            IdPurchase = model.IdPurchase,
            FkStore = StoreId,
            FkSupplier = model.FkSupplier,
            PurchaseDate = model.PurchaseDate,
            InvoiceNumber = model.InvoiceNumber,
            Notes = model.Notes,
            SaveAsDraft = model.SaveAsDraft,
            Lines = model.Lines.Select(l => new InputPurchaseLine
            {
                FkProduct = l.FkProduct,
                FkProductVariant = l.FkProductVariant,
                Quantity = l.Quantity,
                UnitCost = l.UnitCost,
                DiscountAmount = l.DiscountAmount
            }).ToList()
        };
        var result = await _svc.UpdatePurchaseAsync(input, ct);

        return CommonHelpers.SuccessResponse(
            "Purchase order updated.",
            new { id = result.IdPurchase, redirectUrl = Url.Action(nameof(Details), new { id = result.IdPurchase }) });
    }

    // ═══════════════════════════════════════════════════════════════
    // RECEIVE (GRN)
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Shows the goods receipt form with ordered, already received and pending quantities per line.
    /// Receivers with purchases.manage open it from the details page of a posted order.
    /// </summary>
    [HttpGet]
    [RequirePermission("purchases.manage")]
    public async Task<IActionResult> Receive(int id, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Receive goods");

        var select = new InputPurchaseSelect { IdPurchase = id, FkStore = StoreId };
        var item = await _svc.GetPurchaseAsync(select, ct);
        if (item is null) return NotFound();
        if (!item.CanReceive)
            return RedirectToAction(nameof(Details), new { id, notice = "receive-locked" });

        return View(item);
    }

    /// <summary>
    /// Posts a goods receipt sent as JSON; quantities above what is still pending are rejected.
    /// The page script posts the receipt here and then returns to the details page.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("purchases.manage")]
    public async Task<IActionResult> Receive(int id, [FromBody] InGoodsReceiptView model, CancellationToken ct)
    {
        model.IdPurchase = id;
        ModelState.Remove(nameof(InGoodsReceiptView.IdPurchase));
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputGoodsReceipt
        {
            IdPurchase = model.IdPurchase,
            FkStore = StoreId,
            ReceiptDate = model.ReceiptDate,
            Notes = model.Notes,
            CreatedBy = ActorId,
            Lines = model.Lines.Select(l => new InputGrnLine
            {
                IdPurchaseDetail = l.IdPurchaseDetail,
                ReceiveQty = l.ReceiveQty
            }).ToList()
        };
        var result = await _svc.ReceiveGoodsAsync(input, ct);

        var statusText = result.NewPurchaseStatus.Replace('_', ' ');
        return CommonHelpers.SuccessResponse(
            $"Goods receipt {result.GrnNumber} recorded. The purchase order is now {statusText}.",
            new
            {
                id,
                grnNumber = result.GrnNumber,
                purchaseStatus = result.NewPurchaseStatus,
                redirectUrl = Url.Action(nameof(Details), new { id })
            });
    }

    // ═══════════════════════════════════════════════════════════════
    // CANCEL
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Shows the cancellation form that asks for a reason before a purchase order is cancelled.
    /// Managers with purchases.manage open it from the details page.
    /// </summary>
    [HttpGet]
    [RequirePermission("purchases.manage")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Cancel purchase order");

        var select = new InputPurchaseSelect { IdPurchase = id, FkStore = StoreId };
        var item = await _svc.GetPurchaseAsync(select, ct);
        if (item is null) return NotFound();
        if (!item.CanCancel)
            return RedirectToAction(nameof(Details), new { id, notice = "cancel-locked" });

        return View(item);
    }

    /// <summary>
    /// Cancels a purchase order that is not fully received and records the reason.
    /// Already received goods stay in stock; only the pending quantities are dropped.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("purchases.manage")]
    public async Task<IActionResult> Cancel(int id, InPurchaseCancelView model, CancellationToken ct)
    {
        model.IdPurchase = id;
        ModelState.Remove(nameof(InPurchaseCancelView.IdPurchase));
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputPurchaseCancel
        {
            IdPurchase = model.IdPurchase,
            FkStore = StoreId,
            Reason = model.Reason,
            CancelledBy = ActorId
        };
        await _svc.CancelPurchaseAsync(input, ct);

        return CommonHelpers.SuccessResponse(
            "Purchase order cancelled.",
            new { id, redirectUrl = Url.Action(nameof(Details), new { id }) });
    }

    // ═══════════════════════════════════════════════════════════════
    // PRODUCT PICKER
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Searches active products and variants by name, code, SKU or barcode for the purchase line picker.
    /// The purchase form calls this via AJAX while the buyer types.
    /// </summary>
    [HttpGet]
    [RequirePermission("purchases.manage")]
    public async Task<IActionResult> ProductLookup(string? term, CancellationToken ct)
    {
        var input = new InputPurchaseProductList { Search = term };
        var items = await _svc.ListProductsAsync(input, ct);

        return CommonHelpers.SuccessResponse("Products loaded.", items);
    }

    // ═══════════════════════════════════════════════════════════════
    // helpers
    // ═══════════════════════════════════════════════════════════════

    private IActionResult StoreRequiredView(string title)
    {
        ViewData["Title"] = title;
        return View("StoreRequired");
    }

    private static string? NoticeText(string? code) => code switch
    {
        "edit-locked" => "This purchase order can no longer be edited. Only drafts and posted orders without receipts can be changed.",
        "receive-locked" => "Goods cannot be received for this purchase order right now. It must be posted and still have pending quantities.",
        "cancel-locked" => "This purchase order cannot be cancelled because it is already cancelled or fully received.",
        _ => null
    };
}
