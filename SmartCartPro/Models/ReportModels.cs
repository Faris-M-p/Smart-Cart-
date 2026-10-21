using System.ComponentModel.DataAnnotations;
using SmartCartPro.CustomModelValidation;

namespace SmartCartPro.Models;

// ═══════════════════════════════════════════════════════════════════
// REPORT MODELS — store-scoped analytics (In / Input / Output)
// Calendar dates are in locale.timezone; services convert to UTC.
// ═══════════════════════════════════════════════════════════════════

#region Shared report options

public sealed class OutputReportStoreOption
{
    public int IdStore { get; set; }
    public string StoreName { get; set; } = string.Empty;
}

public sealed class OutputReportCategoryOption
{
    public int IdCategory { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public sealed class OutputReportKpi
{
    public string Label { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string? Hint { get; set; }
}

public sealed class OutputReportChartPoint
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public sealed class OutputReportSaleFact
{
    public DateTime SaleDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public bool Cancelled { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class OutputReportReturnFact
{
    public DateTime ReturnDate { get; set; }
    public decimal TotalAmount { get; set; }
}

#endregion

#region Report Index

public sealed class InputReportIndex
{
    public int ActorUserId { get; set; }
    public int ActorFkStore { get; set; }
    public bool IsPlatformSuperAdmin { get; set; }
}

public sealed class OutputReportIndexCard
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? ExternalController { get; set; }
    public string? ExternalAction { get; set; }
}

public sealed class OutputReportIndex
{
    public string Currency { get; set; } = "INR";
    public string TimeZoneId { get; set; } = "Asia/Kolkata";
    public List<OutputReportIndexCard> Cards { get; set; } = new();
}

#endregion

#region Sales

public sealed class InReportSalesView
{
    [Display(Name = "From")]
    public DateTime? DateFrom { get; set; }

    [Display(Name = "To")]
    [DateGreaterThanOrEqualTo("DateFrom", ErrorMessage = "The To date must be on or after the From date.")]
    public DateTime? DateTo { get; set; }

    [Display(Name = "Store")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Store.")]
    public int FkStore { get; set; }

    [Display(Name = "Group by")]
    [Required(ErrorMessage = "Group by is required.")]
    [AllowedValues("day", "month", ErrorMessage = "Something went wrong/Please select a valid grouping.")]
    [StringLengthValidation(0, 10, ErrorMessage = "Something went wrong/Group by is invalid.")]
    [NoScriptTags(ErrorMessage = "Something went wrong/Group by is invalid.")]
    public string GroupBy { get; set; } = "day";
}

public sealed class InputReportSales
{
    public int ActorUserId { get; set; }
    public int ActorFkStore { get; set; }
    public bool IsPlatformSuperAdmin { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int FkStore { get; set; }
    public string GroupBy { get; set; } = "day";
    public DateTime FromUtc { get; set; }
    public DateTime ToUtcExclusive { get; set; }
    public List<int>? StoreIds { get; set; }
}

public sealed class OutputReportSalesRow
{
    public string BucketLabel { get; set; } = string.Empty;
    public DateOnly BucketDate { get; set; }
    public int InvoiceCount { get; set; }
    public decimal Gross { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Net { get; set; }
    public decimal AverageBill { get; set; }
    public decimal Returns { get; set; }
}

public sealed class OutputReportPaymentSlice
{
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int PaymentCount { get; set; }
}

public sealed class OutputReportSales
{
    public InReportSalesView Filter { get; set; } = new();
    public string Currency { get; set; } = "INR";
    public string TimeZoneId { get; set; } = "Asia/Kolkata";
    public bool ShowStorePicker { get; set; }
    public List<OutputReportStoreOption> Stores { get; set; } = new();
    public List<OutputReportKpi> Kpis { get; set; } = new();
    public List<OutputReportSalesRow> Rows { get; set; } = new();
    public List<OutputReportPaymentSlice> Payments { get; set; } = new();
    public List<OutputReportChartPoint> Chart { get; set; } = new();
    public int CancelledCount { get; set; }
    public decimal GrandNet { get; set; }
}

#endregion

#region Products

public sealed class InReportProductsView
{
    [Display(Name = "From")]
    public DateTime? DateFrom { get; set; }

    [Display(Name = "To")]
    [DateGreaterThanOrEqualTo("DateFrom", ErrorMessage = "The To date must be on or after the From date.")]
    public DateTime? DateTo { get; set; }

    [Display(Name = "Store")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Store.")]
    public int FkStore { get; set; }

    [Display(Name = "Category")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Category.")]
    public int FkCategory { get; set; }

    [Display(Name = "Top")]
    [Required]
    [AllowedValues(10, 25, 50, 100, ErrorMessage = "Something went wrong/Please select a valid Top value.")]
    public int Top { get; set; } = 10;

    [Display(Name = "Rank by")]
    [Required(ErrorMessage = "Rank by is required.")]
    [AllowedValues("qty", "revenue", ErrorMessage = "Something went wrong/Please select a valid ranking.")]
    [StringLengthValidation(0, 10, ErrorMessage = "Something went wrong/Rank by is invalid.")]
    [NoScriptTags(ErrorMessage = "Something went wrong/Rank by is invalid.")]
    public string RankBy { get; set; } = "qty";
}

public sealed class InputReportProducts
{
    public int ActorUserId { get; set; }
    public int ActorFkStore { get; set; }
    public bool IsPlatformSuperAdmin { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int FkStore { get; set; }
    public int? FkCategory { get; set; }
    public int Top { get; set; } = 10;
    public string RankBy { get; set; } = "qty";
    public DateTime FromUtc { get; set; }
    public DateTime ToUtcExclusive { get; set; }
    public List<int>? StoreIds { get; set; }
}

public sealed class OutputReportProductRow
{
    public int FkProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public string? CategoryName { get; set; }
    public decimal QuantitySold { get; set; }
    public decimal Revenue { get; set; }
    public int TransactionCount { get; set; }
}

public sealed class OutputReportProducts
{
    public InReportProductsView Filter { get; set; } = new();
    public string Currency { get; set; } = "INR";
    public string TimeZoneId { get; set; } = "Asia/Kolkata";
    public bool ShowStorePicker { get; set; }
    public List<OutputReportStoreOption> Stores { get; set; } = new();
    public List<OutputReportCategoryOption> Categories { get; set; } = new();
    public List<OutputReportKpi> Kpis { get; set; } = new();
    public List<OutputReportProductRow> Rows { get; set; } = new();
    public List<OutputReportChartPoint> Chart { get; set; } = new();
}

#endregion

#region Stock Valuation

public sealed class InReportStockValuationView
{
    [Display(Name = "Store")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Store.")]
    public int FkStore { get; set; }

    [Display(Name = "Category")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Category.")]
    public int FkCategory { get; set; }

    [Display(Name = "Include zero and negative")]
    public bool IncludeZeroNegative { get; set; }
}

public sealed class InputReportStockValuation
{
    public int ActorUserId { get; set; }
    public int ActorFkStore { get; set; }
    public bool IsPlatformSuperAdmin { get; set; }
    public int FkStore { get; set; }
    public int? FkCategory { get; set; }
    public bool IncludeZeroNegative { get; set; }
    public List<int>? StoreIds { get; set; }
}

public sealed class OutputReportStockValuationRow
{
    public int FkProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public string CategoryName { get; set; } = "Uncategorised";
    public string? StoreName { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal UnitCost { get; set; }
    public decimal StockValue { get; set; }
}

public sealed class OutputReportStockValuationGroup
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal QuantityOnHand { get; set; }
    public decimal StockValue { get; set; }
}

public sealed class OutputReportStockValuation
{
    public InReportStockValuationView Filter { get; set; } = new();
    public string Currency { get; set; } = "INR";
    public bool ShowStorePicker { get; set; }
    public List<OutputReportStoreOption> Stores { get; set; } = new();
    public List<OutputReportCategoryOption> Categories { get; set; } = new();
    public List<OutputReportKpi> Kpis { get; set; } = new();
    public List<OutputReportStockValuationRow> Rows { get; set; } = new();
    public List<OutputReportStockValuationGroup> CategoryTotals { get; set; } = new();
    public decimal TotalStockValue { get; set; }
    public string CostBasisNote { get; set; } =
        "Stock value is quantity on hand × the product cost price (document cost basis).";
}

#endregion

#region Tax

public sealed class InReportTaxView
{
    [Display(Name = "From")]
    public DateTime? DateFrom { get; set; }

    [Display(Name = "To")]
    [DateGreaterThanOrEqualTo("DateFrom", ErrorMessage = "The To date must be on or after the From date.")]
    public DateTime? DateTo { get; set; }

    [Display(Name = "Store")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Store.")]
    public int FkStore { get; set; }
}

public sealed class InputReportTax
{
    public int ActorUserId { get; set; }
    public int ActorFkStore { get; set; }
    public bool IsPlatformSuperAdmin { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int FkStore { get; set; }
    public DateTime FromUtc { get; set; }
    public DateTime ToUtcExclusive { get; set; }
    public List<int>? StoreIds { get; set; }
}

public sealed class OutputReportTaxRow
{
    public string TaxCode { get; set; } = string.Empty;
    public string TaxName { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public int SaleCount { get; set; }
    public decimal NetSales { get; set; }
    public decimal TaxCollected { get; set; }
}

public sealed class OutputReportTax
{
    public InReportTaxView Filter { get; set; } = new();
    public string Currency { get; set; } = "INR";
    public string TimeZoneId { get; set; } = "Asia/Kolkata";
    public bool ShowStorePicker { get; set; }
    public List<OutputReportStoreOption> Stores { get; set; } = new();
    public List<OutputReportKpi> Kpis { get; set; } = new();
    public List<OutputReportTaxRow> Rows { get; set; } = new();
    public List<OutputReportChartPoint> Chart { get; set; } = new();
    public decimal TotalTaxCollected { get; set; }
}

#endregion

#region Purchases

public sealed class InReportPurchasesView
{
    [Display(Name = "From")]
    public DateTime? DateFrom { get; set; }

    [Display(Name = "To")]
    [DateGreaterThanOrEqualTo("DateFrom", ErrorMessage = "The To date must be on or after the From date.")]
    public DateTime? DateTo { get; set; }

    [Display(Name = "Store")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Store.")]
    public int FkStore { get; set; }

    [Display(Name = "Exclude cancelled")]
    public bool ExcludeCancelled { get; set; } = true;
}

public sealed class InputReportPurchases
{
    public int ActorUserId { get; set; }
    public int ActorFkStore { get; set; }
    public bool IsPlatformSuperAdmin { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int FkStore { get; set; }
    public bool ExcludeCancelled { get; set; } = true;
    public DateOnly FromLocal { get; set; }
    public DateOnly ToLocal { get; set; }
    public List<int>? StoreIds { get; set; }
}

public sealed class OutputReportPurchaseRow
{
    public int FkSupplier { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public int PurchaseCount { get; set; }
    public decimal OrderedQty { get; set; }
    public decimal ReceivedQty { get; set; }
    public decimal OrderedAmount { get; set; }
}

public sealed class OutputReportPurchases
{
    public InReportPurchasesView Filter { get; set; } = new();
    public string Currency { get; set; } = "INR";
    public string TimeZoneId { get; set; } = "Asia/Kolkata";
    public bool ShowStorePicker { get; set; }
    public List<OutputReportStoreOption> Stores { get; set; } = new();
    public List<OutputReportKpi> Kpis { get; set; } = new();
    public List<OutputReportPurchaseRow> Rows { get; set; } = new();
    public List<OutputReportChartPoint> Chart { get; set; } = new();
}

#endregion

#region Customers

public sealed class InReportCustomersView
{
    [Display(Name = "From")]
    public DateTime? DateFrom { get; set; }

    [Display(Name = "To")]
    [DateGreaterThanOrEqualTo("DateFrom", ErrorMessage = "The To date must be on or after the From date.")]
    public DateTime? DateTo { get; set; }

    [Display(Name = "Store")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Store.")]
    public int FkStore { get; set; }

    [Display(Name = "Top")]
    [Required]
    [AllowedValues(10, 25, 50, 100, ErrorMessage = "Something went wrong/Please select a valid Top value.")]
    public int Top { get; set; } = 25;
}

public sealed class InputReportCustomers
{
    public int ActorUserId { get; set; }
    public int ActorFkStore { get; set; }
    public bool IsPlatformSuperAdmin { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int FkStore { get; set; }
    public int Top { get; set; } = 25;
    public DateTime FromUtc { get; set; }
    public DateTime ToUtcExclusive { get; set; }
    public List<int>? StoreIds { get; set; }
}

public sealed class OutputReportCustomerRow
{
    public int? FkCustomer { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int VisitCount { get; set; }
    public decimal TotalSpend { get; set; }
    public decimal AverageSpend { get; set; }
    public DateTime? LastPurchaseAt { get; set; }
}

public sealed class OutputReportCustomers
{
    public InReportCustomersView Filter { get; set; } = new();
    public string Currency { get; set; } = "INR";
    public string TimeZoneId { get; set; } = "Asia/Kolkata";
    public bool ShowStorePicker { get; set; }
    public List<OutputReportStoreOption> Stores { get; set; } = new();
    public List<OutputReportKpi> Kpis { get; set; } = new();
    public List<OutputReportCustomerRow> Rows { get; set; } = new();
    public List<OutputReportChartPoint> Chart { get; set; } = new();
}

#endregion
