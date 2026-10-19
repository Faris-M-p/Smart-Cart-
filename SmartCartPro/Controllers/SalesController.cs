using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>POS & billing: create sale, invoices, returns, cash sessions, day-close.</summary>
[Authorize]
[ModuleAccess("POS")]
public class SalesController : Controller
{
    private readonly ISaleService _svc;
    private int ActorId => AuthClaims.GetUserId(User) ?? 0;
    private int StoreId => AuthClaims.GetFkStore(User) ?? 0;

    public SalesController(ISaleService svc) => _svc = svc;

    // ═══════════════════════════════════════════════════════════════
    // POS
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    [RequirePermission("pos.view")]
    public IActionResult Pos() => View();

    /// <summary>Create a sale from POS (JSON post).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("pos.view")]
    public async Task<IActionResult> CreateSale([FromBody] InSaleCreateView model, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var input = new InputSaleCreate
        {
            FkStore = model.FkStore > 0 ? model.FkStore : StoreId,
            FkCustomer = model.FkCustomer,
            FkCashSession = model.FkCashSession,
            Notes = model.Notes,
            ClientRef = model.ClientRef,
            CreatedBy = ActorId,
            Lines = model.Lines.Select(l => new InputSaleLine
            {
                FkProduct = l.FkProduct,
                FkProductVariant = l.FkProductVariant,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                DiscountAmount = l.DiscountAmount
            }).ToList(),
            Payments = model.Payments.Select(p => new InputPayment
            {
                PaymentMethod = p.PaymentMethod,
                Amount = p.Amount,
                ReferenceNo = p.ReferenceNo
            }).ToList()
        };

        var result = await _svc.CreateSaleAsync(input, ct);
        if (!result.Ok)
            return StatusCode(result.StatusCode, new { result.ResponseMsg });

        return Ok(result);
    }

    // ═══════════════════════════════════════════════════════════════
    // INVOICES
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    [RequirePermission("pos.view")]
    public async Task<IActionResult> Invoices([FromQuery] InputSaleList input, CancellationToken ct)
    {
        if (input.FkStore is null or 0) input.FkStore = StoreId;
        input.Page = Math.Max(1, input.Page);
        return View(await _svc.ListSalesAsync(input, ct));
    }

    [HttpGet]
    [RequirePermission("pos.view")]
    public async Task<IActionResult> InvoiceDetails(int id, CancellationToken ct)
    {
        var item = await _svc.GetSaleAsync(new InputSaleSelect { IdSale = id }, ct);
        if (item is null) return NotFound();
        return View(item);
    }

    /// <summary>Invoice detail JSON for print/preview.</summary>
    [HttpGet]
    public async Task<IActionResult> InvoiceJson(int id, CancellationToken ct)
    {
        var item = await _svc.GetSaleAsync(new InputSaleSelect { IdSale = id }, ct);
        if (item is null) return NotFound();
        return Json(item);
    }

    // ═══════════════════════════════════════════════════════════════
    // CANCEL SALE
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    [RequirePermission("pos.cancel")]
    public IActionResult CancelSale(int id)
        => View(new InSaleCancelView { IdSale = id });

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("pos.cancel")]
    public async Task<IActionResult> CancelSale(int id, InSaleCancelView model, CancellationToken ct)
    {
        model.IdSale = id;
        if (!ModelState.IsValid) return View(model);

        var result = await _svc.CancelSaleAsync(new InputSaleCancel
        {
            IdSale = model.IdSale,
            FkStore = StoreId,
            Reason = model.Reason,
            CancelledBy = ActorId
        }, ct);

        if (!result.Ok) { ModelState.AddModelError(string.Empty, result.ResponseMsg); return View(model); }
        TempData["SuccessMessage"] = "Sale cancelled and stock reversed.";
        return RedirectToAction(nameof(Invoices));
    }

    // ═══════════════════════════════════════════════════════════════
    // RETURNS
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    [RequirePermission("pos.return")]
    public async Task<IActionResult> Returns(int id, CancellationToken ct)
    {
        var sale = await _svc.GetSaleAsync(new InputSaleSelect { IdSale = id }, ct);
        if (sale is null) return NotFound();

        var model = new InSaleReturnView
        {
            IdSale = sale.IdSale,
            InvoiceNumber = sale.InvoiceNumber,
            Lines = sale.Lines.Select(l => new InReturnLineView
            {
                IdSaleDetail = l.IdSaleDetail,
                FkProduct = l.FkProduct,
                FkProductVariant = l.FkProductVariant,
                ProductName = l.ProductName,
                MaxQty = l.Quantity,
                ReturnQty = 0
            }).ToList()
        };
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("pos.return")]
    public async Task<IActionResult> Returns(int id, InSaleReturnView model, CancellationToken ct)
    {
        model.IdSale = id;
        if (!ModelState.IsValid) return View(model);

        var input = new InputSaleReturn
        {
            IdSale = model.IdSale,
            FkStore = StoreId,
            Reason = model.Reason,
            CreatedBy = ActorId,
            Lines = model.Lines
                .Where(l => l.ReturnQty > 0)
                .Select(l => new InputReturnLine
                {
                    FkProduct = l.FkProduct,
                    FkProductVariant = l.FkProductVariant,
                    ReturnQty = l.ReturnQty,
                    UnitPrice = 0 // resolved inside repository from original sale
                }).ToList()
        };

        var result = await _svc.ReturnSaleAsync(input, ct);
        if (!result.Ok) { ModelState.AddModelError(string.Empty, result.ResponseMsg); return View(model); }
        TempData["SuccessMessage"] = $"Return {result.ReturnNumber} processed. Amount: {result.TotalAmount:C}";
        return RedirectToAction(nameof(Invoices));
    }

    // ═══════════════════════════════════════════════════════════════
    // CASH SESSION
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    [RequirePermission("pos.view")]
    public async Task<IActionResult> Session(CancellationToken ct)
    {
        var session = await _svc.GetOpenSessionAsync(new InputCashSessionSelect { FkStore = StoreId }, ct);
        ViewBag.OpenSession = session;
        return View(new InCashSessionOpenView { FkStore = StoreId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("pos.view")]
    public async Task<IActionResult> OpenSession(InCashSessionOpenView model, CancellationToken ct)
    {
        if (!ModelState.IsValid) { ViewBag.OpenSession = null; return View("Session", model); }
        var result = await _svc.OpenSessionAsync(new InputCashSessionOpen
        {
            FkStore = model.FkStore > 0 ? model.FkStore : StoreId,
            FkUser = ActorId,
            OpeningFloat = model.OpeningFloat,
            Notes = model.Notes
        }, ct);
        if (!result.Ok) { ModelState.AddModelError(string.Empty, result.ResponseMsg); ViewBag.OpenSession = null; return View("Session", model); }
        TempData["SuccessMessage"] = "Cash session opened.";
        return RedirectToAction(nameof(Session));
    }

    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("pos.view")]
    public async Task<IActionResult> CloseSession(InCashSessionCloseView model, CancellationToken ct)
    {
        if (!ModelState.IsValid) return RedirectToAction(nameof(Session));
        var result = await _svc.CloseSessionAsync(new InputCashSessionClose
        {
            IdCashSession = model.IdCashSession,
            FkStore = StoreId,
            ClosingCash = model.ClosingCash,
            Notes = model.Notes
        }, ct);
        if (!result.Ok) { TempData["ErrorMessage"] = result.ResponseMsg; return RedirectToAction(nameof(Session)); }
        TempData["SuccessMessage"] = $"Session closed. Variance: {result.Variance:C}";
        return RedirectToAction(nameof(DayClose), new { id = model.IdCashSession });
    }

    // ═══════════════════════════════════════════════════════════════
    // DAY CLOSE / CASH SUMMARY
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    [RequirePermission("pos.view")]
    public async Task<IActionResult> DayClose(int id, CancellationToken ct)
        => View(await _svc.GetCashSummaryAsync(new InputCashSummary { FkStore = StoreId, IdCashSession = id }, ct));

    /// <summary>JSON endpoint for cash summary (print / export).</summary>
    [HttpGet]
    public async Task<IActionResult> CashSummaryJson(int id, CancellationToken ct)
        => Json(await _svc.GetCashSummaryAsync(new InputCashSummary { FkStore = StoreId, IdCashSession = id }, ct));

    // ═══════════════════════════════════════════════════════════════
    // ONLINE ORDERS (stub — separate phase)
    // ═══════════════════════════════════════════════════════════════

    [HttpGet]
    public IActionResult OnlineOrders() => View();
}
