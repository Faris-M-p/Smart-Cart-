using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>Purchase order management: create, edit, receive (GRN), cancel.</summary>
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

    // ═══════════════════════════════════════════════════════════════
    // LIST
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    [RequirePermission("purchasing.view")]
    public async Task<IActionResult> Index([FromQuery] InputPurchaseList input, CancellationToken ct)
    {
        if (input.FkStore is null or 0) input.FkStore = StoreId;
        input.Page = Math.Max(1, input.Page);
        return View(await _svc.ListPurchasesAsync(input, ct));
    }

    // ═══════════════════════════════════════════════════════════════
    // CREATE
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    [RequirePermission("purchasing.manage")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        ViewBag.Suppliers = await _supplierSvc.GetSupplierLookupAsync(ct);
        return View(new InPurchaseCreateView { FkStore = StoreId, PurchaseDate = DateOnly.FromDateTime(DateTime.Today) });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("purchasing.manage")]
    public async Task<IActionResult> Create(InPurchaseCreateView model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Suppliers = await _supplierSvc.GetSupplierLookupAsync(ct);
            return View(model);
        }

        if (!model.Lines.Any())
        {
            ModelState.AddModelError(string.Empty, "At least one line item is required.");
            ViewBag.Suppliers = await _supplierSvc.GetSupplierLookupAsync(ct);
            return View(model);
        }

        var result = await _svc.CreatePurchaseAsync(InputPurchaseCreate.From(model, ActorId), ct);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.ResponseMsg);
            ViewBag.Suppliers = await _supplierSvc.GetSupplierLookupAsync(ct);
            return View(model);
        }
        TempData["SuccessMessage"] = "Purchase order created.";
        return RedirectToAction(nameof(Details), new { id = result.IdPurchase });
    }

    // ═══════════════════════════════════════════════════════════════
    // DETAILS / EDIT
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    [RequirePermission("purchasing.view")]
    public async Task<IActionResult> Details(int id, CancellationToken ct)
    {
        var item = await _svc.GetPurchaseAsync(new InputPurchaseSelect { IdPurchase = id }, ct);
        if (item is null) return NotFound();
        return View(item);
    }

    [HttpGet]
    [RequirePermission("purchasing.manage")]
    public async Task<IActionResult> Edit(int id, CancellationToken ct)
    {
        var item = await _svc.GetPurchaseAsync(new InputPurchaseSelect { IdPurchase = id }, ct);
        if (item is null) return NotFound();
        if (item.Status != "posted")
        {
            TempData["ErrorMessage"] = "Only posted purchases can be edited.";
            return RedirectToAction(nameof(Details), new { id });
        }

        ViewBag.Suppliers = await _supplierSvc.GetSupplierLookupAsync(ct);
        var view = new InPurchaseUpdateView
        {
            IdPurchase = item.IdPurchase,
            FkStore = item.FkStore,
            FkSupplier = item.FkSupplier,
            PurchaseDate = item.PurchaseDate,
            InvoiceNumber = item.InvoiceNumber,
            Notes = item.Notes,
            Lines = item.Lines.Select(l => new InPurchaseLineView
            {
                FkProduct = l.FkProduct,
                FkProductVariant = l.FkProductVariant,
                Quantity = l.Quantity,
                UnitCost = l.UnitCost
            }).ToList()
        };
        return View(view);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("purchasing.manage")]
    public async Task<IActionResult> Edit(int id, InPurchaseUpdateView model, CancellationToken ct)
    {
        model.IdPurchase = id;
        if (!ModelState.IsValid)
        {
            ViewBag.Suppliers = await _supplierSvc.GetSupplierLookupAsync(ct);
            return View(model);
        }

        var input = new InputPurchaseUpdate
        {
            IdPurchase = model.IdPurchase,
            FkStore = model.FkStore,
            FkSupplier = model.FkSupplier,
            PurchaseDate = model.PurchaseDate,
            InvoiceNumber = model.InvoiceNumber,
            Notes = model.Notes,
            Lines = model.Lines.Select(l => new InputPurchaseLine
            {
                FkProduct = l.FkProduct,
                FkProductVariant = l.FkProductVariant,
                Quantity = l.Quantity,
                UnitCost = l.UnitCost
            }).ToList()
        };

        var result = await _svc.UpdatePurchaseAsync(input, ct);
        if (!result.Ok)
        {
            ModelState.AddModelError(string.Empty, result.ResponseMsg);
            ViewBag.Suppliers = await _supplierSvc.GetSupplierLookupAsync(ct);
            return View(model);
        }
        TempData["SuccessMessage"] = "Purchase order updated.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ═══════════════════════════════════════════════════════════════
    // GOODS RECEIPT (GRN)
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    [RequirePermission("purchasing.manage")]
    public async Task<IActionResult> Receive(int id, CancellationToken ct)
    {
        var po = await _svc.GetPurchaseAsync(new InputPurchaseSelect { IdPurchase = id }, ct);
        if (po is null) return NotFound();

        var model = new InGoodsReceiptView
        {
            IdPurchase = po.IdPurchase,
            ReceiptDate = DateOnly.FromDateTime(DateTime.Today),
            Lines = po.Lines.Select(l => new InGrnLineView
            {
                IdPurchaseDetail = l.IdPurchaseDetail,
                FkProduct = l.FkProduct,
                FkProductVariant = l.FkProductVariant,
                ProductName = l.ProductName,
                OrderedQty = l.Quantity,
                AlreadyReceivedQty = 0,
                ReceiveQty = l.Quantity
            }).ToList()
        };
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("purchasing.manage")]
    public async Task<IActionResult> Receive(int id, InGoodsReceiptView model, CancellationToken ct)
    {
        model.IdPurchase = id;
        if (!ModelState.IsValid) return View(model);

        var input = new InputGoodsReceipt
        {
            IdPurchase = model.IdPurchase,
            FkStore = StoreId,
            ReceiptDate = model.ReceiptDate,
            Notes = model.Notes,
            CreatedBy = ActorId,
            Lines = model.Lines
                .Where(l => l.ReceiveQty > 0)
                .Select(l => new InputGrnLine
                {
                    IdPurchaseDetail = l.IdPurchaseDetail,
                    FkProduct = l.FkProduct,
                    FkProductVariant = l.FkProductVariant,
                    ReceiveQty = l.ReceiveQty
                }).ToList()
        };

        var result = await _svc.ReceiveGoodsAsync(input, ct);
        if (!result.Ok) { ModelState.AddModelError(string.Empty, result.ResponseMsg); return View(model); }
        TempData["SuccessMessage"] = $"GRN {result.GrnNumber} recorded. PO status: {result.NewPurchaseStatus}";
        return RedirectToAction(nameof(Details), new { id });
    }

    // ═══════════════════════════════════════════════════════════════
    // CANCEL
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    [RequirePermission("purchasing.manage")]
    public IActionResult Cancel(int id)
        => View(new InPurchaseCancelView { IdPurchase = id });

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("purchasing.manage")]
    public async Task<IActionResult> Cancel(int id, InPurchaseCancelView model, CancellationToken ct)
    {
        model.IdPurchase = id;
        if (!ModelState.IsValid) return View(model);

        var result = await _svc.CancelPurchaseAsync(new InputPurchaseCancel
        {
            IdPurchase = model.IdPurchase,
            Reason = model.Reason,
            CancelledBy = ActorId
        }, ct);

        if (!result.Ok) { ModelState.AddModelError(string.Empty, result.ResponseMsg); return View(model); }
        TempData["SuccessMessage"] = "Purchase order cancelled.";
        return RedirectToAction(nameof(Index));
    }
}
