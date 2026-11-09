using System.ComponentModel.DataAnnotations;
using SmartCartPro.CustomModelValidation;

namespace SmartCartPro.Models;

// ═══════════════════════════════════════════════════════════════════
// PURCHASE MODULE MODELS
// ═══════════════════════════════════════════════════════════════════

#region Purchase Management

public sealed class InPurchaseCreateView
{
    [Display(Name = "Supplier")]
    [Required(ErrorMessage = "Please select a Supplier.")]
    [GreaterThanZero(ErrorMessage = "Please select a Supplier.")]
    public int FkSupplier { get; set; }

    [Display(Name = "Purchase Date")]
    [Required(ErrorMessage = "The Purchase Date field is required.")]
    public DateOnly PurchaseDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Supplier Invoice #")]
    [StringLengthValidation(0, 100, ErrorMessage = "The Supplier Invoice # cannot exceed 100 characters.")]
    [NoScriptTags(ErrorMessage = "The Supplier Invoice # cannot contain script tags.")]
    public string? InvoiceNumber { get; set; }

    [Display(Name = "Invoice Date")]
    [Required(ErrorMessage = "The Invoice Date is required.")]
    public DateOnly InvoiceDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Mark as Approved")]
    public bool MarkAsApproved { get; set; }

    [Display(Name = "Amount Paid Now")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "Amount Paid cannot be negative.")]
    public decimal AmountPaid { get; set; }

    [Display(Name = "Payment Method")]
    public string? PaymentMethod { get; set; }

    [Display(Name = "Notes")]
    [StringLengthValidation(0, 1000, ErrorMessage = "The Notes cannot exceed 1000 characters.")]
    [NoScriptTags(ErrorMessage = "The Notes cannot contain script tags.")]
    public string? Notes { get; set; }

    [Display(Name = "Lines")]
    [NotEmptyList(ErrorMessage = "At least one purchase line is required.")]
    public List<InPurchaseLineView> Lines { get; set; } = new();
}

public sealed class InPurchaseLineView
{
    [Display(Name = "Product")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Please select a valid Product.")]
    public int FkProduct { get; set; }

    [Display(Name = "Variant")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Please select a valid Variant.")]
    public int? FkProductVariant { get; set; }

    [Display(Name = "Quantity")]
    [Required]
    [DecimalGreaterThanZero(ErrorMessage = "The Quantity must be greater than zero.")]
    public decimal Quantity { get; set; }

    [Display(Name = "Unit Cost")]
    [Required]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Unit Cost must be greater than or equal to zero.")]
    public decimal UnitCost { get; set; }

    [Display(Name = "Discount")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Discount must be greater than or equal to zero.")]
    public decimal DiscountAmount { get; set; }

    public decimal TaxRate { get; set; }
    public bool TaxInclusive { get; set; }
}

public sealed class InputPurchaseCreate
{
    public int FkStore { get; set; }
    public int FkSupplier { get; set; }
    public DateOnly PurchaseDate { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public bool MarkAsApproved { get; set; }
    public decimal AmountPaid { get; set; }
    public string? PaymentMethod { get; set; }
    public string? Notes { get; set; }
    public int? CreatedBy { get; set; }
    public List<InputPurchaseLine> Lines { get; set; } = new();

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Charges { get; set; }
    public decimal RoundOff { get; set; }
    public decimal TotalAmount { get; set; }
}

public sealed class InputPurchaseLine
{
    public int FkProduct { get; set; }
    public int? FkProductVariant { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxRate { get; set; }
    public bool TaxInclusive { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class OutputPurchaseCreate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdPurchase { get; set; }
    public bool Ok => ResponseCode == 0;
}

public sealed class InPurchaseUpdateView
{
    [Required]
    [GreaterThanZero]
    public int IdPurchase { get; set; }

    [Display(Name = "Supplier")]
    [Required(ErrorMessage = "Please select a Supplier.")]
    [GreaterThanZero(ErrorMessage = "Please select a Supplier.")]
    public int FkSupplier { get; set; }

    [Display(Name = "Purchase Date")]
    [Required]
    public DateOnly PurchaseDate { get; set; }

    [Display(Name = "Supplier Invoice #")]
    [StringLengthValidation(0, 100)]
    [NoScriptTags]
    public string? InvoiceNumber { get; set; }

    [Display(Name = "Invoice Date")]
    [Required]
    public DateOnly InvoiceDate { get; set; }

    [Display(Name = "Notes")]
    [StringLengthValidation(0, 1000)]
    [NoScriptTags]
    public string? Notes { get; set; }

    [Display(Name = "Lines")]
    [NotEmptyList(ErrorMessage = "At least one purchase line is required.")]
    public List<InPurchaseLineView> Lines { get; set; } = new();
}

public sealed class InputPurchaseUpdate
{
    public int IdPurchase { get; set; }
    public int FkStore { get; set; }
    public int FkSupplier { get; set; }
    public DateOnly PurchaseDate { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public string? Notes { get; set; }
    public List<InputPurchaseLine> Lines { get; set; } = new();

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Charges { get; set; }
    public decimal RoundOff { get; set; }
    public decimal TotalAmount { get; set; }
}

public sealed class OutputPurchaseUpdate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdPurchase { get; set; }
    public bool Ok => ResponseCode == 0;
}

public sealed class InputPurchaseSelect
{
    public int IdPurchase { get; set; }
    public int FkStore { get; set; }
}

public sealed class OutputPurchaseLine
{
    public int IdPurchaseDetail { get; set; }
    public int FkProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public int? FkProductVariant { get; set; }
    public string? VariantName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxRate { get; set; }
    public bool TaxInclusive { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public decimal ReturnedQty { get; set; }
    public decimal EligibleReturnQty => Math.Max(0m, Quantity - ReturnedQty);
}

public sealed class OutputPurchaseSelect
{
    public int IdPurchase { get; set; }
    public string PurchaseNumber => $"PUR-{IdPurchase:D6}";
    public int FkStore { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int FkSupplier { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? SupplierCode { get; set; }
    public string? SupplierPhone { get; set; }
    public string? SupplierEmail { get; set; }
    public string? SupplierTaxNumber { get; set; }
    public string? SupplierAddress { get; set; }
    public DateOnly PurchaseDate { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateOnly? InvoiceDate { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal Charges { get; set; }
    public decimal RoundOff { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AmountPaid { get; set; }
    public string? PaymentMethod { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public bool Cancelled { get; set; }
    public DateTime? CancelledOn { get; set; }
    public string? CancelledReason { get; set; }

    public bool CanEdit => Status == "Pending" && !Cancelled;
    public bool CanApprove => Status == "Pending" && !Cancelled;
    public bool CanCancel => Status == "Pending" && !Cancelled;
    public bool CanReturn => Status == "Approved" && !Cancelled && Lines.Any(l => l.EligibleReturnQty > 0);

    public List<OutputPurchaseLine> Lines { get; set; } = new();

    public InPurchaseUpdateView ToUpdateView() => new()
    {
        IdPurchase = IdPurchase,
        FkSupplier = FkSupplier,
        PurchaseDate = PurchaseDate,
        InvoiceNumber = InvoiceNumber ?? string.Empty,
        InvoiceDate = InvoiceDate ?? PurchaseDate,
        Notes = Notes,
        Lines = Lines.Select(l => new InPurchaseLineView
        {
            FkProduct = l.FkProduct,
            FkProductVariant = l.FkProductVariant,
            Quantity = l.Quantity,
            UnitCost = l.UnitCost,
            DiscountAmount = l.DiscountAmount,
            TaxRate = l.TaxRate,
            TaxInclusive = l.TaxInclusive
        }).ToList()
    };
}

public sealed class InputPurchaseList
{
    public int? FkStore { get; set; }
    public int? FkSupplier { get; set; }
    public string? Status { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string SortBy { get; set; } = "purchasedate";
    public bool SortDesc { get; set; } = true;
}

public sealed class OutputPurchaseListItem
{
    public int IdPurchase { get; set; }
    public string PurchaseNumber => $"PUR-{IdPurchase:D6}";
    public int FkStore { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int FkSupplier { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public DateOnly PurchaseDate { get; set; }
    public string? InvoiceNumber { get; set; }
    public int LineCount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public bool CanEdit => Status == "Pending";
    public bool CanApprove => Status == "Pending";
    public bool CanCancel => Status == "Pending";
    public bool CanReturn => Status == "Approved";
}

public sealed class OutputPurchaseList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputPurchaseListItem> Items { get; set; } = new();
    public InputPurchaseList Filter { get; set; } = new();
}

public sealed class InPurchaseCancelView
{
    [Required]
    [GreaterThanZero]
    public int IdPurchase { get; set; }

    [Display(Name = "Reason")]
    [Required(ErrorMessage = "Please enter a reason for cancelling.")]
    [TrimmedString(2, 500, ErrorMessage = "The Reason must be between 2 and 500 characters.")]
    [NoScriptTags]
    public string Reason { get; set; } = string.Empty;
}

public sealed class InputPurchaseCancel
{
    public int IdPurchase { get; set; }
    public int FkStore { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int? CancelledBy { get; set; }
}

public sealed class OutputPurchaseCancel
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public bool Ok => ResponseCode == 0;
}

public sealed class InputPurchaseProductList
{
    public string? Search { get; set; }
    public List<int>? ProductIds { get; set; }
    public int Limit { get; set; } = 10;
}

public sealed class OutputPurchaseProductSelect
{
    public int FkProduct { get; set; }
    public int? FkProductVariant { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TaxRate { get; set; }
}

#endregion

#region Purchase Return Models

public sealed class InPurchaseReturnCreateView
{
    [Required]
    [GreaterThanZero]
    public int FkPurchase { get; set; }

    [Required]
    [GreaterThanZero]
    public int FkSupplier { get; set; }

    [Display(Name = "Return Date")]
    [Required]
    public DateOnly ReturnDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Return Reason")]
    [StringLengthValidation(0, 500)]
    [NoScriptTags]
    public string? Reason { get; set; }

    [Display(Name = "Notes")]
    [StringLengthValidation(0, 1000)]
    [NoScriptTags]
    public string? Notes { get; set; }

    public bool MarkAsApproved { get; set; }

    [NotEmptyList(ErrorMessage = "At least one item must be returned.")]
    public List<InPurchaseReturnLineView> Lines { get; set; } = new();
}

public sealed class InPurchaseReturnLineView
{
    [Required]
    [GreaterThanZero]
    public int FkPurchaseDetail { get; set; }

    [Required]
    [GreaterThanZero]
    public int FkProduct { get; set; }

    public int? FkProductVariant { get; set; }

    [Required]
    [DecimalGreaterThanZero(ErrorMessage = "Return quantity must be greater than zero.")]
    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }
}

public sealed class OutputPurchaseReturnSelect
{
    public int IdPurchaseReturn { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public int FkStore { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int FkPurchase { get; set; }
    public string PurchaseNumber => $"PUR-{FkPurchase:D6}";
    public string? InvoiceNumber { get; set; }
    public int FkSupplier { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public DateOnly ReturnDate { get; set; }
    public string Status { get; set; } = "Pending";
    public decimal TotalAmount { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PostedAt { get; set; }

    public bool CanApprove => Status == "Pending";
    public bool CanCancel => Status == "Pending";

    public List<OutputPurchaseReturnLine> Lines { get; set; } = new();
}

public sealed class OutputPurchaseReturnLine
{
    public int IdPurchaseReturnDetail { get; set; }
    public int FkPurchaseDetail { get; set; }
    public int FkProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public int? FkProductVariant { get; set; }
    public string? VariantName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class InputPurchaseReturnList
{
    public int? FkStore { get; set; }
    public int? FkSupplier { get; set; }
    public string? Status { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class OutputPurchaseReturnListItem
{
    public int IdPurchaseReturn { get; set; }
    public string ReturnNumber { get; set; } = string.Empty;
    public int FkPurchase { get; set; }
    public string PurchaseNumber => $"PUR-{FkPurchase:D6}";
    public string SupplierName { get; set; } = string.Empty;
    public DateOnly ReturnDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class OutputPurchaseReturnList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputPurchaseReturnListItem> Items { get; set; } = new();
    public InputPurchaseReturnList Filter { get; set; } = new();
}

#endregion

#region Purchase Order Models

public sealed class InPurchaseOrderView
{
    public int IdPurchaseOrder { get; set; }

    [Display(Name = "Order Date")]
    [Required]
    public DateOnly OrderDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Supplier (Optional)")]
    public int? FkSupplier { get; set; }

    [Display(Name = "Remarks / Notes")]
    [StringLengthValidation(0, 1000)]
    [NoScriptTags]
    public string? Notes { get; set; }

    [NotEmptyList(ErrorMessage = "At least one product requirement line is required.")]
    public List<InPurchaseOrderLineView> Lines { get; set; } = new();
}

public sealed class InPurchaseOrderLineView
{
    [Required]
    [GreaterThanZero]
    public int FkProduct { get; set; }

    public int? FkProductVariant { get; set; }

    [Required]
    [DecimalGreaterThanZero(ErrorMessage = "Quantity must be greater than zero.")]
    public decimal Quantity { get; set; }

    public decimal UnitCost { get; set; }
    public string? Notes { get; set; }
}

public sealed class OutputPurchaseOrderSelect
{
    public int IdPurchaseOrder { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int FkStore { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int? FkSupplier { get; set; }
    public string? SupplierName { get; set; }
    public DateOnly OrderDate { get; set; }
    public string Status { get; set; } = "Open";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatorName { get; set; }

    public List<OutputPurchaseOrderLine> Lines { get; set; } = new();

    public InPurchaseOrderView ToView() => new()
    {
        IdPurchaseOrder = IdPurchaseOrder,
        OrderDate = OrderDate,
        FkSupplier = FkSupplier,
        Notes = Notes,
        Lines = Lines.Select(l => new InPurchaseOrderLineView
        {
            FkProduct = l.FkProduct,
            FkProductVariant = l.FkProductVariant,
            Quantity = l.Quantity,
            UnitCost = l.UnitCost,
            Notes = l.Notes
        }).ToList()
    };
}

public sealed class OutputPurchaseOrderLine
{
    public int IdPurchaseOrderDetail { get; set; }
    public int FkProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public int? FkProductVariant { get; set; }
    public string? VariantName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public string? Notes { get; set; }
}

public sealed class InputPurchaseOrderList
{
    public int? FkStore { get; set; }
    public int? FkSupplier { get; set; }
    public string? Status { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public bool TodayOnly { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class OutputPurchaseOrderListItem
{
    public int IdPurchaseOrder { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string? SupplierName { get; set; }
    public DateOnly OrderDate { get; set; }
    public int LineCount { get; set; }
    public decimal TotalQuantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class OutputPurchaseOrderList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputPurchaseOrderListItem> Items { get; set; } = new();
    public InputPurchaseOrderList Filter { get; set; } = new();
}

#endregion

#region Supplier Balance & Financial Models

public sealed class OutputSupplierBalance
{
    public int IdSupplier { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    /// <summary>Positive = credit/receivable held by supplier; Negative = payable to supplier.</summary>
    public decimal CurrentBalance { get; set; }
    public decimal AvailableCredit => CurrentBalance > 0 ? CurrentBalance : 0m;
    public decimal AmountPayable => CurrentBalance < 0 ? -CurrentBalance : 0m;
    public bool HasCredit => CurrentBalance > 0;
}

public sealed class InSupplierPaymentView
{
    [Display(Name = "Supplier")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Please select a Supplier.")]
    public int FkSupplier { get; set; }

    [Display(Name = "Amount")]
    [Required]
    [DecimalGreaterThanZero(ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Display(Name = "Transaction Date")]
    [Required]
    public DateOnly TransactionDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Transaction Type")]
    [Required]
    public string TransactionType { get; set; } = "Payment"; // Payment, Refund, Opening

    [Display(Name = "Payment Method")]
    [Required]
    public string PaymentMethod { get; set; } = "Cash";

    [Display(Name = "Reference #")]
    [StringLengthValidation(0, 100)]
    public string? ReferenceNo { get; set; }

    [Display(Name = "Notes")]
    [StringLengthValidation(0, 1000)]
    public string? Notes { get; set; }
}

public sealed class OutputSupplierStatement
{
    public int IdSupplier { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string? SupplierCode { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal ClosingBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public List<OutputSupplierStatementRow> Entries { get; set; } = new();
}

public sealed class OutputSupplierStatementRow
{
    public int Id { get; set; }
    public DateOnly TransactionDate { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
    /// <summary>Increases company liability to supplier (e.g. Purchase).</summary>
    public decimal Debit { get; set; }
    /// <summary>Reduces company liability or creates advance/credit (e.g. Payment, Return).</summary>
    public decimal Credit { get; set; }
    public decimal SignedAmount { get; set; }
    public decimal RunningBalance { get; set; }
}

public sealed class InputSupplierTransactionList
{
    public int? FkSupplier { get; set; }
    public int? FkStore { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class OutputSupplierTransactionItem
{
    public int Id { get; set; }
    public string TransactionNumber { get; set; } = string.Empty;
    public int FkSupplier { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public DateOnly TransactionDate { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal SignedAmount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ReferenceNo { get; set; }
    public string? Notes { get; set; }
    public bool IsReversed { get; set; }
}

public sealed class OutputSupplierTransactionList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public decimal TotalPayments { get; set; }
    public decimal TotalRefunds { get; set; }
    public List<OutputSupplierTransactionItem> Items { get; set; } = new();
    public InputSupplierTransactionList Filter { get; set; } = new();
}

#endregion
