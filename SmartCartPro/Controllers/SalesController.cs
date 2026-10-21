using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Exceptions;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>POS billing, cash sessions, invoices and returns for the signed-in store.</summary>
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

    /// <summary>
    /// Two-column cashier screen: product search on the left and the current bill on the right.
    /// Cashiers with pos.sell use this to ring up in-store sales after a cash session is open.
    /// </summary>
    [HttpGet]
    [RequirePermission("pos.sell")]
    public async Task<IActionResult> Pos(CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("POS");

        var input = new InputPosSelect { FkStore = StoreId };
        return View(await _svc.GetPosAsync(input, ct));
    }

    /// <summary>
    /// Completes a POS sale: prices and tax are taken from master data, stock is deducted, and the open session is attached.
    /// The POS page posts the bill here as JSON after the cashier presses Complete (F9).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequirePermission("pos.sell")]
    public async Task<IActionResult> CreateSale([FromBody] InSaleCreateView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputSaleCreate
        {
            FkStore = StoreId,
            FkCustomer = model.FkCustomer,
            Notes = model.Notes,
            ClientRef = model.ClientRef,
            CreatedBy = ActorId,
            Lines = model.Lines.Select(l => new InputSaleLine
            {
                FkProduct = l.FkProduct,
                FkProductVariant = l.FkProductVariant,
                Quantity = l.Quantity,
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

        return CommonHelpers.SuccessResponse(
            "Sale completed.",
            new
            {
                id = result.IdSale,
                invoiceNumber = result.InvoiceNumber,
                totalAmount = result.TotalAmount,
                paidAmount = result.PaidAmount,
                changeGiven = result.ChangeGiven,
                printUrl = Url.Action(nameof(InvoicePrint), new { id = result.IdSale, autoprint = 1 })
            });
    }

    /// <summary>
    /// Live POS product search by name, SKU or barcode. Cashiers may lack products.view, so this is gated by pos.sell.
    /// The POS search box calls this while the cashier types.
    /// </summary>
    [HttpGet]
    [RequirePermission("pos.sell")]
    public async Task<IActionResult> ProductSearch(string? term, int take = 20, CancellationToken ct = default)
    {
        var input = new InputSaleProductSearch { FkStore = StoreId, Term = term, Take = take };
        var items = await _svc.SearchProductsAsync(input, ct);
        return CommonHelpers.SuccessResponse("Products loaded.", items);
    }

    /// <summary>
    /// Finds one sellable SKU by exact barcode for the scanner / Enter key on the POS search box.
    /// Used when a cashier scans or types a barcode and presses Enter.
    /// </summary>
    [HttpGet]
    [RequirePermission("pos.sell")]
    public async Task<IActionResult> ProductByBarcode(string? barcode, CancellationToken ct = default)
    {
        var input = new InputSaleProductBarcode { FkStore = StoreId, Barcode = barcode ?? string.Empty };
        var item = await _svc.GetProductByBarcodeAsync(input, ct);
        return CommonHelpers.SuccessResponse("Product loaded.", item);
    }

    // ═══════════════════════════════════════════════════════════════
    // INVOICES
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Paged invoice list for the current store with search, status, dates, payment method and customer filters.
    /// Cashiers and report viewers open this from Sales / Invoices.
    /// </summary>
    [HttpGet]
    [RequirePermission("pos.sell", "reports.view")]
    public async Task<IActionResult> Invoices([FromQuery] InputSaleList input, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Invoices");

        input.FkStore = StoreId;
        return View(await _svc.ListSalesAsync(input, ct));
    }

    /// <summary>
    /// Full invoice with lines, payments and cancel / print / return actions.
    /// Opened from the invoice list, the POS success dialog, or a customer history link.
    /// </summary>
    [HttpGet]
    [RequirePermission("pos.sell", "reports.view")]
    public async Task<IActionResult> InvoiceDetails(int id, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Invoice");

        var select = new InputSaleSelect { IdSale = id, FkStore = StoreId };
        var item = await _svc.GetSaleAsync(select, ct);
        if (item is null) return NotFound();
        return View(item);
    }

    /// <summary>
    /// Standalone receipt for an 80 mm thermal printer (default) or A4. Optional autoprint opens the browser print dialog.
    /// Opened from invoice details or the POS success dialog.
    /// </summary>
    [HttpGet]
    [RequirePermission("pos.sell", "reports.view")]
    public async Task<IActionResult> InvoicePrint(int id, string? paper, string? autoprint, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Invoice");

        var select = new InputSaleSelect { IdSale = id, FkStore = StoreId };
        var item = await _svc.GetSaleAsync(select, ct);
        if (item is null) return NotFound();

        ViewBag.Paper = string.Equals(paper, "a4", StringComparison.OrdinalIgnoreCase) ? "a4" : "80mm";
        ViewBag.AutoPrint = autoprint == "1";
        return View(item);
    }

    /// <summary>
    /// Invoice payload for reprint or a till display that already has the id.
    /// The POS success dialog and invoice page script read this as JSON.
    /// </summary>
    [HttpGet]
    [RequirePermission("pos.sell", "reports.view")]
    public async Task<IActionResult> InvoiceJson(int id, CancellationToken ct)
    {
        var select = new InputSaleSelect { IdSale = id, FkStore = StoreId };
        var item = await _svc.GetSaleAsync(select, ct);
        if (item is null)
            throw new SmartCartPro.Exceptions.ResourceNotFoundException(
                "Invoice not found.", $"Sale {id} was not found in store {StoreId}.");

        return CommonHelpers.SuccessResponse("Invoice loaded.", item);
    }

    // ═══════════════════════════════════════════════════════════════
    // CANCEL SALE
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Voids a completed sale of this store, records the reason and puts the stock back.
    /// The invoice details modal posts { id, reason } here; sales with returns are refused.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("pos.cancel")]
    public async Task<IActionResult> CancelSale(int id, InSaleCancelView model, CancellationToken ct)
    {
        if (id > 0) model.IdSale = id;
        ModelState.Remove(nameof(InSaleCancelView.IdSale));
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputSaleCancel
        {
            IdSale = model.IdSale,
            FkStore = StoreId,
            Reason = model.Reason,
            CancelledBy = ActorId
        };
        await _svc.CancelSaleAsync(input, ct);

        return CommonHelpers.SuccessResponse(
            "Sale cancelled and stock reversed.",
            new { id, redirectUrl = Url.Action(nameof(InvoiceDetails), new { id }) });
    }

    // ═══════════════════════════════════════════════════════════════
    // RETURNS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Paged store returns list, or redirects Sales/Returns/{saleId} to the return form.
    /// Cashiers with pos.return open this from Sales / Returns; old invoice links still resolve.
    /// </summary>
    [HttpGet]
    [RequirePermission("pos.return")]
    public async Task<IActionResult> Returns(int? id, [FromQuery] InputSalesReturnList input, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Returns");
        if (id is > 0) return RedirectToAction(nameof(Return), new { id });

        input.FkStore = StoreId;
        return View(await _svc.ListSalesReturnsAsync(input, ct));
    }

    /// <summary>
    /// Posts a sale return posted to the legacy Sales/Returns/{id} URL.
    /// Kept so older forms still restock; the current form posts to Return.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("pos.return")]
    public Task<IActionResult> Returns(int id, InSaleReturnView model, CancellationToken ct)
        => CompleteReturnAsync(id, model, ct);

    /// <summary>
    /// Return entry against one completed invoice of this store, with remaining quantities.
    /// Opened from invoice details or a redirect of Sales/Returns/{id}.
    /// </summary>
    [HttpGet]
    [RequirePermission("pos.return")]
    public async Task<IActionResult> Return(int id, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Return");

        var select = new InputSaleReturnableSelect { IdSale = id, FkStore = StoreId };
        var sale = await _svc.GetReturnableSaleAsync(select, ct);

        var model = new InSaleReturnView
        {
            IdSale = sale.IdSale,
            InvoiceNumber = sale.InvoiceNumber,
            CustomerName = sale.CustomerName,
            SaleDate = sale.SaleDate,
            CashierName = sale.CashierName,
            Lines = sale.Lines.Select(l => new InReturnLineView
            {
                IdSaleDetail = l.IdSaleDetail,
                FkProduct = l.FkProduct,
                FkProductVariant = l.FkProductVariant,
                ProductName = l.ProductName,
                VariantName = l.VariantName,
                Sku = l.Sku,
                OriginalQty = l.OriginalQty,
                AlreadyReturnedQty = l.ReturnedQty,
                RemainingQty = l.RemainingQty,
                MaxQty = l.RemainingQty,
                UnitPrice = l.UnitPrice,
                ReturnQty = 0
            }).ToList()
        };
        return View(model);
    }

    /// <summary>
    /// Posts a sale return and restores stock for the returned quantities.
    /// The return form submits here after the cashier enters quantities and a reason.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("pos.return")]
    public Task<IActionResult> Return(int id, InSaleReturnView model, CancellationToken ct)
        => CompleteReturnAsync(id, model, ct);

    /// <summary>
    /// Read-only return document: header, lines and a link back to the original invoice.
    /// Opened from the returns list or after a successful return.
    /// </summary>
    [HttpGet]
    [RequirePermission("pos.return")]
    public async Task<IActionResult> ReturnDetails(int id, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Return");

        var select = new InputSalesReturnSelect { IdSalesReturn = id, FkStore = StoreId };
        var item = await _svc.GetSalesReturnAsync(select, ct);
        if (item is null)
            throw new ResourceNotFoundException(
                "Return not found.",
                $"Sales return {id} was not found in store {StoreId}.");

        return View(item);
    }

    // ═══════════════════════════════════════════════════════════════
    // CASH SESSION
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Lists cash drawer sessions for the store and shows the open-session / close form.
    /// Cashiers open this at shift start and end.
    /// </summary>
    [HttpGet]
    [RequirePermission("pos.sell")]
    public async Task<IActionResult> Session([FromQuery] InputCashSessionList input, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Cash session");

        input.FkStore = StoreId;
        return View(await _svc.ListSessionsAsync(input, ct));
    }

    /// <summary>
    /// Opens the store's cash session with an opening float. Only one session may be open.
    /// Used from the POS empty-session card and the Session page.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("pos.sell")]
    public async Task<IActionResult> OpenSession(InCashSessionOpenView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputCashSessionOpen
        {
            FkStore = StoreId,
            FkUser = ActorId,
            OpeningFloat = model.OpeningFloat,
            Notes = model.Notes
        };
        var result = await _svc.OpenSessionAsync(input, ct);

        return CommonHelpers.SuccessResponse(
            "Cash session opened.",
            new { id = result.IdCashSession, redirectUrl = Url.Action(nameof(Pos)) });
    }

    /// <summary>
    /// Closes the open session, stores counted cash and the variance (counted minus expected).
    /// Used at shift end from the Session page.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("pos.sell")]
    public async Task<IActionResult> CloseSession(InCashSessionCloseView model, CancellationToken ct)
    {
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputCashSessionClose
        {
            IdCashSession = model.IdCashSession,
            FkStore = StoreId,
            ClosingCash = model.ClosingCash,
            Notes = model.Notes
        };
        var result = await _svc.CloseSessionAsync(input, ct);

        return CommonHelpers.SuccessResponse(
            $"Session closed. Variance: {result.Variance:N2}",
            new
            {
                id = model.IdCashSession,
                variance = result.Variance,
                expectedCash = result.ExpectedCash,
                redirectUrl = Url.Action(nameof(DayClose), new { id = model.IdCashSession })
            });
    }

    // ═══════════════════════════════════════════════════════════════
    // DAY CLOSE / CASH SUMMARY
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// End-of-day (or mid-shift) till summary: sale counts, payment totals, returns and expected vs counted cash.
    /// Managers print this after a session is closed.
    /// </summary>
    [HttpGet]
    [RequirePermission("pos.sell")]
    public async Task<IActionResult> DayClose(int id, CancellationToken ct)
    {
        if (StoreId <= 0) return StoreRequiredView("Day close");

        var input = new InputCashSummary { FkStore = StoreId, IdCashSession = id };
        return View(await _svc.GetCashSummaryAsync(input, ct));
    }

    /// <summary>
    /// JSON copy of the day-close summary for a print preview or export.
    /// The session page and print script call this when they need the numbers without a full page.
    /// </summary>
    [HttpGet]
    [RequirePermission("pos.sell")]
    public async Task<IActionResult> CashSummaryJson(int id, CancellationToken ct)
    {
        var input = new InputCashSummary { FkStore = StoreId, IdCashSession = id };
        var summary = await _svc.GetCashSummaryAsync(input, ct);
        return CommonHelpers.SuccessResponse("Cash summary loaded.", summary);
    }

    // ═══════════════════════════════════════════════════════════════
    // helpers
    // ═══════════════════════════════════════════════════════════════

    private IActionResult StoreRequiredView(string title)
    {
        ViewData["Title"] = title;
        return View("StoreRequired");
    }

    private async Task<IActionResult> CompleteReturnAsync(int id, InSaleReturnView model, CancellationToken ct)
    {
        model.IdSale = id;
        ModelState.Remove(nameof(InSaleReturnView.IdSale));
        CommonHelpers.ValidateModelState(ModelState);

        var input = new InputSaleReturn
        {
            IdSale = model.IdSale,
            FkStore = StoreId,
            Reason = model.Reason,
            CreatedBy = ActorId,
            Lines = model.Lines.Select(l => new InputReturnLine
            {
                IdSaleDetail = l.IdSaleDetail,
                FkProduct = l.FkProduct,
                FkProductVariant = l.FkProductVariant is > 0 ? l.FkProductVariant : null,
                ReturnQty = l.ReturnQty,
                UnitPrice = 0
            }).ToList()
        };
        var result = await _svc.ReturnSaleAsync(input, ct);

        return CommonHelpers.SuccessResponse(
            $"Return {result.ReturnNumber} processed.",
            new
            {
                id = result.IdSalesReturn,
                returnNumber = result.ReturnNumber,
                totalAmount = result.TotalAmount,
                redirectUrl = Url.Action(nameof(ReturnDetails), new { id = result.IdSalesReturn })
            });
    }
}
