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
    private readonly ICurrentUserAccess _access;

    public ReportsController(IReportService svc, ICurrentUserAccess access)
    {
        _svc = svc;
        _access = access;
    }

    private int ActorUserId => AuthClaims.GetUserId(User) ?? 0;
    private int ActorFkStore => AuthClaims.GetFkStore(User) ?? 0;
    private bool IsSuperAdmin => AuthClaims.IsPlatformSuperAdminUser(User);

    #region Hub & Legacy Reports

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
            IsPlatformSuperAdmin = IsSuperAdmin,
            CanSeeProfit = IsSuperAdmin || await _access.CanAsync("reports.profit", ct)
        };
        var model = await _svc.GetIndexAsync(input, ct);
        var kept = new List<OutputReportIndexCard>();
        foreach (var card in model.Cards)
        {
            var code = ReportFeatureMap.ForAction(card.Action);
            if (code is null || await _access.FeatureEnabledAsync(code, ct))
                kept.Add(card);
        }

        model.Cards = kept;
        return View(model);
    }

    /// <summary>
    /// Sales report by day or month with invoices, tax, returns and payments.
    /// Used from Reports / Sales with month-to-date as the default range.
    /// </summary>
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.BILL_WISE")]
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
    [FeatureAccess("REPORTS.SALES.BILL_WISE")]
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
    [FeatureAccess("REPORTS.SALES.ITEM")]
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
    [FeatureAccess("REPORTS.SALES.ITEM")]
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
    [FeatureAccess("REPORTS.STOCK.VALUATION")]
    public async Task<IActionResult> StockValuation([FromQuery] InReportStockValuationView model, CancellationToken ct)
    {
        var input = new InputReportStockValuation
        {
            ActorUserId = ActorUserId,
            ActorFkStore = ActorFkStore,
            IsPlatformSuperAdmin = IsSuperAdmin,
            FkStore = model.FkStore,
            FkCategory = model.FkCategory > 0 ? model.FkCategory : null,
            IncludeZeroNegative = model.IncludeZeroNegative,
            CanSeeProfit = IsSuperAdmin || await _access.CanAsync("reports.profit", ct)
        };
        var result = await _svc.GetStockValuationAsync(input, ct);
        return View(result);
    }

    /// <summary>
    /// Downloads stock valuation as a UTF-8 CSV, including zero lines when requested.
    /// Used from the Stock valuation export button.
    /// </summary>
    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.VALUATION")]
    public async Task<IActionResult> StockValuationCsv([FromQuery] InReportStockValuationView model, CancellationToken ct)
    {
        var input = new InputReportStockValuation
        {
            ActorUserId = ActorUserId,
            ActorFkStore = ActorFkStore,
            IsPlatformSuperAdmin = IsSuperAdmin,
            FkStore = model.FkStore,
            FkCategory = model.FkCategory > 0 ? model.FkCategory : null,
            IncludeZeroNegative = model.IncludeZeroNegative,
            CanSeeProfit = IsSuperAdmin || await _access.CanAsync("reports.profit", ct)
        };
        var result = await _svc.GetStockValuationAsync(input, ct);
        var headers = result.ShowCost
            ? new[] { "Product", "Code", "Category", "Store", "On hand", "Unit cost", "Stock value" }
            : new[] { "Product", "Code", "Category", "Store", "On hand" };
        return CsvFile("stock-valuation.csv",
            headers,
            result.Rows.Select(r => result.ShowCost
                ? new[]
                {
                    r.ProductName,
                    r.ProductCode ?? "",
                    r.CategoryName,
                    r.StoreName ?? "",
                    Num(r.QuantityOnHand),
                    Num(r.UnitCost),
                    Num(r.StockValue)
                }
                : new[]
                {
                    r.ProductName,
                    r.ProductCode ?? "",
                    r.CategoryName,
                    r.StoreName ?? "",
                    Num(r.QuantityOnHand)
                }));
    }

    /// <summary>
    /// Tax collected by tax name and rate from sale lines joined to product tax.
    /// Finance staff open this from the reports hub.
    /// </summary>
    [HttpGet]
    [FeatureAccess("REPORTS.TAX.GST")]
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
    [FeatureAccess("REPORTS.TAX.GST")]
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
    [FeatureAccess("REPORTS.PURCHASE.BILLS")]
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
    [FeatureAccess("REPORTS.PURCHASE.BILLS")]
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
    [FeatureAccess("REPORTS.PARTIES.CUSTOMERS")]
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
    [FeatureAccess("REPORTS.PARTIES.CUSTOMERS")]
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

    #endregion

    #region Report Suite Actions

    [HttpGet]
    [FeatureAccess("REPORTS.SALES.BILL_WISE")]
    public Task<IActionResult> SalesBillWise([FromQuery] InSalesBillWiseView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.BILL_WISE")]
    public Task<IActionResult> SalesBillWiseData([FromQuery] InSalesBillWiseView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.BILL_WISE")]
    public Task<IActionResult> SalesBillWiseExport([FromQuery] InSalesBillWiseView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.BILL_WISE")]
    public Task<IActionResult> SalesBillWisePrint([FromQuery] InSalesBillWiseView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    [FeatureAccess("REPORTS.SALES.ITEM")]
    public Task<IActionResult> ItemSales([FromQuery] InItemSalesView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.ITEM")]
    public Task<IActionResult> ItemSalesData([FromQuery] InItemSalesView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.ITEM")]
    public Task<IActionResult> ItemSalesExport([FromQuery] InItemSalesView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.ITEM")]
    public Task<IActionResult> ItemSalesPrint([FromQuery] InItemSalesView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    [FeatureAccess("REPORTS.SALES.CATEGORY")]
    public Task<IActionResult> CategorySales([FromQuery] InCategorySalesView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.CATEGORY")]
    public Task<IActionResult> CategorySalesData([FromQuery] InCategorySalesView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.CATEGORY")]
    public Task<IActionResult> CategorySalesExport([FromQuery] InCategorySalesView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.CATEGORY")]
    public Task<IActionResult> CategorySalesPrint([FromQuery] InCategorySalesView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    [FeatureAccess("REPORTS.SALES.RETURNS")]
    public Task<IActionResult> SalesReturns([FromQuery] InSalesReturnReportView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.RETURNS")]
    public Task<IActionResult> SalesReturnsData([FromQuery] InSalesReturnReportView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.RETURNS")]
    public Task<IActionResult> SalesReturnsExport([FromQuery] InSalesReturnReportView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.RETURNS")]
    public Task<IActionResult> SalesReturnsPrint([FromQuery] InSalesReturnReportView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    [FeatureAccess("REPORTS.SALES.DAY_CLOSE")]
    public Task<IActionResult> DayClose([FromQuery] InDayCloseView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.DAY_CLOSE")]
    public Task<IActionResult> DayCloseData([FromQuery] InDayCloseView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.DAY_CLOSE")]
    public Task<IActionResult> DayCloseExport([FromQuery] InDayCloseView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.DAY_CLOSE")]
    public Task<IActionResult> DayClosePrint([FromQuery] InDayCloseView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.VALUATION")]
    public Task<IActionResult> CurrentStock([FromQuery] InCurrentStockView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.VALUATION")]
    public Task<IActionResult> CurrentStockData([FromQuery] InCurrentStockView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.VALUATION")]
    public Task<IActionResult> CurrentStockExport([FromQuery] InCurrentStockView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.VALUATION")]
    public Task<IActionResult> CurrentStockPrint([FromQuery] InCurrentStockView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.ITEM_LEDGER")]
    public Task<IActionResult> ItemLedger([FromQuery] InItemLedgerView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.ITEM_LEDGER")]
    public Task<IActionResult> ItemLedgerData([FromQuery] InItemLedgerView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.ITEM_LEDGER")]
    public Task<IActionResult> ItemLedgerExport([FromQuery] InItemLedgerView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.ITEM_LEDGER")]
    public Task<IActionResult> ItemLedgerPrint([FromQuery] InItemLedgerView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.STOCK_LEDGER")]
    public Task<IActionResult> StockLedger([FromQuery] InStockLedgerView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.STOCK_LEDGER")]
    public Task<IActionResult> StockLedgerData([FromQuery] InStockLedgerView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.STOCK_LEDGER")]
    public Task<IActionResult> StockLedgerExport([FromQuery] InStockLedgerView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.STOCK_LEDGER")]
    public Task<IActionResult> StockLedgerPrint([FromQuery] InStockLedgerView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.MOVEMENT")]
    public Task<IActionResult> StockMovement([FromQuery] InStockMovementReportView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.MOVEMENT")]
    public Task<IActionResult> StockMovementData([FromQuery] InStockMovementReportView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.MOVEMENT")]
    public Task<IActionResult> StockMovementExport([FromQuery] InStockMovementReportView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.MOVEMENT")]
    public Task<IActionResult> StockMovementPrint([FromQuery] InStockMovementReportView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.LOW_STOCK")]
    public Task<IActionResult> LowStockReport([FromQuery] InLowStockReportView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.LOW_STOCK")]
    public Task<IActionResult> LowStockReportData([FromQuery] InLowStockReportView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.LOW_STOCK")]
    public Task<IActionResult> LowStockReportExport([FromQuery] InLowStockReportView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.LOW_STOCK")]
    public Task<IActionResult> LowStockReportPrint([FromQuery] InLowStockReportView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.NON_MOVING")]
    public Task<IActionResult> NonMoving([FromQuery] InNonMovingView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.NON_MOVING")]
    public Task<IActionResult> NonMovingData([FromQuery] InNonMovingView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.NON_MOVING")]
    public Task<IActionResult> NonMovingExport([FromQuery] InNonMovingView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.NON_MOVING")]
    public Task<IActionResult> NonMovingPrint([FromQuery] InNonMovingView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    [FeatureAccess("REPORTS.PURCHASE.BILLS")]
    public Task<IActionResult> PurchaseBills([FromQuery] InPurchaseBillsView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.PURCHASE.BILLS")]
    public Task<IActionResult> PurchaseBillsData([FromQuery] InPurchaseBillsView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.PURCHASE.BILLS")]
    public Task<IActionResult> PurchaseBillsExport([FromQuery] InPurchaseBillsView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.PURCHASE.BILLS")]
    public Task<IActionResult> PurchaseBillsPrint([FromQuery] InPurchaseBillsView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    [FeatureAccess("REPORTS.PURCHASE.ANALYSIS")]
    public Task<IActionResult> PurchaseAnalysis([FromQuery] InPurchaseAnalysisView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.PURCHASE.ANALYSIS")]
    public Task<IActionResult> PurchaseAnalysisData([FromQuery] InPurchaseAnalysisView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.PURCHASE.ANALYSIS")]
    public Task<IActionResult> PurchaseAnalysisExport([FromQuery] InPurchaseAnalysisView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.PURCHASE.ANALYSIS")]
    public Task<IActionResult> PurchaseAnalysisPrint([FromQuery] InPurchaseAnalysisView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    [FeatureAccess("REPORTS.TAX.GST")]
    public Task<IActionResult> GstSummary([FromQuery] InGstSummaryView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.TAX.GST")]
    public Task<IActionResult> GstSummaryData([FromQuery] InGstSummaryView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.TAX.GST")]
    public Task<IActionResult> GstSummaryExport([FromQuery] InGstSummaryView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.TAX.GST")]
    public Task<IActionResult> GstSummaryPrint([FromQuery] InGstSummaryView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    [RequirePermission("reports.profit")]
    [FeatureAccess("REPORTS.PROFIT.MARGIN")]
    public Task<IActionResult> ProfitMargin([FromQuery] InProfitMarginView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    [RequirePermission("reports.profit")]
    [FeatureAccess("REPORTS.PROFIT.MARGIN")]
    public Task<IActionResult> ProfitMarginData([FromQuery] InProfitMarginView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    [RequirePermission("reports.profit")]
    [FeatureAccess("REPORTS.PROFIT.MARGIN")]
    public Task<IActionResult> ProfitMarginExport([FromQuery] InProfitMarginView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    [RequirePermission("reports.profit")]
    [FeatureAccess("REPORTS.PROFIT.MARGIN")]
    public Task<IActionResult> ProfitMarginPrint([FromQuery] InProfitMarginView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    [FeatureAccess("REPORTS.SALES.DISCOUNT")]
    public Task<IActionResult> DiscountReport([FromQuery] InDiscountReportView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.DISCOUNT")]
    public Task<IActionResult> DiscountReportData([FromQuery] InDiscountReportView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.DISCOUNT")]
    public Task<IActionResult> DiscountReportExport([FromQuery] InDiscountReportView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.DISCOUNT")]
    public Task<IActionResult> DiscountReportPrint([FromQuery] InDiscountReportView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    [FeatureAccess("REPORTS.SALES.CANCELLED")]
    public Task<IActionResult> CancelledBills([FromQuery] InCancelledBillsView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.CANCELLED")]
    public Task<IActionResult> CancelledBillsData([FromQuery] InCancelledBillsView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.CANCELLED")]
    public Task<IActionResult> CancelledBillsExport([FromQuery] InCancelledBillsView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.CANCELLED")]
    public Task<IActionResult> CancelledBillsPrint([FromQuery] InCancelledBillsView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.ADJUSTMENTS")]
    public Task<IActionResult> StockAdjustments([FromQuery] InStockAdjustmentReportView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.ADJUSTMENTS")]
    public Task<IActionResult> StockAdjustmentsData([FromQuery] InStockAdjustmentReportView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.ADJUSTMENTS")]
    public Task<IActionResult> StockAdjustmentsExport([FromQuery] InStockAdjustmentReportView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.STOCK.ADJUSTMENTS")]
    public Task<IActionResult> StockAdjustmentsPrint([FromQuery] InStockAdjustmentReportView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    [FeatureAccess("REPORTS.SALES.HOURLY")]
    public Task<IActionResult> HourlySales([FromQuery] InHourlySalesView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.HOURLY")]
    public Task<IActionResult> HourlySalesData([FromQuery] InHourlySalesView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.HOURLY")]
    public Task<IActionResult> HourlySalesExport([FromQuery] InHourlySalesView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.SALES.HOURLY")]
    public Task<IActionResult> HourlySalesPrint([FromQuery] InHourlySalesView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    [FeatureAccess("REPORTS.PARTIES.MASTERS")]
    public Task<IActionResult> MasterLists([FromQuery] InMasterListsView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.PARTIES.MASTERS")]
    public Task<IActionResult> MasterListsData([FromQuery] InMasterListsView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.PARTIES.MASTERS")]
    public Task<IActionResult> MasterListsExport([FromQuery] InMasterListsView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    [FeatureAccess("REPORTS.PARTIES.MASTERS")]
    public Task<IActionResult> MasterListsPrint([FromQuery] InMasterListsView model, CancellationToken ct) => Print(model, ct);

    #endregion

    #region Report Suite Helpers

    private async Task<IActionResult> Show(InReportQueryView model, CancellationToken ct)
    {
        var input = await MapAsync(model, false, ct);
        var screen = await _svc.GetScreenAsync(input, ct);
        await StripDisabledReportLinksAsync(screen, ct);
        return View(screen);
    }

    private async Task<IActionResult> Data(InReportQueryView model, CancellationToken ct)
    {
        var input = await MapAsync(model, false, ct);
        var screen = await _svc.GetScreenAsync(input, ct);
        if (!string.IsNullOrWhiteSpace(screen.Error))
            return new BadRequestObjectResult(CommonResponseModel.Failure(screen.Error));

        return CommonHelpers.SuccessResponse("Report loaded.", new
        {
            screen.Title,
            screen.TotalCount,
            screen.Page,
            screen.PageSize,
            columns = screen.Columns.Select(c => c.Name),
            rows = screen.Rows.Select(r => r.Select(c => c.Text)),
            totals = screen.Totals?.Select(c => c.Text),
            kpis = screen.Kpis,
            notice = screen.Notice
        });
    }

    private async Task<IActionResult> Export(InReportQueryView model, CancellationToken ct)
    {
        var input = await MapAsync(model, true, ct);
        var screen = await _svc.GetScreenAsync(input, ct);
        if (!string.IsNullOrWhiteSpace(screen.Error))
            return BadRequest(screen.Error);

        var bytes = ReportExcel.Build(screen);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", screen.Key + ".xlsx");
    }

    private async Task<IActionResult> Print(InReportQueryView model, CancellationToken ct)
    {
        var input = await MapAsync(model, true, ct);
        var screen = await _svc.GetScreenAsync(input, ct);
        await StripDisabledReportLinksAsync(screen, ct);
        return View("Print", screen);
    }

    private async Task<InputReportQuery> MapAsync(InReportQueryView model, bool export, CancellationToken ct)
    {
        var key = ControllerContext.ActionDescriptor.ActionName
            .Replace("Data", "", StringComparison.Ordinal)
            .Replace("Export", "", StringComparison.Ordinal)
            .Replace("Print", "", StringComparison.Ordinal);
        var onlyInStock = key == "NonMoving"
            ? !string.Equals(Request.Query["onlyInStock"], "false", StringComparison.OrdinalIgnoreCase)
            : model.OnlyInStock;
        var input = new InputReportQuery
        {
            ReportKey = key,
            ActorUserId = ActorUserId,
            ActorFkStore = ActorFkStore,
            IsPlatformSuperAdmin = IsSuperAdmin,
            CanSeeProfit = IsSuperAdmin || await _access.CanAsync("reports.profit", ct),
            PrintedBy = User.Identity?.Name,
            ForExport = export,
            DateFrom = model.DateFrom,
            DateTo = model.DateTo,
            FkStore = model.FkStore,
            Page = model.Page,
            PageSize = model.PageSize,
            SortBy = model.SortBy,
            SortDir = model.SortDir,
            FkCategory = model.FkCategory,
            FkSubCategory = model.FkSubCategory,
            FkBrand = model.FkBrand,
            FkSupplier = model.FkSupplier,
            FkCustomer = model.FkCustomer,
            FkUser = model.FkUser,
            FkProduct = model.FkProduct,
            FkProductVariant = model.FkProductVariant,
            Search = model.Search,
            PaymentMode = model.PaymentMode,
            Status = model.Status,
            Level = model.Level,
            GroupBy = model.GroupBy,
            MovementType = model.MovementType,
            Days = model.Days,
            ShowZero = model.ShowZero,
            OnlyNegative = model.OnlyNegative,
            OnlyInStock = onlyInStock
        };
        return input;
    }

    private async Task StripDisabledReportLinksAsync(OutputReportScreen screen, CancellationToken ct)
    {
        foreach (var row in screen.Rows)
        {
            foreach (var cell in row)
                cell.Href = await KeepReportHrefAsync(cell.Href, ct);
        }

        if (screen.Totals is null)
            return;

        foreach (var cell in screen.Totals)
            cell.Href = await KeepReportHrefAsync(cell.Href, ct);
    }

    private async Task<string?> KeepReportHrefAsync(string? href, CancellationToken ct)
    {
        var action = ReportFeatureMap.ActionFromHref(href);
        var code = ReportFeatureMap.ForAction(action);
        if (code is null)
            return href;
        return await _access.FeatureEnabledAsync(code, ct) ? href : null;
    }

    #endregion

    #region Export & Formatting Helpers

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

    #endregion
}
