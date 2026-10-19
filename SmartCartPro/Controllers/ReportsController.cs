using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>
/// Reports controller — Phase 7 expansion.
/// All data actions return JSON for front-end charts / export.
/// ModuleAccess: REPORTS | Permission: reports.view
/// </summary>
[Authorize]
[ModuleAccess("REPORTS")]
[RequirePermission("reports.view")]
public class ReportsController : Controller
{
    private readonly IReportService _svc;
    private int StoreId => AuthClaims.GetFkStore(User) ?? 0;

    public ReportsController(IReportService svc) => _svc = svc;

    // ── Shell views (no data — loaded via AJAX) ──────────────────

    /// <summary>
    /// Shell view for sales reports and charts.
    /// Data is loaded client-side via JSON actions such as SalesByDate.
    /// </summary>
    public IActionResult Sales() => View();

    /// <summary>
    /// Shell view for product and inventory report pages.
    /// Data is loaded via AJAX from StockValuation, LowStock, and TopSelling.
    /// </summary>
    public IActionResult Products() => View();

    /// <summary>
    /// Shell view for customer-focused report pages.
    /// Used when customer analytics UI is expanded in the reports module.
    /// </summary>
    public IActionResult Customers() => View();

    // ── JSON data actions ────────────────────────────────────────

    /// <summary>Daily sales totals for a date range.</summary>
    [HttpGet]
    public async Task<IActionResult> SalesByDate([FromQuery] InputReportSalesByDate input, CancellationToken ct)
    {
        if (input.FkStore is null or 0 && StoreId > 0) input.FkStore = StoreId;
        return Json(await _svc.GetSalesByDateAsync(input, ct));
    }

    /// <summary>Current inventory value (qty × cost price) per product per store.</summary>
    [HttpGet]
    public async Task<IActionResult> StockValuation([FromQuery] InputReportStockValuation input, CancellationToken ct)
    {
        if (input.FkStore is null or 0 && StoreId > 0) input.FkStore = StoreId;
        return Json(await _svc.GetStockValuationAsync(input, ct));
    }

    /// <summary>Products whose quantity on hand is at or below reorder level.</summary>
    [HttpGet]
    public async Task<IActionResult> LowStock([FromQuery] InputReportLowStock input, CancellationToken ct)
    {
        if (input.FkStore is null or 0 && StoreId > 0) input.FkStore = StoreId;
        input.Page     = Math.Max(1, input.Page);
        input.PageSize = input.PageSize is <= 0 or > 500 ? 100 : input.PageSize;
        return Json(await _svc.GetLowStockReportAsync(input, ct));
    }

    /// <summary>Tax collected grouped by tax type for a date range.</summary>
    [HttpGet]
    public async Task<IActionResult> TaxSummary([FromQuery] InputReportTaxSummary input, CancellationToken ct)
    {
        if (input.FkStore is null or 0 && StoreId > 0) input.FkStore = StoreId;
        return Json(await _svc.GetTaxSummaryAsync(input, ct));
    }

    /// <summary>Top N products by quantity sold in a date range.</summary>
    [HttpGet]
    public async Task<IActionResult> TopSelling([FromQuery] InputReportTopSelling input, CancellationToken ct)
    {
        if (input.FkStore is null or 0 && StoreId > 0) input.FkStore = StoreId;
        input.Top = input.Top is <= 0 or > 100 ? 20 : input.Top;
        return Json(await _svc.GetTopSellingAsync(input, ct));
    }
}
