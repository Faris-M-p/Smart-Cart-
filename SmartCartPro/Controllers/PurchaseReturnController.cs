using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

[Authorize]
[ModuleAccess("PURCHASES")]
public class PurchaseReturnController : Controller
{
    private readonly IPurchaseService _service;
    private readonly ISupplierService _supplierSvc;

    private int ActorId => AuthClaims.GetUserId(User) ?? 0;
    private int StoreId => AuthClaims.GetFkStore(User) ?? 0;

    public PurchaseReturnController(IPurchaseService service, ISupplierService supplierSvc)
    {
        _service = service;
        _supplierSvc = supplierSvc;
    }

    #region Purchase Returns

    [HttpGet]
    [RequirePermission("purchasereturns.view")]
    public async Task<IActionResult> Index([FromQuery] InputPurchaseReturnList input, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Purchase Returns");

        input.FkStore = StoreId;
        ViewBag.Suppliers = await _supplierSvc.GetSupplierLookupAsync(true, ct);
        return View(await _service.ListReturnsAsync(input, ct));
    }

    [HttpGet]
    [RequirePermission("purchasereturns.manage")]
    public async Task<IActionResult> Sources(string? search, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Select Purchase to Return");

        ViewBag.Search = search;
        var sources = await _service.ListReturnSourcesAsync(StoreId, search, ct);
        return View(sources);
    }

    [HttpGet]
    [RequirePermission("purchasereturns.manage")]
    public async Task<IActionResult> Create(int? purchaseId, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Create Purchase Return");

        if (!purchaseId.HasValue || purchaseId <= 0)
        {
            return RedirectToAction(nameof(Sources));
        }

        var purchase = await _service.GetPurchaseAsync(new InputPurchaseSelect { IdPurchase = purchaseId.Value, FkStore = StoreId }, ct);
        if (purchase is null) return NotFound();

        if (purchase.Status != "Approved" || purchase.Cancelled)
        {
            TempData["ErrorMessage"] = "Returns can only be created against approved, non-cancelled purchases.";
            return RedirectToAction(nameof(Sources));
        }

        var eligibleLines = purchase.Lines.Where(l => l.EligibleReturnQty > 0).ToList();
        if (eligibleLines.Count == 0)
        {
            TempData["ErrorMessage"] = "All items from this purchase have already been returned.";
            return RedirectToAction(nameof(Sources));
        }

        ViewBag.Purchase = purchase;

        var vm = new InPurchaseReturnCreateView
        {
            FkPurchase = purchase.IdPurchase,
            FkSupplier = purchase.FkSupplier,
            ReturnDate = DateOnly.FromDateTime(DateTime.Today),
            Lines = eligibleLines.Select(l => new InPurchaseReturnLineView
            {
                FkPurchaseDetail = l.IdPurchaseDetail,
                FkProduct = l.FkProduct,
                FkProductVariant = l.FkProductVariant,
                Quantity = l.EligibleReturnQty,
                UnitCost = l.UnitCost
            }).ToList()
        };

        return View(vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("purchasereturns.manage")]
    public async Task<IActionResult> Create([FromBody] InPurchaseReturnCreateView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var id = await _service.CreateReturnAsync(model, StoreId, ActorId, ct);

        return CommonHelpers.SuccessResponse(
            model.MarkAsApproved ? "Purchase return created and posted to stock & balance." : "Purchase return created as Pending.",
            new { id, redirectUrl = Url.Action(nameof(Details), new { id }) });
    }

    [HttpGet]
    [RequirePermission("purchasereturns.view")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Purchase Return Details");

        var item = await _service.GetReturnAsync(id, StoreId, ct);
        if (item is null) return NotFound();

        return View(item);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("purchasereturns.post")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Approve Purchase Return");

        await _service.ApproveReturnAsync(id, StoreId, ActorId, ct);

        return CommonHelpers.SuccessResponse(
            "Purchase return approved. Inventory reduced and supplier balance credited.",
            new { id, redirectUrl = Url.Action(nameof(Details), new { id }) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("purchasereturns.manage")]
    public async Task<IActionResult> Cancel(int id, string reason, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Cancel Purchase Return");

        await _service.CancelReturnAsync(id, StoreId, reason, ct);

        return CommonHelpers.SuccessResponse(
            "Purchase return cancelled.",
            new { id, redirectUrl = Url.Action(nameof(Details), new { id }) });
    }

    [HttpGet]
    [RequirePermission("purchasereturns.view")]
    public async Task<IActionResult> Print(int id, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Purchase Return Print");

        var item = await _service.GetReturnAsync(id, StoreId, ct);
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
