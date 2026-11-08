using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>
/// Purchase management: list, create, edit, approve, cancel, print, and supplier balance lookup.
/// </summary>
[Authorize]
[ModuleAccess("PURCHASES")]
public class PurchaseController : Controller
{
    private readonly IPurchaseService _svc;
    private readonly ISupplierService _supplierSvc;

    private int ActorId => AuthClaims.GetUserId(User) ?? 0;
    private int StoreId => AuthClaims.GetFkStore(User) ?? 0;

    public PurchaseController(IPurchaseService svc, ISupplierService supplierSvc)
    {
        _svc = svc;
        _supplierSvc = supplierSvc;
    }

    #region Purchase Management

    // ═══════════════════════════════════════════════════════════════
    // LIST
    // ═══════════════════════════════════════════════════════════════

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

    [HttpGet]
    [RequirePermission("purchases.manage")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("New Purchase");

        ViewBag.Suppliers = await _supplierSvc.GetSupplierLookupAsync(ct);
        return View(new InPurchaseCreateView());
    }

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
            InvoiceDate = model.InvoiceDate,
            MarkAsApproved = model.MarkAsApproved,
            AmountPaid = model.AmountPaid,
            PaymentMethod = model.PaymentMethod,
            Notes = model.Notes,
            CreatedBy = ActorId,
            Lines = model.Lines.Select(l => new InputPurchaseLine
            {
                FkProduct = l.FkProduct,
                FkProductVariant = l.FkProductVariant,
                Quantity = l.Quantity,
                UnitCost = l.UnitCost,
                DiscountAmount = l.DiscountAmount,
                TaxRate = l.TaxRate,
                TaxInclusive = l.TaxInclusive
            }).ToList()
        };

        var result = await _svc.CreatePurchaseAsync(input, ct);

        return CommonHelpers.SuccessResponse(
            model.MarkAsApproved ? "Purchase recorded and approved into stock." : "Purchase saved as Pending.",
            new { id = result.IdPurchase, redirectUrl = Url.Action(nameof(Details), new { id = result.IdPurchase }) });
    }

    // ═══════════════════════════════════════════════════════════════
    // DETAILS / PRINT
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    [RequirePermission("purchases.view")]
    public async Task<IActionResult> Details(int id, string? notice, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Purchase Details");

        var select = new InputPurchaseSelect { IdPurchase = id, FkStore = StoreId };
        var item = await _svc.GetPurchaseAsync(select, ct);
        if (item is null) return NotFound();

        ViewBag.Notice = NoticeText(notice);
        return View(item);
    }

    [HttpGet]
    [RequirePermission("purchases.view")]
    public async Task<IActionResult> Print(int id, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Purchase Print");

        var select = new InputPurchaseSelect { IdPurchase = id, FkStore = StoreId };
        var item = await _svc.GetPurchaseAsync(select, ct);
        if (item is null) return NotFound();

        return View(item);
    }

    // ═══════════════════════════════════════════════════════════════
    // EDIT
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    [RequirePermission("purchases.manage")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Edit Purchase");

        var select = new InputPurchaseSelect { IdPurchase = id, FkStore = StoreId };
        var item = await _svc.GetPurchaseAsync(select, ct);
        if (item is null) return NotFound();
        if (!item.CanEdit)
            return RedirectToAction(nameof(Details), new { id, notice = "edit-locked" });

        ViewBag.Purchase = item;
        ViewBag.Suppliers = await _supplierSvc.GetSupplierLookupAsync(true, ct);
        return View(item.ToUpdateView());
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("purchases.manage")]
    public async Task<IActionResult> Edit(int id, [FromBody] InPurchaseUpdateView model, CancellationToken ct)
    {
        ModelState.Remove(nameof(InPurchaseUpdateView.IdPurchase));
        CommonHelpers.ValidateModelState(ModelState);
        model.IdPurchase = id;

        var input = new InputPurchaseUpdate
        {
            IdPurchase = model.IdPurchase,
            FkStore = StoreId,
            FkSupplier = model.FkSupplier,
            PurchaseDate = model.PurchaseDate,
            InvoiceNumber = model.InvoiceNumber,
            InvoiceDate = model.InvoiceDate,
            Notes = model.Notes,
            Lines = model.Lines.Select(l => new InputPurchaseLine
            {
                FkProduct = l.FkProduct,
                FkProductVariant = l.FkProductVariant,
                Quantity = l.Quantity,
                UnitCost = l.UnitCost,
                DiscountAmount = l.DiscountAmount,
                TaxRate = l.TaxRate,
                TaxInclusive = l.TaxInclusive
            }).ToList()
        };

        var result = await _svc.UpdatePurchaseAsync(input, ct);

        return CommonHelpers.SuccessResponse(
            "Purchase updated successfully.",
            new { id = result.IdPurchase, redirectUrl = Url.Action(nameof(Details), new { id = result.IdPurchase }) });
    }

    // ═══════════════════════════════════════════════════════════════
    // APPROVE
    // ═══════════════════════════════════════════════════════════════

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("purchases.approve")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Approve Purchase");

        await _svc.ApprovePurchaseAsync(id, StoreId, ActorId, ct);

        return CommonHelpers.SuccessResponse(
            "Purchase approved into stock and posted to supplier ledger.",
            new { id, redirectUrl = Url.Action(nameof(Details), new { id }) });
    }

    // ═══════════════════════════════════════════════════════════════
    // CANCEL
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    [RequirePermission("purchases.manage")]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Cancel Purchase");

        var select = new InputPurchaseSelect { IdPurchase = id, FkStore = StoreId };
        var item = await _svc.GetPurchaseAsync(select, ct);
        if (item is null) return NotFound();
        if (!item.CanCancel)
            return RedirectToAction(nameof(Details), new { id, notice = "cancel-locked" });

        return View(item);
    }

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
            "Purchase cancelled.",
            new { id, redirectUrl = Url.Action(nameof(Details), new { id }) });
    }

    // ═══════════════════════════════════════════════════════════════
    // LOOKUPS
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    [RequirePermission("purchases.manage")]
    public async Task<IActionResult> ProductLookup(string? term, CancellationToken ct)
    {
        var input = new InputPurchaseProductList { Search = term };
        var items = await _svc.ListProductsAsync(input, ct);

        return CommonHelpers.SuccessResponse("Products loaded.", items);
    }

    [HttpGet]
    [RequirePermission("purchases.view")]
    public async Task<IActionResult> SupplierBalance(int supplierId, CancellationToken ct)
    {
        if (StoreId <= 0) return BadRequest("Store context required.");
        var balance = await _svc.GetSupplierBalanceAsync(supplierId, StoreId, ct);
        return Json(balance);
    }

    #endregion

    #region Private Helpers

    private IActionResult StoreRequiredView(string title)
    {
        ViewData["Title"] = title;
        return View("StoreRequired");
    }

    private static string? NoticeText(string? code) => code switch
    {
        "edit-locked" => "This purchase can no longer be edited. Only pending purchases can be changed.",
        "cancel-locked" => "This purchase cannot be cancelled because it is already approved or cancelled.",
        _ => null
    };

    #endregion
}
