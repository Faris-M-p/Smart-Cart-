using System.ComponentModel.DataAnnotations;
using SmartCartPro.CustomModelValidation;

namespace SmartCartPro.Models;

// ═══════════════════════════════════════════════════════════════════
// POS SALE (CREATE)
// ═══════════════════════════════════════════════════════════════════

#region Sale Create

public sealed class InSaleCreateView
{
    [Display(Name = "Store")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Store.")]
    public int FkStore { get; set; }

    [Display(Name = "Customer")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Customer.")]
    public int? FkCustomer { get; set; }

    [Display(Name = "Cash Session")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Cash Session.")]
    public int? FkCashSession { get; set; }

    [Display(Name = "Notes")]
    [StringLengthValidation(0, 500, ErrorMessage = "The Notes cannot exceed 500 characters.")]
    [NoScriptTags(ErrorMessage = "The Notes cannot contain script tags.")]
    public string? Notes { get; set; }

    /// <summary>Client-generated idempotency reference to prevent double-posts.</summary>
    [Display(Name = "Client Reference")]
    [StringLengthValidation(0, 100, ErrorMessage = "The Client Reference cannot exceed 100 characters.")]
    [NoScriptTags(ErrorMessage = "The Client Reference cannot contain script tags.")]
    public string? ClientRef { get; set; }

    [Display(Name = "Lines")]
    [Required]
    [NotEmptyList(ErrorMessage = "At least one sale line is required.")]
    public List<InSaleLineView> Lines { get; set; } = new();

    [Display(Name = "Payments")]
    [Required]
    [NotEmptyList(ErrorMessage = "At least one payment is required.")]
    public List<InPaymentView> Payments { get; set; } = new();
}

public sealed class InSaleLineView
{
    [Display(Name = "Product")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Product.")]
    public int FkProduct { get; set; }

    [Display(Name = "Variant")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Variant.")]
    public int? FkProductVariant { get; set; }

    [Display(Name = "Quantity")]
    [Required]
    [DecimalGreaterThanZero(ErrorMessage = "The Quantity must be greater than zero.")]
    public decimal Quantity { get; set; }

    /// <summary>Override price; if 0/null, server recalculates from product master.</summary>
    [Display(Name = "Unit Price")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Unit Price must be greater than or equal to zero.")]
    public decimal? UnitPrice { get; set; }

    [Display(Name = "Discount Amount")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Discount Amount must be greater than or equal to zero.")]
    public decimal DiscountAmount { get; set; }
}

public sealed class InPaymentView
{
    [Display(Name = "Payment Method")]
    [Required(ErrorMessage = "The Payment Method field is required.")]
    [StringLengthValidation(1, 50, ErrorMessage = "The Payment Method must be between 1 and 50 characters.")]
    [NoScriptTags(ErrorMessage = "The Payment Method cannot contain script tags.")]
    public string PaymentMethod { get; set; } = string.Empty;

    [Display(Name = "Amount")]
    [Required]
    [DecimalGreaterThanZero(ErrorMessage = "The Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Display(Name = "Reference No.")]
    [StringLengthValidation(0, 100, ErrorMessage = "The Reference No. cannot exceed 100 characters.")]
    [NoScriptTags(ErrorMessage = "The Reference No. cannot contain script tags.")]
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
    [Display(Name = "Sale")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Sale.")]
    public int IdSale { get; set; }

    [Display(Name = "Reason")]
    [Required(ErrorMessage = "The Reason field is required.")]
    [StringLengthValidation(2, 500, ErrorMessage = "The Reason must be between 2 and 500 characters.")]
    [NoScriptTags(ErrorMessage = "The Reason cannot contain script tags.")]
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
    [Display(Name = "Sale")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Sale.")]
    public int IdSale { get; set; }

    [Display(Name = "Invoice Number")]
    [StringLengthValidation(0, 50, ErrorMessage = "The Invoice Number cannot exceed 50 characters.")]
    [NoScriptTags(ErrorMessage = "The Invoice Number cannot contain script tags.")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Display(Name = "Reason")]
    [StringLengthValidation(0, 500, ErrorMessage = "The Reason cannot exceed 500 characters.")]
    [NoScriptTags(ErrorMessage = "The Reason cannot contain script tags.")]
    public string? Reason { get; set; }

    [Display(Name = "Lines")]
    [Required]
    [NotEmptyList(ErrorMessage = "At least one return line is required.")]
    public List<InReturnLineView> Lines { get; set; } = new();
}

public sealed class InReturnLineView
{
    [Display(Name = "Sale Detail")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Sale Detail.")]
    public int IdSaleDetail { get; set; }

    [Display(Name = "Product")]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Product.")]
    public int FkProduct { get; set; }

    [Display(Name = "Variant")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Variant.")]
    public int? FkProductVariant { get; set; }

    [Display(Name = "Product Name")]
    public string ProductName { get; set; } = string.Empty;

    [Display(Name = "Max Qty")]
    public decimal MaxQty { get; set; }

    [Display(Name = "Return Qty")]
    [DecimalGreaterThanZero(ErrorMessage = "The Return Qty must be greater than zero.")]
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
    [Display(Name = "Store")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Store.")]
    public int FkStore { get; set; }

    [Display(Name = "Opening Float")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Opening Float must be greater than or equal to zero.")]
    public decimal OpeningFloat { get; set; }

    [Display(Name = "Notes")]
    [StringLengthValidation(0, 500, ErrorMessage = "The Notes cannot exceed 500 characters.")]
    [NoScriptTags(ErrorMessage = "The Notes cannot contain script tags.")]
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
    [Display(Name = "Cash Session")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Cash Session.")]
    public int IdCashSession { get; set; }

    [Display(Name = "Closing Cash Counted")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Closing Cash Counted must be greater than or equal to zero.")]
    public decimal ClosingCash { get; set; }

    [Display(Name = "Notes")]
    [StringLengthValidation(0, 500, ErrorMessage = "The Notes cannot exceed 500 characters.")]
    [NoScriptTags(ErrorMessage = "The Notes cannot contain script tags.")]
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
