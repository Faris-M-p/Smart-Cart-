using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.Models;

// ═══════════════════════════════════════════════════════════════════
// POS SALE (CREATE)
// ═══════════════════════════════════════════════════════════════════

#region Sale Create

public sealed class InSaleCreateView
{
    [Required]
    public int FkStore { get; set; }

    public int? FkCustomer { get; set; }

    public int? FkCashSession { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    /// <summary>Client-generated idempotency reference to prevent double-posts.</summary>
    [MaxLength(100)]
    public string? ClientRef { get; set; }

    [Required, MinLength(1)]
    public List<InSaleLineView> Lines { get; set; } = new();

    [Required, MinLength(1)]
    public List<InPaymentView> Payments { get; set; } = new();
}

public sealed class InSaleLineView
{
    [Required]
    public int FkProduct { get; set; }

    public int? FkProductVariant { get; set; }

    [Required, Range(0.001, double.MaxValue)]
    public decimal Quantity { get; set; }

    /// <summary>Override price; if 0/null, server recalculates from product master.</summary>
    public decimal? UnitPrice { get; set; }

    [Range(0, double.MaxValue)]
    public decimal DiscountAmount { get; set; }
}

public sealed class InPaymentView
{
    [Required]
    public string PaymentMethod { get; set; } = string.Empty;

    [Required, Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    public string? ReferenceNo { get; set; }
}

public sealed class InputSaleCreate
{
    public int FkStore { get; set; }
    public int? FkCustomer { get; set; }
    public int? FkCashSession { get; set; }
    public string? Notes { get; set; }
    public string? ClientRef { get; set; }
    public int? CreatedBy { get; set; }
    public List<InputSaleLine> Lines { get; set; } = new();
    public List<InputPayment> Payments { get; set; } = new();
}

public sealed class InputSaleLine
{
    public int FkProduct { get; set; }
    public int? FkProductVariant { get; set; }
    public decimal Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
}

public sealed class InputPayment
{
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? ReferenceNo { get; set; }
}

public sealed class OutputSaleCreate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdSale { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ChangeGiven { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// SALE SELECT (INVOICE DETAIL)
// ═══════════════════════════════════════════════════════════════════

#region Sale Select

public sealed class InputSaleSelect { public int IdSale { get; set; } }

public sealed class OutputSaleLine
{
    public int IdSaleDetail { get; set; }
    public int FkProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public int? FkProductVariant { get; set; }
    public string? VariantName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class OutputSalePayment
{
    public int IdPayment { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? ReferenceNo { get; set; }
    public DateTime PaidAt { get; set; }
}

public sealed class OutputSaleSelect
{
    public int IdSale { get; set; }
    public int FkStore { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int? FkCustomer { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ChangeGiven { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<OutputSaleLine> Lines { get; set; } = new();
    public List<OutputSalePayment> Payments { get; set; } = new();
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// SALE LIST
// ═══════════════════════════════════════════════════════════════════

#region Sale List

public sealed class InputSaleList
{
    public int? FkStore { get; set; }
    public int? FkCustomer { get; set; }
    public string? Status { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string SortBy { get; set; } = "saledate";
    public bool SortDesc { get; set; } = true;
}

public sealed class OutputSaleListItem
{
    public int IdSale { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public string? CustomerName { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class OutputSaleList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputSaleListItem> Items { get; set; } = new();
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// SALE CANCEL
// ═══════════════════════════════════════════════════════════════════

#region Sale Cancel

public sealed class InSaleCancelView
{
    public int IdSale { get; set; }

    [Required, MaxLength(500)]
    [Display(Name = "Reason")]
    public string Reason { get; set; } = string.Empty;
}

public sealed class InputSaleCancel
{
    public int IdSale { get; set; }
    public int FkStore { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int CancelledBy { get; set; }
}

public sealed class OutputSaleCancel
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// SALE RETURN
// ═══════════════════════════════════════════════════════════════════

#region Sale Return

public sealed class InSaleReturnView
{
    public int IdSale { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Reason { get; set; }

    [Required, MinLength(1)]
    public List<InReturnLineView> Lines { get; set; } = new();
}

public sealed class InReturnLineView
{
    public int IdSaleDetail { get; set; }
    public int FkProduct { get; set; }
    public int? FkProductVariant { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal MaxQty { get; set; }

    [Range(0.001, double.MaxValue)]
    public decimal ReturnQty { get; set; }
}

public sealed class InputSaleReturn
{
    public int IdSale { get; set; }
    public int FkStore { get; set; }
    public string? Reason { get; set; }
    public int CreatedBy { get; set; }
    public List<InputReturnLine> Lines { get; set; } = new();
}

public sealed class InputReturnLine
{
    public int FkProduct { get; set; }
    public int? FkProductVariant { get; set; }
    public decimal ReturnQty { get; set; }
    public decimal UnitPrice { get; set; }
}

public sealed class OutputSaleReturn
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdSalesReturn { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// CASH SESSION
// ═══════════════════════════════════════════════════════════════════

#region Cash Session

public sealed class InCashSessionOpenView
{
    [Required]
    public int FkStore { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Opening float")]
    public decimal OpeningFloat { get; set; }

    public string? Notes { get; set; }
}

public sealed class InputCashSessionOpen
{
    public int FkStore { get; set; }
    public int FkUser { get; set; }
    public decimal OpeningFloat { get; set; }
    public string? Notes { get; set; }
}

public sealed class InCashSessionCloseView
{
    public int IdCashSession { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Closing cash counted")]
    public decimal ClosingCash { get; set; }

    public string? Notes { get; set; }
}

public sealed class InputCashSessionClose
{
    public int IdCashSession { get; set; }
    public int FkStore { get; set; }
    public decimal ClosingCash { get; set; }
    public string? Notes { get; set; }
}

public sealed class OutputCashSessionOpen
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdCashSession { get; set; }
    public bool Ok => ResponseCode == 0;
}

public sealed class OutputCashSessionClose
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public decimal ExpectedCash { get; set; }
    public decimal ClosingCash { get; set; }
    public decimal Variance { get; set; }
    public bool Ok => ResponseCode == 0;
}

public sealed class InputCashSessionSelect
{
    public int FkStore { get; set; }
    public int? IdCashSession { get; set; }
}

public sealed class OutputCashSessionSelect
{
    public int IdCashSession { get; set; }
    public int FkStore { get; set; }
    public int FkUser { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal OpeningFloat { get; set; }
    public decimal? ClosingCash { get; set; }
    public decimal? ExpectedCash { get; set; }
    public decimal? Variance { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// DAY CLOSE / CASH SUMMARY
// ═══════════════════════════════════════════════════════════════════

#region Cash Summary

public sealed class InputCashSummary
{
    public int FkStore { get; set; }
    public int IdCashSession { get; set; }
}

public sealed class OutputCashSummaryPaymentMethod
{
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int Count { get; set; }
}

public sealed class OutputCashSummary
{
    public int IdCashSession { get; set; }
    public DateTime OpenedAt { get; set; }
    public decimal OpeningFloat { get; set; }
    public int TotalSales { get; set; }
    public decimal TotalSalesAmount { get; set; }
    public decimal TotalReturns { get; set; }
    public decimal NetSales { get; set; }
    public decimal ExpectedCash { get; set; }
    public List<OutputCashSummaryPaymentMethod> ByMethod { get; set; } = new();
}

#endregion
