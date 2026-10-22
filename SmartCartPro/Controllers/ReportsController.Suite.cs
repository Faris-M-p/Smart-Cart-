using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;

namespace SmartCartPro.Controllers;

public partial class ReportsController
{
    [HttpGet]
    public Task<IActionResult> SalesBillWise([FromQuery] InSalesBillWiseView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    public Task<IActionResult> SalesBillWiseData([FromQuery] InSalesBillWiseView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    public Task<IActionResult> SalesBillWiseExport([FromQuery] InSalesBillWiseView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    public Task<IActionResult> SalesBillWisePrint([FromQuery] InSalesBillWiseView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    public Task<IActionResult> ItemSales([FromQuery] InItemSalesView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    public Task<IActionResult> ItemSalesData([FromQuery] InItemSalesView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    public Task<IActionResult> ItemSalesExport([FromQuery] InItemSalesView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    public Task<IActionResult> ItemSalesPrint([FromQuery] InItemSalesView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    public Task<IActionResult> CategorySales([FromQuery] InCategorySalesView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    public Task<IActionResult> CategorySalesData([FromQuery] InCategorySalesView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    public Task<IActionResult> CategorySalesExport([FromQuery] InCategorySalesView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    public Task<IActionResult> CategorySalesPrint([FromQuery] InCategorySalesView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    public Task<IActionResult> SalesReturns([FromQuery] InSalesReturnReportView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    public Task<IActionResult> SalesReturnsData([FromQuery] InSalesReturnReportView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    public Task<IActionResult> SalesReturnsExport([FromQuery] InSalesReturnReportView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    public Task<IActionResult> SalesReturnsPrint([FromQuery] InSalesReturnReportView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    public Task<IActionResult> DayClose([FromQuery] InDayCloseView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    public Task<IActionResult> DayCloseData([FromQuery] InDayCloseView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    public Task<IActionResult> DayCloseExport([FromQuery] InDayCloseView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    public Task<IActionResult> DayClosePrint([FromQuery] InDayCloseView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    public Task<IActionResult> CurrentStock([FromQuery] InCurrentStockView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    public Task<IActionResult> CurrentStockData([FromQuery] InCurrentStockView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    public Task<IActionResult> CurrentStockExport([FromQuery] InCurrentStockView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    public Task<IActionResult> CurrentStockPrint([FromQuery] InCurrentStockView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    public Task<IActionResult> ItemLedger([FromQuery] InItemLedgerView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    public Task<IActionResult> ItemLedgerData([FromQuery] InItemLedgerView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    public Task<IActionResult> ItemLedgerExport([FromQuery] InItemLedgerView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    public Task<IActionResult> ItemLedgerPrint([FromQuery] InItemLedgerView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    public Task<IActionResult> StockLedger([FromQuery] InStockLedgerView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    public Task<IActionResult> StockLedgerData([FromQuery] InStockLedgerView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    public Task<IActionResult> StockLedgerExport([FromQuery] InStockLedgerView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    public Task<IActionResult> StockLedgerPrint([FromQuery] InStockLedgerView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    public Task<IActionResult> StockMovement([FromQuery] InStockMovementReportView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    public Task<IActionResult> StockMovementData([FromQuery] InStockMovementReportView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    public Task<IActionResult> StockMovementExport([FromQuery] InStockMovementReportView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    public Task<IActionResult> StockMovementPrint([FromQuery] InStockMovementReportView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    public Task<IActionResult> LowStockReport([FromQuery] InLowStockReportView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    public Task<IActionResult> LowStockReportData([FromQuery] InLowStockReportView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    public Task<IActionResult> LowStockReportExport([FromQuery] InLowStockReportView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    public Task<IActionResult> LowStockReportPrint([FromQuery] InLowStockReportView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    public Task<IActionResult> NonMoving([FromQuery] InNonMovingView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    public Task<IActionResult> NonMovingData([FromQuery] InNonMovingView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    public Task<IActionResult> NonMovingExport([FromQuery] InNonMovingView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    public Task<IActionResult> NonMovingPrint([FromQuery] InNonMovingView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    public Task<IActionResult> PurchaseBills([FromQuery] InPurchaseBillsView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    public Task<IActionResult> PurchaseBillsData([FromQuery] InPurchaseBillsView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    public Task<IActionResult> PurchaseBillsExport([FromQuery] InPurchaseBillsView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    public Task<IActionResult> PurchaseBillsPrint([FromQuery] InPurchaseBillsView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    public Task<IActionResult> PurchaseAnalysis([FromQuery] InPurchaseAnalysisView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    public Task<IActionResult> PurchaseAnalysisData([FromQuery] InPurchaseAnalysisView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    public Task<IActionResult> PurchaseAnalysisExport([FromQuery] InPurchaseAnalysisView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    public Task<IActionResult> PurchaseAnalysisPrint([FromQuery] InPurchaseAnalysisView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    public Task<IActionResult> GstSummary([FromQuery] InGstSummaryView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    public Task<IActionResult> GstSummaryData([FromQuery] InGstSummaryView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    public Task<IActionResult> GstSummaryExport([FromQuery] InGstSummaryView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    public Task<IActionResult> GstSummaryPrint([FromQuery] InGstSummaryView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    [RequirePermission("reports.profit")]
    public Task<IActionResult> ProfitMargin([FromQuery] InProfitMarginView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    [RequirePermission("reports.profit")]
    public Task<IActionResult> ProfitMarginData([FromQuery] InProfitMarginView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    [RequirePermission("reports.profit")]
    public Task<IActionResult> ProfitMarginExport([FromQuery] InProfitMarginView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    [RequirePermission("reports.profit")]
    public Task<IActionResult> ProfitMarginPrint([FromQuery] InProfitMarginView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    public Task<IActionResult> DiscountReport([FromQuery] InDiscountReportView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    public Task<IActionResult> DiscountReportData([FromQuery] InDiscountReportView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    public Task<IActionResult> DiscountReportExport([FromQuery] InDiscountReportView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    public Task<IActionResult> DiscountReportPrint([FromQuery] InDiscountReportView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    public Task<IActionResult> CancelledBills([FromQuery] InCancelledBillsView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    public Task<IActionResult> CancelledBillsData([FromQuery] InCancelledBillsView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    public Task<IActionResult> CancelledBillsExport([FromQuery] InCancelledBillsView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    public Task<IActionResult> CancelledBillsPrint([FromQuery] InCancelledBillsView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    public Task<IActionResult> StockAdjustments([FromQuery] InStockAdjustmentReportView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    public Task<IActionResult> StockAdjustmentsData([FromQuery] InStockAdjustmentReportView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    public Task<IActionResult> StockAdjustmentsExport([FromQuery] InStockAdjustmentReportView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    public Task<IActionResult> StockAdjustmentsPrint([FromQuery] InStockAdjustmentReportView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    public Task<IActionResult> HourlySales([FromQuery] InHourlySalesView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    public Task<IActionResult> HourlySalesData([FromQuery] InHourlySalesView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    public Task<IActionResult> HourlySalesExport([FromQuery] InHourlySalesView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    public Task<IActionResult> HourlySalesPrint([FromQuery] InHourlySalesView model, CancellationToken ct) => Print(model, ct);

    [HttpGet]
    public Task<IActionResult> MasterLists([FromQuery] InMasterListsView model, CancellationToken ct) => Show(model, ct);
    [HttpGet]
    public Task<IActionResult> MasterListsData([FromQuery] InMasterListsView model, CancellationToken ct) => Data(model, ct);
    [HttpGet]
    public Task<IActionResult> MasterListsExport([FromQuery] InMasterListsView model, CancellationToken ct) => Export(model, ct);
    [HttpGet]
    public Task<IActionResult> MasterListsPrint([FromQuery] InMasterListsView model, CancellationToken ct) => Print(model, ct);

    private async Task<IActionResult> Show(InReportQueryView model, CancellationToken ct)
    {
        var input = await MapAsync(model, false, ct);
        var screen = await _svc.GetScreenAsync(input, ct);
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
}
