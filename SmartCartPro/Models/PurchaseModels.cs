using System.ComponentModel.DataAnnotations;
using SmartCartPro.CustomModelValidation;

namespace SmartCartPro.Models;

// ═══════════════════════════════════════════════════════════════════
// PURCHASE
// ═══════════════════════════════════════════════════════════════════

#region Purchase Create

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

    [Display(Name = "Notes")]
    [StringLengthValidation(0, 1000, ErrorMessage = "The Notes cannot exceed 1000 characters.")]
    [NoScriptTags(ErrorMessage = "The Notes cannot contain script tags.")]
    public string? Notes { get; set; }

    /// <summary>true = keep as draft (not receivable yet); false = post the purchase order.</summary>
    [Display(Name = "Save as draft")]
    public bool SaveAsDraft { get; set; }

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

    [Display(Name = "Discount")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Discount must be greater than or equal to zero.")]
    public decimal DiscountAmount { get; set; }
}

public sealed class InputPurchaseCreate
{
    /// <summary>Always the signed-in user's store (never taken from the client).</summary>
    public int FkStore { get; set; }
    public int FkSupplier { get; set; }
    public DateOnly PurchaseDate { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? Notes { get; set; }
    public bool SaveAsDraft { get; set; }
    public int? CreatedBy { get; set; }
    public List<InputPurchaseLine> Lines { get; set; } = new();

    // Recomputed by PurchaseService from authoritative data — values supplied by the controller are ignored.
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
}

public sealed class InputPurchaseLine
{
    public int FkProduct { get; set; }
    public int? FkProductVariant { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal DiscountAmount { get; set; }

    // Recomputed by PurchaseService (tax comes from the product's tax profile, never from the client).
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

#endregion

#region Purchase Update

public sealed class InPurchaseUpdateView
{
    [Display(Name = "Purchase")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Purchase.")]
    public int IdPurchase { get; set; }

    [Display(Name = "Supplier")]
    [Required(ErrorMessage = "Please select a Supplier.")]
    [GreaterThanZero(ErrorMessage = "Please select a Supplier.")]
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

    /// <summary>true = keep as draft; false = post. A posted purchase order cannot go back to draft.</summary>
    [Display(Name = "Save as draft")]
    public bool SaveAsDraft { get; set; }

    [Display(Name = "Lines")]
    [NotEmptyList(ErrorMessage = "At least one purchase line is required.")]
    public List<InPurchaseLineView> Lines { get; set; } = new();
}

public sealed class InputPurchaseUpdate
{
    public int IdPurchase { get; set; }
    /// <summary>Always the signed-in user's store (never taken from the client).</summary>
    public int FkStore { get; set; }
    public int FkSupplier { get; set; }
    public DateOnly PurchaseDate { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? Notes { get; set; }
    public bool SaveAsDraft { get; set; }
    public List<InputPurchaseLine> Lines { get; set; } = new();

    // Recomputed by PurchaseService from authoritative data — values supplied by the controller are ignored.
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
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

#endregion

#region Purchase Select

public sealed class InputPurchaseSelect
{
    public int IdPurchase { get; set; }
    /// <summary>Store scope — a purchase order of another store is reported as not found.</summary>
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
    /// <summary>Quantity received so far, derived from the stock movements posted for this purchase order.</summary>
    public decimal ReceivedQty { get; set; }
    public decimal UnitCost { get; set; }
    /// <summary>Line discount derived from the stored amounts (gross + tax − line total).</summary>
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }

    public decimal PendingQty => Math.Max(0m, Quantity - ReceivedQty);
    public decimal GrossAmount => Math.Round(Quantity * UnitCost, 2, MidpointRounding.AwayFromZero);
}

public sealed class OutputPurchaseReceipt
{
    public int IdStockMovement { get; set; }
    /// <summary>UTC timestamp as stored.</summary>
    public DateTime ReceivedAt { get; set; }
    public int FkProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public decimal Quantity { get; set; }
    /// <summary>Holds the GRN number, receipt date and the user's notes.</summary>
    public string? Notes { get; set; }
}

public sealed class OutputPurchaseSelect
{
    public int IdPurchase { get; set; }
    /// <summary>Display number derived from the id — no separate PO sequence is stored.</summary>
    public string PoNumber => $"PO-{IdPurchase:D6}";
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
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool Cancelled { get; set; }
    public DateTime? CancelledOn { get; set; }
    public string? CancelledReason { get; set; }

    // Set by PurchaseService — the single source of truth for which actions are allowed.
    public bool CanEdit { get; set; }
    public bool CanReceive { get; set; }
    public bool CanCancel { get; set; }

    public List<OutputPurchaseLine> Lines { get; set; } = new();
    public List<OutputPurchaseReceipt> Receipts { get; set; } = new();

    public decimal TotalOrderedQty => Lines.Sum(l => l.Quantity);
    public decimal TotalReceivedQty => Lines.Sum(l => l.ReceivedQty);

    public InPurchaseUpdateView ToUpdateView() => new()
    {
        IdPurchase = IdPurchase,
        FkSupplier = FkSupplier,
        PurchaseDate = PurchaseDate,
        InvoiceNumber = InvoiceNumber,
        Notes = Notes,
        SaveAsDraft = Status == "draft",
        Lines = Lines.Select(l => new InPurchaseLineView
        {
            FkProduct = l.FkProduct,
            FkProductVariant = l.FkProductVariant,
            Quantity = l.Quantity,
            UnitCost = l.UnitCost,
            DiscountAmount = l.DiscountAmount
        }).ToList()
    };
}

#endregion

#region Purchase List

public sealed class InputPurchaseList
{
    /// <summary>Store scope — set by the controller from the signed-in user's claim.</summary>
    public int? FkStore { get; set; }
    public int? FkSupplier { get; set; }
    /// <summary>draft | posted | partially_received | received | cancelled (empty = all).</summary>
    public string? Status { get; set; }
    /// <summary>First purchase date to include.</summary>
    public DateOnly? From { get; set; }
    /// <summary>Last purchase date to include.</summary>
    public DateOnly? To { get; set; }
    /// <summary>Matches supplier invoice number, supplier name or a PO number such as PO-000123.</summary>
    public string? Search { get; set; }
    /// <summary>Prepared by the service when <see cref="Search"/> looks like a PO number.</summary>
    public int? SearchId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    /// <summary>id | purchasedate | supplier | invoicenumber | totalamount | status</summary>
    public string SortBy { get; set; } = "purchasedate";
    public bool SortDesc { get; set; } = true;
}

public sealed class OutputPurchaseListItem
{
    public int IdPurchase { get; set; }
    public string PoNumber => $"PO-{IdPurchase:D6}";
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

    // Set by PurchaseService.
    public bool CanEdit { get; set; }
    public bool CanReceive { get; set; }
    public bool CanCancel { get; set; }
}

public sealed class OutputPurchaseList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputPurchaseListItem> Items { get; set; } = new();
    /// <summary>Normalised filter echoed back so the view can repopulate its form and build pager links.</summary>
    public InputPurchaseList Filter { get; set; } = new();
}

#endregion

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
    [StringLengthValidation(0, 500, ErrorMessage = "The Notes cannot exceed 500 characters.")]
    [NoScriptTags(ErrorMessage = "The Notes cannot contain script tags.")]
    public string? Notes { get; set; }

    [Display(Name = "Lines")]
    [NotEmptyList(ErrorMessage = "At least one line is required.")]
    public List<InGrnLineView> Lines { get; set; } = new();
}

public sealed class InGrnLineView
{
    [Display(Name = "Purchase Line")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Purchase Line.")]
    public int IdPurchaseDetail { get; set; }

    [Display(Name = "Receive Quantity")]
    [Required]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Receive Quantity must be greater than or equal to zero.")]
    public decimal ReceiveQty { get; set; }
}

public sealed class InputGoodsReceipt
{
    public int IdPurchase { get; set; }
    /// <summary>Always the signed-in user's store (never taken from the client).</summary>
    public int FkStore { get; set; }
    public DateOnly ReceiptDate { get; set; }
    public string? Notes { get; set; }
    public int? CreatedBy { get; set; }
    public List<InputGrnLine> Lines { get; set; } = new();

    // Prepared by PurchaseService.
    public string GrnNumber { get; set; } = string.Empty;
    /// <summary>Status the purchase order moves to once this receipt is posted.</summary>
    public string NewStatus { get; set; } = string.Empty;
}

public sealed class InputGrnLine
{
    public int IdPurchaseDetail { get; set; }
    public decimal ReceiveQty { get; set; }

    // Resolved by PurchaseService from the purchase order line (the client never supplies products).
    public int FkProduct { get; set; }
    public int? FkProductVariant { get; set; }
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

#region Purchase Cancel

public sealed class InPurchaseCancelView
{
    [Display(Name = "Purchase")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Purchase.")]
    public int IdPurchase { get; set; }

    [Display(Name = "Reason")]
    [Required(ErrorMessage = "Please enter a reason for cancelling.")]
    [TrimmedString(2, 500, ErrorMessage = "The Reason must be between 2 and 500 characters.")]
    [NoScriptTags(ErrorMessage = "The Reason cannot contain script tags.")]
    public string Reason { get; set; } = string.Empty;
}

public sealed class InputPurchaseCancel
{
    public int IdPurchase { get; set; }
    /// <summary>Always the signed-in user's store (never taken from the client).</summary>
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

#endregion

#region Purchase Product Lookup

public sealed class InputPurchaseProductList
{
    /// <summary>Free-text search on product name, code, SKU or barcode (variants included).</summary>
    public string? Search { get; set; }
    /// <summary>When set, returns exactly these products (used by PurchaseService to validate lines).</summary>
    public List<int>? ProductIds { get; set; }
    /// <summary>Maximum number of products (each product may add its variant rows).</summary>
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
    /// <summary>Suggested unit cost — the product's current cost price.</summary>
    public decimal UnitCost { get; set; }
    /// <summary>Percentage from the product's tax profile (0 when none).</summary>
    public decimal TaxRate { get; set; }
}

#endregion
