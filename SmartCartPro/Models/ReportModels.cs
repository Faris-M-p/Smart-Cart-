namespace SmartCartPro.Models;

// ═══════════════════════════════════════════════════════════════════
// REPORT MODELS — Phase 7
// All reports are parameterised by date range + optional store.
// ═══════════════════════════════════════════════════════════════════

#region Sales by Date

public sealed class InputReportSalesByDate
{
    public DateTime DateFrom { get; set; } = DateTime.UtcNow.Date.AddDays(-30);
    public DateTime DateTo { get; set; } = DateTime.UtcNow.Date.AddDays(1);
    public int? FkStore { get; set; }
}

public sealed class SalesByDateRow
{
    public DateTime SaleDate { get; set; }
    public int OrderCount { get; set; }
    public decimal NetSales { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
}

public sealed class OutputReportSalesByDate
{
    public InputReportSalesByDate Filter { get; set; } = new();
    public List<SalesByDateRow> Rows { get; set; } = new();
    public decimal GrandTotal { get; set; }
}

#endregion

#region Stock Valuation

public sealed class InputReportStockValuation
{
    public int? FkStore { get; set; }
    public int? FkCategory { get; set; }
}

public sealed class StockValuationRow
{
    public int FkProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public string? CategoryName { get; set; }
    public int? FkStore { get; set; }
    public string? StoreName { get; set; }
    public decimal QuantityOnHand { get; set; }
    /// <summary>Latest purchase price used as cost proxy.</summary>
    public decimal UnitCost { get; set; }
    public decimal StockValue { get; set; }
}

public sealed class OutputReportStockValuation
{
    public InputReportStockValuation Filter { get; set; } = new();
    public List<StockValuationRow> Rows { get; set; } = new();
    public decimal TotalStockValue { get; set; }
}

#endregion

#region Low Stock

public sealed class InputReportLowStock
{
    public int? FkStore { get; set; }
    public int? FkCategory { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 100;
}

// Uses OutputInventorySelect from InventoryModels — reused directly.

#endregion

#region Tax Summary

public sealed class InputReportTaxSummary
{
    public DateTime DateFrom { get; set; } = DateTime.UtcNow.Date.AddDays(-30);
    public DateTime DateTo { get; set; } = DateTime.UtcNow.Date.AddDays(1);
    public int? FkStore { get; set; }
}

public sealed class TaxSummaryRow
{
    public string? TaxCode { get; set; }
    public string? TaxName { get; set; }
    public decimal TaxRate { get; set; }
    public int SaleCount { get; set; }
    public decimal NetSales { get; set; }
    public decimal TaxCollected { get; set; }
}

public sealed class OutputReportTaxSummary
{
    public InputReportTaxSummary Filter { get; set; } = new();
    public List<TaxSummaryRow> Rows { get; set; } = new();
    public decimal TotalTaxCollected { get; set; }
}

#endregion

#region Top Selling Products

public sealed class InputReportTopSelling
{
    public DateTime DateFrom { get; set; } = DateTime.UtcNow.Date.AddDays(-30);
    public DateTime DateTo { get; set; } = DateTime.UtcNow.Date.AddDays(1);
    public int? FkStore { get; set; }
    public int Top { get; set; } = 20;
}

public sealed class TopSellingRow
{
    public int FkProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public string? CategoryName { get; set; }
    public decimal TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TransactionCount { get; set; }
}

public sealed class OutputReportTopSelling
{
    public InputReportTopSelling Filter { get; set; } = new();
    public List<TopSellingRow> Rows { get; set; } = new();
}

#endregion
