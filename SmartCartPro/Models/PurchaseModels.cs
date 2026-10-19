using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.Models;

// ═══════════════════════════════════════════════════════════════════
// PURCHASE
// ═══════════════════════════════════════════════════════════════════

#region Purchase Create

public sealed class InPurchaseCreateView
{
    [Required]
    [Display(Name = "Store")]
    public int FkStore { get; set; }

    [Required]
    [Display(Name = "Supplier")]
    public int FkSupplier { get; set; }

    [Required]
    [Display(Name = "Purchase date")]
    public DateOnly PurchaseDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Supplier invoice #")]
    public string? InvoiceNumber { get; set; }

    public string? Notes { get; set; }

    public List<InPurchaseLineView> Lines { get; set; } = new();
}

public sealed class InPurchaseLineView
{
    [Required]
    public int FkProduct { get; set; }

    public int? FkProductVariant { get; set; }

    [Required, Range(0.001, double.MaxValue)]
    public decimal Quantity { get; set; }

    [Required, Range(0, double.MaxValue)]
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

    public static InputPurchaseCreate From(InPurchaseCreateView v, int createdBy) => new()
    {
        FkStore = v.FkStore,
        FkSupplier = v.FkSupplier,
        PurchaseDate = v.PurchaseDate,
        InvoiceNumber = v.InvoiceNumber,
        Notes = v.Notes,
        CreatedBy = createdBy,
        Lines = v.Lines.Select(l => new InputPurchaseLine
        {
            FkProduct = l.FkProduct,
            FkProductVariant = l.FkProductVariant,
            Quantity = l.Quantity,
            UnitCost = l.UnitCost
        }).ToList()
    };
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
    public int IdPurchase { get; set; }

    [Required]
    [Display(Name = "Store")]
    public int FkStore { get; set; }

    [Required]
    [Display(Name = "Supplier")]
    public int FkSupplier { get; set; }

    [Required]
    [Display(Name = "Purchase date")]
    public DateOnly PurchaseDate { get; set; }

    [Display(Name = "Supplier invoice #")]
    public string? InvoiceNumber { get; set; }

    public string? Notes { get; set; }

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
    public int IdPurchase { get; set; }

    [Required]
    [Display(Name = "Receipt date")]
    public DateOnly ReceiptDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public string? Notes { get; set; }

    public List<InGrnLineView> Lines { get; set; } = new();
}

public sealed class InGrnLineView
{
    public int IdPurchaseDetail { get; set; }
    public int FkProduct { get; set; }
    public int? FkProductVariant { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal OrderedQty { get; set; }
    public decimal AlreadyReceivedQty { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Receive qty")]
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
    public int IdPurchase { get; set; }

    [Required, MaxLength(500)]
    [Display(Name = "Reason")]
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
