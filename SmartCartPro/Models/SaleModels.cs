using System.ComponentModel.DataAnnotations;
using SmartCartPro.CustomModelValidation;

namespace SmartCartPro.Models;

// ═══════════════════════════════════════════════════════════════════
// POS PAGE
// ═══════════════════════════════════════════════════════════════════

#region POS Page

public sealed class InputPosSelect
{
    public int FkStore { get; set; }
}

public sealed class OutputPosSelect
{
    public int FkStore { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "INR";
    public decimal DefaultTaxRate { get; set; }
    public OutputCashSessionSelect? OpenSession { get; set; }
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// POS PRODUCT LOOKUP
// ═══════════════════════════════════════════════════════════════════

#region Sale Product Lookup

public sealed class InputSaleProductSearch
{
    public int FkStore { get; set; }
    public string? Term { get; set; }
    public int Take { get; set; } = 20;
}

public sealed class InputSaleProductBarcode
{
    public int FkStore { get; set; }
    public string Barcode { get; set; } = string.Empty;
}

public sealed class InputSaleProductKeys
{
    public int FkStore { get; set; }
    public List<InputSaleProductKey> Keys { get; set; } = new();
}

public sealed class InputSaleProductKey
{
    public int FkProduct { get; set; }
    public int? FkProductVariant { get; set; }
}

public sealed class OutputSaleProductSelect
{
    public int IdProduct { get; set; }
    public int? IdProductVariant { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public string? UnitName { get; set; }
    public string? UnitCode { get; set; }
    public bool AllowsFraction { get; set; }
    public decimal SellingPrice { get; set; }
    public bool HasProductTax { get; set; }
    public decimal TaxRate { get; set; }
    public decimal StockOnHand { get; set; }
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// POS SALE (CREATE)
// ═══════════════════════════════════════════════════════════════════

#region Sale Create

public sealed class InSaleCreateView
{
    [Display(Name = "Customer")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Customer.")]
    public int? FkCustomer { get; set; }

    [Display(Name = "Notes")]
    [StringLengthValidation(0, 500, ErrorMessage = "The Notes cannot exceed 500 characters.")]
    [NoScriptTags(ErrorMessage = "The Notes cannot contain script tags.")]
    public string? Notes { get; set; }

    [Display(Name = "Client Reference")]
    [Required(ErrorMessage = "Something went wrong/The Client Reference is required.")]
    [StringLengthValidation(8, 100, ErrorMessage = "Something went wrong/The Client Reference must be between 8 and 100 characters.")]
    [NoScriptTags(ErrorMessage = "Something went wrong/The Client Reference cannot contain script tags.")]
    public string ClientRef { get; set; } = string.Empty;

    [Display(Name = "Lines")]
    [Required]
    [NotEmptyList(ErrorMessage = "Add at least one item to the bill.")]
    public List<InSaleLineView> Lines { get; set; } = new();

    [Display(Name = "Payments")]
    [Required]
    [NotEmptyList(ErrorMessage = "Add at least one payment.")]
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

    [Display(Name = "Discount Amount")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Discount Amount must be greater than or equal to zero.")]
    public decimal DiscountAmount { get; set; }
}

public sealed class InPaymentView
{
    [Display(Name = "Payment Method")]
    [Required(ErrorMessage = "The Payment Method field is required.")]
    [AllowedValues("cash", "card", "upi", "wallet", "bank", "other", ErrorMessage = "Choose a valid payment method.")]
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
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ChangeGiven { get; set; }
    public List<InputSaleLine> Lines { get; set; } = new();
    public List<InputPayment> Payments { get; set; } = new();
}

public sealed class InputSaleLine
{
    public int FkProduct { get; set; }
    public int? FkProductVariant { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
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

public sealed class InputSaleSelect
{
    public int IdSale { get; set; }
    public int FkStore { get; set; }
}

public sealed class OutputSaleLine
{
    public int IdSaleDetail { get; set; }
    public int FkProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public int? FkProductVariant { get; set; }
    public string? VariantName { get; set; }
    public string? Sku { get; set; }
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
    public string? StorePhone { get; set; }
    public string? StoreEmail { get; set; }
    public string? StoreAddress { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyLegalName { get; set; }
    public string? CompanyTaxNumber { get; set; }
    public string? CompanyPhone { get; set; }
    public string? CompanyEmail { get; set; }
    public string? CompanyAddress { get; set; }
    public string CurrencyCode { get; set; } = "INR";
    public int? FkCustomer { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerEmail { get; set; }
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
    public string? CashierName { get; set; }
    public bool Cancelled { get; set; }
    public DateTime? CancelledOn { get; set; }
    public string? CancelledReason { get; set; }
    public bool HasReturns { get; set; }
    public bool CanCancel { get; set; }
    public bool CanReturn { get; set; }
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
    public int FkStore { get; set; }
    public int? FkCustomer { get; set; }
    public int? CustomerId { get; set; }
    public string? Status { get; set; }
    public string? PaymentMethod { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToExclusiveUtc { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string SortBy { get; set; } = "saledate";
    public bool SortDesc { get; set; } = true;
}

public sealed class OutputSaleListItem
{
    public int IdSale { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public string? CustomerName { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
}

public sealed class OutputSaleList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public decimal PageSubTotal { get; set; }
    public decimal PageTaxAmount { get; set; }
    public decimal PageDiscountAmount { get; set; }
    public decimal PageTotalAmount { get; set; }
    public decimal PagePaidAmount { get; set; }
    public List<OutputSaleListItem> Items { get; set; } = new();
    public InputSaleList? Filter { get; set; }
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
    [StringLengthValidation(3, 500, ErrorMessage = "The Reason must be between 3 and 500 characters.")]
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

    [Display(Name = "Customer")]
    public string? CustomerName { get; set; }

    [Display(Name = "Sale Date")]
    public DateTime SaleDate { get; set; }

    [Display(Name = "Cashier")]
    public string? CashierName { get; set; }

    [Display(Name = "Reason")]
    [Required(ErrorMessage = "The Reason field is required.")]
    [StringLengthValidation(3, 500, ErrorMessage = "The Reason must be between 3 and 500 characters.")]
    [NoScriptTags(ErrorMessage = "The Reason cannot contain script tags.")]
    public string Reason { get; set; } = string.Empty;

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

    [Display(Name = "Variant Name")]
    public string? VariantName { get; set; }

    [Display(Name = "SKU")]
    public string? Sku { get; set; }

    [Display(Name = "Original Qty")]
    public decimal OriginalQty { get; set; }

    [Display(Name = "Already Returned")]
    public decimal AlreadyReturnedQty { get; set; }

    [Display(Name = "Remaining Qty")]
    public decimal RemainingQty { get; set; }

    [Display(Name = "Max Qty")]
    public decimal MaxQty { get; set; }

    [Display(Name = "Unit Price")]
    public decimal UnitPrice { get; set; }

    [Display(Name = "Return Qty")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Return Qty must be greater than or equal to zero.")]
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
    public int IdSaleDetail { get; set; }
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

public sealed class InputSaleReturnableSelect
{
    public int IdSale { get; set; }
    public int FkStore { get; set; }
}

public sealed class OutputSaleReturnableSelect
{
    public int IdSale { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public string? CustomerName { get; set; }
    public string? CashierName { get; set; }
    public List<OutputSaleReturnableLine> Lines { get; set; } = new();
}

public sealed class OutputSaleReturnableLine
{
    public int IdSaleDetail { get; set; }
    public int FkProduct { get; set; }
    public int? FkProductVariant { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public string? Sku { get; set; }
    public decimal OriginalQty { get; set; }
    public decimal ReturnedQty { get; set; }
    public decimal RemainingQty { get; set; }
    public decimal UnitPrice { get; set; }
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// SALES RETURN LIST
// ═══════════════════════════════════════════════════════════════════

#region Sales Return List

public sealed class InputSalesReturnList
{
    public int FkStore { get; set; }
    public string? Status { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToExclusiveUtc { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string SortBy { get; set; } = "returndate";
    public bool SortDesc { get; set; } = true;
}

public sealed class OutputSalesReturnListItem
{
    public int IdSalesReturn { get; set; }
    public int FkSale { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public string? CashierName { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class OutputSalesReturnList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputSalesReturnListItem> Items { get; set; } = new();
    public InputSalesReturnList? Filter { get; set; }
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// SALES RETURN SELECT
// ═══════════════════════════════════════════════════════════════════

#region Sales Return Select

public sealed class InputSalesReturnSelect
{
    public int IdSalesReturn { get; set; }
    public int FkStore { get; set; }
}

public sealed class OutputSalesReturnLine
{
    public int IdSalesReturnDetail { get; set; }
    public int FkProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int? FkProductVariant { get; set; }
    public string? VariantName { get; set; }
    public string? Sku { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class OutputSalesReturnSelect
{
    public int IdSalesReturn { get; set; }
    public int FkStore { get; set; }
    public int FkSale { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string ReturnNumber { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime ReturnDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public string? CashierName { get; set; }
    public bool Cancelled { get; set; }
    public DateTime? CancelledOn { get; set; }
    public string? CancelledReason { get; set; }
    public List<OutputSalesReturnLine> Lines { get; set; } = new();
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// CASH SESSION
// ═══════════════════════════════════════════════════════════════════

#region Cash Session

public sealed class InCashSessionOpenView
{
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
    public string StoreName { get; set; } = string.Empty;
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

public sealed class InputCashSessionList
{
    public int FkStore { get; set; }
    public string? Status { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public DateTime? FromUtc { get; set; }
    public DateTime? ToExclusiveUtc { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class OutputCashSessionListItem
{
    public int IdCashSession { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal OpeningFloat { get; set; }
    public decimal? ClosingCash { get; set; }
    public decimal? ExpectedCash { get; set; }
    public decimal? Variance { get; set; }
    public string Status { get; set; } = string.Empty;
    public int SaleCount { get; set; }
}

public sealed class OutputCashSessionList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public OutputCashSessionSelect? OpenSession { get; set; }
    public List<OutputCashSessionListItem> Items { get; set; } = new();
    public InputCashSessionList? Filter { get; set; }
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
    public int FkStore { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime OpenedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public decimal OpeningFloat { get; set; }
    public decimal? ClosingCash { get; set; }
    public decimal ExpectedCash { get; set; }
    public decimal? Variance { get; set; }
    public int TotalSales { get; set; }
    public decimal TotalSalesAmount { get; set; }
    public int CancelledSales { get; set; }
    public int ReturnCount { get; set; }
    public decimal TotalReturns { get; set; }
    public decimal NetSales { get; set; }
    public decimal CashIn { get; set; }
    public decimal ChangeGiven { get; set; }
    public List<OutputCashSummaryPaymentMethod> ByMethod { get; set; } = new();
}

#endregion
