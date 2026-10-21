using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

[Authorize]
[ModuleAccess("REPORTS")]
[RequirePermission("reports.view")]
public class ReportsController : Controller
{
    private readonly IReportService _svc;

    public ReportsController(IReportService svc) => _svc = svc;

    private int ActorUserId => AuthClaims.GetUserId(User) ?? 0;
    private int ActorFkStore => AuthClaims.GetFkStore(User) ?? 0;
    private bool IsSuperAdmin => AuthClaims.IsPlatformSuperAdminUser(User);

    /// <summary>
    /// Shows the reports hub with cards for each available report.
    /// Staff with reports.view open this from the Reports menu.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var input = new InputReportIndex
        {
            ActorUserId = ActorUserId,
            ActorFkStore = ActorFkStore,
            IsPlatformSuperAdmin = IsSuperAdmin
        };
        var model = await _svc.GetIndexAsync(input, ct);
        return View(model);
    }

    /// <summary>
    /// Sales report by day or month with invoices, tax, returns and payments.
    /// Used from Reports / Sales with month-to-date as the default range.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Sales([FromQuery] InReportSalesView model, CancellationToken ct)
    {
        ClearInvalidDates(model.DateFrom, model.DateTo);
        var input = new InputReportSales
        {
            ActorUserId = ActorUserId,
            ActorFkStore = ActorFkStore,
            IsPlatformSuperAdmin = IsSuperAdmin,
            DateFrom = model.DateFrom,
            DateTo = model.DateTo,
            FkStore = model.FkStore,
            GroupBy = model.GroupBy
        };
        var result = await _svc.GetSalesAsync(input, ct);
        return View(result);
    }

    /// <summary>
    /// Downloads the sales report as a UTF-8 CSV for the selected range.
    /// Used from the Sales report Print / export bar.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> SalesCsv([FromQuery] InReportSalesView model, CancellationToken ct)
    {
        ClearInvalidDates(model.DateFrom, model.DateTo);
        var input = new InputReportSales
        {
            ActorUserId = ActorUserId,
            ActorFkStore = ActorFkStore,
            IsPlatformSuperAdmin = IsSuperAdmin,
            DateFrom = model.DateFrom,
            DateTo = model.DateTo,
            FkStore = model.FkStore,
            GroupBy = model.GroupBy
        };
        var result = await _svc.GetSalesAsync(input, ct);
        return CsvFile("sales-report.csv",
            new[] { "Period", "Invoices", "Gross", "Discounts", "Tax", "Net", "Avg bill", "Returns" },
            result.Rows.Select(r => new[]
            {
                r.BucketLabel,
                Num(r.InvoiceCount),
                Num(r.Gross),
                Num(r.DiscountAmount),
                Num(r.TaxAmount),
                Num(r.Net),
                Num(r.AverageBill),
                Num(r.Returns)
            }));
    }

    /// <summary>
    /// Top-selling products by quantity or revenue for the selected range.
    /// Merchandisers open this from Reports / Products.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Products([FromQuery] InReportProductsView model, CancellationToken ct)
    {
        ClearInvalidDates(model.DateFrom, model.DateTo);
        var input = new InputReportProducts
        {
            ActorUserId = ActorUserId,
            ActorFkStore = ActorFkStore,
            IsPlatformSuperAdmin = IsSuperAdmin,
            DateFrom = model.DateFrom,
            DateTo = model.DateTo,
            FkStore = model.FkStore,
            FkCategory = model.FkCategory > 0 ? model.FkCategory : null,
            Top = model.Top,
            RankBy = model.RankBy
        };
        var result = await _svc.GetProductsAsync(input, ct);
        return View(result);
    }

    /// <summary>
    /// Downloads the product report as a UTF-8 CSV for the selected range.
    /// Used from the Products report export button.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> ProductsCsv([FromQuery] InReportProductsView model, CancellationToken ct)
    {
        ClearInvalidDates(model.DateFrom, model.DateTo);
        var input = new InputReportProducts
        {
            ActorUserId = ActorUserId,
            ActorFkStore = ActorFkStore,
            IsPlatformSuperAdmin = IsSuperAdmin,
            DateFrom = model.DateFrom,
            DateTo = model.DateTo,
            FkStore = model.FkStore,
            FkCategory = model.FkCategory > 0 ? model.FkCategory : null,
            Top = model.Top,
            RankBy = model.RankBy
        };
        var result = await _svc.GetProductsAsync(input, ct);
        return CsvFile("products-report.csv",
            new[] { "Product", "Code", "Category", "Qty sold", "Revenue", "Transactions" },
            result.Rows.Select(r => new[]
            {
                r.ProductName,
                r.ProductCode ?? "",
                r.CategoryName ?? "",
                Num(r.QuantitySold),
                Num(r.Revenue),
                Num(r.TransactionCount)
            }));
    }

    /// <summary>
    /// Current stock value (quantity on hand × product cost) with category subtotals.
    /// Inventory staff open this from the reports hub.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> StockValuation([FromQuery] InReportStockValuationView model, CancellationToken ct)
    {
        var input = new InputReportStockValuation
        {
            ActorUserId = ActorUserId,
            ActorFkStore = ActorFkStore,
            IsPlatformSuperAdmin = IsSuperAdmin,
            FkStore = model.FkStore,
            FkCategory = model.FkCategory > 0 ? model.FkCategory : null,
            IncludeZeroNegative = model.IncludeZeroNegative
        };
        var result = await _svc.GetStockValuationAsync(input, ct);
        return View(result);
    }

    /// <summary>
    /// Downloads stock valuation as a UTF-8 CSV, including zero lines when requested.
    /// Used from the Stock valuation export button.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> StockValuationCsv([FromQuery] InReportStockValuationView model, CancellationToken ct)
    {
        var input = new InputReportStockValuation
        {
            ActorUserId = ActorUserId,
            ActorFkStore = ActorFkStore,
            IsPlatformSuperAdmin = IsSuperAdmin,
            FkStore = model.FkStore,
            FkCategory = model.FkCategory > 0 ? model.FkCategory : null,
            IncludeZeroNegative = model.IncludeZeroNegative
        };
        var result = await _svc.GetStockValuationAsync(input, ct);
        return CsvFile("stock-valuation.csv",
            new[] { "Product", "Code", "Category", "Store", "On hand", "Unit cost", "Stock value" },
            result.Rows.Select(r => new[]
            {
                r.ProductName,
                r.ProductCode ?? "",
                r.CategoryName,
                r.StoreName ?? "",
                Num(r.QuantityOnHand),
                Num(r.UnitCost),
                Num(r.StockValue)
            }));
    }

    /// <summary>
    /// Tax collected by tax name and rate from sale lines joined to product tax.
    /// Finance staff open this from the reports hub.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Tax([FromQuery] InReportTaxView model, CancellationToken ct)
    {
        ClearInvalidDates(model.DateFrom, model.DateTo);
        var input = new InputReportTax
        {
            ActorUserId = ActorUserId,
            ActorFkStore = ActorFkStore,
            IsPlatformSuperAdmin = IsSuperAdmin,
            DateFrom = model.DateFrom,
            DateTo = model.DateTo,
            FkStore = model.FkStore
        };
        var result = await _svc.GetTaxAsync(input, ct);
        return View(result);
    }

    /// <summary>
    /// Downloads the tax report as a UTF-8 CSV for the selected range.
    /// Used from the Tax report export button.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> TaxCsv([FromQuery] InReportTaxView model, CancellationToken ct)
    {
        ClearInvalidDates(model.DateFrom, model.DateTo);
        var input = new InputReportTax
        {
            ActorUserId = ActorUserId,
            ActorFkStore = ActorFkStore,
            IsPlatformSuperAdmin = IsSuperAdmin,
            DateFrom = model.DateFrom,
            DateTo = model.DateTo,
            FkStore = model.FkStore
        };
        var result = await _svc.GetTaxAsync(input, ct);
        return CsvFile("tax-report.csv",
            new[] { "Tax code", "Tax name", "Rate", "Sales", "Net sales", "Tax collected" },
            result.Rows.Select(r => new[]
            {
                r.TaxCode,
                r.TaxName,
                Num(r.TaxRate),
                Num(r.SaleCount),
                Num(r.NetSales),
                Num(r.TaxCollected)
            }));
    }

    /// <summary>
    /// Purchases by supplier with ordered versus received quantities.
    /// Buyers open this from the reports hub; cancelled orders are excluded by default.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Purchases([FromQuery] InReportPurchasesView model, CancellationToken ct)
    {
        ClearInvalidDates(model.DateFrom, model.DateTo);
        var input = new InputReportPurchases
        {
            ActorUserId = ActorUserId,
            ActorFkStore = ActorFkStore,
            IsPlatformSuperAdmin = IsSuperAdmin,
            DateFrom = model.DateFrom,
            DateTo = model.DateTo,
            FkStore = model.FkStore,
            ExcludeCancelled = model.ExcludeCancelled
        };
        var result = await _svc.GetPurchasesAsync(input, ct);
        return View(result);
    }

    /// <summary>
    /// Downloads the purchase report as a UTF-8 CSV for the selected range.
    /// Used from the Purchases report export button.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> PurchasesCsv([FromQuery] InReportPurchasesView model, CancellationToken ct)
    {
        ClearInvalidDates(model.DateFrom, model.DateTo);
        var input = new InputReportPurchases
        {
            ActorUserId = ActorUserId,
            ActorFkStore = ActorFkStore,
            IsPlatformSuperAdmin = IsSuperAdmin,
            DateFrom = model.DateFrom,
            DateTo = model.DateTo,
            FkStore = model.FkStore,
            ExcludeCancelled = model.ExcludeCancelled
        };
        var result = await _svc.GetPurchasesAsync(input, ct);
        return CsvFile("purchases-report.csv",
            new[] { "Supplier", "Orders", "Ordered qty", "Received qty", "Ordered amount" },
            result.Rows.Select(r => new[]
            {
                r.SupplierName,
                Num(r.PurchaseCount),
                Num(r.OrderedQty),
                Num(r.ReceivedQty),
                Num(r.OrderedAmount)
            }));
    }

    /// <summary>
    /// Top customers by spend with visits, average ticket and last purchase.
    /// Walk-in sales appear as a single row. Used from Reports / Customers.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Customers([FromQuery] InReportCustomersView model, CancellationToken ct)
    {
        ClearInvalidDates(model.DateFrom, model.DateTo);
        var input = new InputReportCustomers
        {
            ActorUserId = ActorUserId,
            ActorFkStore = ActorFkStore,
            IsPlatformSuperAdmin = IsSuperAdmin,
            DateFrom = model.DateFrom,
            DateTo = model.DateTo,
            FkStore = model.FkStore,
            Top = model.Top
        };
        var result = await _svc.GetCustomersAsync(input, ct);
        return View(result);
    }

    /// <summary>
    /// Downloads the customer report as a UTF-8 CSV for the selected range.
    /// Used from the Customers report export button.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> CustomersCsv([FromQuery] InReportCustomersView model, CancellationToken ct)
    {
        ClearInvalidDates(model.DateFrom, model.DateTo);
        var input = new InputReportCustomers
        {
            ActorUserId = ActorUserId,
            ActorFkStore = ActorFkStore,
            IsPlatformSuperAdmin = IsSuperAdmin,
            DateFrom = model.DateFrom,
            DateTo = model.DateTo,
            FkStore = model.FkStore,
            Top = model.Top
        };
        var result = await _svc.GetCustomersAsync(input, ct);
        return CsvFile("customers-report.csv",
            new[] { "Customer", "Visits", "Total spend", "Average", "Last purchase" },
            result.Rows.Select(r => new[]
            {
                r.CustomerName,
                Num(r.VisitCount),
                Num(r.TotalSpend),
                Num(r.AverageSpend),
                r.LastPurchaseAt.HasValue
                    ? r.LastPurchaseAt.Value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
                    : ""
            }));
    }

    private void ClearInvalidDates(DateTime? from, DateTime? to)
    {
        if (from.HasValue && to.HasValue && to.Value < from.Value)
            ModelState.Clear();
    }

    private FileContentResult CsvFile(string fileName, IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<string>> rows)
    {
        var sb = new StringBuilder();
        sb.Append(string.Join(",", headers.Select(CsvCell)));
        sb.Append("\r\n");
        foreach (var row in rows)
        {
            sb.Append(string.Join(",", row.Select(CsvCell)));
            sb.Append("\r\n");
        }

        var utf8Bom = Encoding.UTF8.GetPreamble();
        var body = Encoding.UTF8.GetBytes(sb.ToString());
        var bytes = new byte[utf8Bom.Length + body.Length];
        Buffer.BlockCopy(utf8Bom, 0, bytes, 0, utf8Bom.Length);
        Buffer.BlockCopy(body, 0, bytes, utf8Bom.Length, body.Length);
        return File(bytes, "text/csv; charset=utf-8", fileName);
    }

    private static string CsvCell(string? value)
    {
        var s = value ?? string.Empty;
        if (s.Length > 0 && (s[0] is '=' or '+' or '-' or '@'))
            s = "'" + s;
        if (s.Contains('"') || s.Contains(',') || s.Contains('\n') || s.Contains('\r'))
            s = "\"" + s.Replace("\"", "\"\"") + "\"";
        return s;
    }

    private static string Num(decimal value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    private static string Num(int value) => value.ToString(CultureInfo.InvariantCulture);
}
