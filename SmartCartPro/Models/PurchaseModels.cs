using System.ComponentModel.DataAnnotations;
using SmartCartPro.CustomModelValidation;

namespace SmartCartPro.Models;

// ═══════════════════════════════════════════════════════════════════
// PURCHASE
// ═══════════════════════════════════════════════════════════════════

#region Purchase Create

public sealed class InPurchaseCreateView
{
    [Display(Name = "Store")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Store.")]
    public int FkStore { get; set; }

    [Display(Name = "Supplier")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Supplier.")]
    public int FkSupplier { get; set; }

    [Display(Name = "Purchase Date")]
    [Required(ErrorMessage = "The Purchase Date field is required.")]
    public DateOnly PurchaseDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Supplier Invoice #")]
    [StringLengthValidation(0, 100, ErrorMessage = "The Supplier Invoice # cannot exceed 100 characters.")]
    [NoScriptTags(ErrorMessage = "The Supplier Invoice # cannot contain script tags.")]
    public string? InvoiceNumber { get; set; }

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
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Product.")]
    public int FkProduct { get; set; }

    [Display(Name = "Variant")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Variant.")]
    public int? FkProductVariant { get; set; }

    [Display(Name = "Quantity")]
    [Required]
    [DecimalGreaterThanZero(ErrorMessage = "The Quantity must be greater than zero.")]
    public decimal Quantity { get; set; }

    [Display(Name = "Unit Cost")]
    [Required]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Unit Cost must be greater than or equal to zero.")]
    public decimal UnitCost { get; set; }
}

public sealed class InputPurchaseCreate
{
    public int FkStore { get; set; }
    public int FkSupplier { get; set; }
    public DateOnly PurchaseDate { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? Notes { get; set; }
    public int? CreatedBy { get; set; }
    public List<InputPurchaseLine> Lines { get; set; } = new();
}

public sealed class InputPurchaseLine
{
    public int FkProduct { get; set; }
    public int? FkProductVariant { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
}

public sealed class OutputPurchaseCreate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdPurchase { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Purchase Update

public sealed class InPurchaseUpdateView
{
    [Display(Name = "Purchase")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Purchase.")]
    public int IdPurchase { get; set; }

    [Display(Name = "Store")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Store.")]
    public int FkStore { get; set; }

    [Display(Name = "Supplier")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Supplier.")]
    public int FkSupplier { get; set; }

    [Display(Name = "Purchase Date")]
    [Required(ErrorMessage = "The Purchase Date field is required.")]
    public DateOnly PurchaseDate { get; set; }

    [Display(Name = "Supplier Invoice #")]
    [StringLengthValidation(0, 100, ErrorMessage = "The Supplier Invoice # cannot exceed 100 characters.")]
    [NoScriptTags(ErrorMessage = "The Supplier Invoice # cannot contain script tags.")]
    public string? InvoiceNumber { get; set; }

    [Display(Name = "Notes")]
    [StringLengthValidation(0, 1000, ErrorMessage = "The Notes cannot exceed 1000 characters.")]
    [NoScriptTags(ErrorMessage = "The Notes cannot contain script tags.")]
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
    public string? Notes { get; set; }
    public List<InputPurchaseLine> Lines { get; set; } = new();
}

public sealed class OutputPurchaseUpdate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdPurchase { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Purchase Select

public sealed class InputPurchaseSelect { public int IdPurchase { get; set; } }

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
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class OutputPurchaseSelect
{
    public int IdPurchase { get; set; }
    public int FkStore { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int FkSupplier { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public DateOnly PurchaseDate { get; set; }
    public string? InvoiceNumber { get; set; }
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OutputPurchaseLine> Lines { get; set; } = new();
}

#endregion

#region Purchase List

public sealed class InputPurchaseList
{
    public int? FkStore { get; set; }
    public int? FkSupplier { get; set; }
    public string? Status { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string SortBy { get; set; } = "createdat";
    public bool SortDesc { get; set; } = true;
}

public sealed class OutputPurchaseListItem
{
    public int IdPurchase { get; set; }
    public int FkStore { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public DateOnly PurchaseDate { get; set; }
    public string? InvoiceNumber { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class OutputPurchaseList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputPurchaseListItem> Items { get; set; } = new();
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// GRN — GOODS RECEIPT
// ═══════════════════════════════════════════════════════════════════

#region Goods Receipt

public sealed class InGoodsReceiptView
{
    [Display(Name = "Purchase")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Purchase.")]
    public int IdPurchase { get; set; }

    [Display(Name = "Receipt Date")]
    [Required(ErrorMessage = "The Receipt Date field is required.")]
    public DateOnly ReceiptDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Notes")]
    [StringLengthValidation(0, 1000, ErrorMessage = "The Notes cannot exceed 1000 characters.")]
    [NoScriptTags(ErrorMessage = "The Notes cannot contain script tags.")]
    public string? Notes { get; set; }

    [Display(Name = "Lines")]
    public List<InGrnLineView> Lines { get; set; } = new();
}

public sealed class InGrnLineView
{
    [Display(Name = "Purchase Detail")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Purchase Detail.")]
    public int IdPurchaseDetail { get; set; }

    [Display(Name = "Product")]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Product.")]
    public int FkProduct { get; set; }

    [Display(Name = "Variant")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Variant.")]
    public int? FkProductVariant { get; set; }

    [Display(Name = "Product Name")]
    public string ProductName { get; set; } = string.Empty;

    [Display(Name = "Ordered Qty")]
    public decimal OrderedQty { get; set; }

    [Display(Name = "Already Received Qty")]
    public decimal AlreadyReceivedQty { get; set; }

    [Display(Name = "Receive Qty")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Receive Qty must be greater than or equal to zero.")]
    public decimal ReceiveQty { get; set; }
}

public sealed class InputGoodsReceipt
{
    public int IdPurchase { get; set; }
    public int FkStore { get; set; }
    public DateOnly ReceiptDate { get; set; }
    public string? Notes { get; set; }
    public int CreatedBy { get; set; }
    public List<InputGrnLine> Lines { get; set; } = new();
}

public sealed class InputGrnLine
{
    public int IdPurchaseDetail { get; set; }
    public int FkProduct { get; set; }
    public int? FkProductVariant { get; set; }
    public decimal ReceiveQty { get; set; }
}

public sealed class OutputGoodsReceipt
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string GrnNumber { get; set; } = string.Empty;
    public string NewPurchaseStatus { get; set; } = string.Empty;
    public bool Ok => ResponseCode == 0;
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// PURCHASE CANCEL
// ═══════════════════════════════════════════════════════════════════

#region Purchase Cancel

public sealed class InPurchaseCancelView
{
    [Display(Name = "Purchase")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Purchase.")]
    public int IdPurchase { get; set; }

    [Display(Name = "Reason")]
    [Required(ErrorMessage = "The Reason field is required.")]
    [StringLengthValidation(2, 500, ErrorMessage = "The Reason must be between 2 and 500 characters.")]
    [NoScriptTags(ErrorMessage = "The Reason cannot contain script tags.")]
    public string Reason { get; set; } = string.Empty;
}

public sealed class InputPurchaseCancel
{
    public int IdPurchase { get; set; }
    public string Reason { get; set; } = string.Empty;
    public int CancelledBy { get; set; }
}

public sealed class OutputPurchaseCancel
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion
