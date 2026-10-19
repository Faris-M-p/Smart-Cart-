using System.ComponentModel.DataAnnotations;
using SmartCartPro.CustomModelValidation;

namespace SmartCartPro.Models;

// ═══════════════════════════════════════════════════════════════════
// STOCK MOVEMENT (central write path)
// ═══════════════════════════════════════════════════════════════════

#region Stock Movement

/// <summary>
/// Passed to <c>IStockRepository.ApplyMovementAsync</c> — the single entry point for all stock changes.
/// MovementType must match DB CHECK: purchase|sale|adjustment|transfer_in|transfer_out|return_in|return_out|opening
/// </summary>
public sealed class InputStockMovement
{
    public int FkStore { get; set; }
    public int FkProduct { get; set; }
    public int? FkProductVariant { get; set; }
    public string MovementType { get; set; } = string.Empty;
    /// <summary>Always positive. Direction determined by movement type.</summary>
    public decimal Quantity { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public string? Notes { get; set; }
    public int? CreatedBy { get; set; }
}

public sealed class OutputStockMovement
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdStockMovement { get; set; }
    public decimal NewQuantityOnHand { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// INVENTORY BALANCE
// ═══════════════════════════════════════════════════════════════════

#region Inventory Select

public sealed class InputInventorySelect
{
    public int FkStore { get; set; }
    public int FkProduct { get; set; }
    public int? FkProductVariant { get; set; }
}

public sealed class OutputInventorySelect
{
    public int IdInventory { get; set; }
    public int FkStore { get; set; }
    public int FkProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public int? FkProductVariant { get; set; }
    public string? VariantName { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal ReorderLevel { get; set; }
    public bool BelowReorder => QuantityOnHand <= ReorderLevel;
}

#endregion

#region Inventory List

public sealed class InputInventoryList
{
    public int FkStore { get; set; }
    public string? Search { get; set; }
    public int? FkCategory { get; set; }
    public bool BelowReorderOnly { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed class OutputInventoryList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputInventorySelect> Items { get; set; } = new();
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// OPENING STOCK
// ═══════════════════════════════════════════════════════════════════

#region Opening Stock

public sealed class InOpeningStockView
{
    [Display(Name = "Store")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Store.")]
    public int FkStore { get; set; }

    [Display(Name = "Product")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Product.")]
    public int FkProduct { get; set; }

    [Display(Name = "Variant")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Variant.")]
    public int? FkProductVariant { get; set; }

    [Display(Name = "Opening Qty")]
    [Required]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Opening Qty must be greater than or equal to zero.")]
    public decimal Quantity { get; set; }

    [Display(Name = "Notes")]
    [StringLengthValidation(0, 500, ErrorMessage = "The Notes cannot exceed 500 characters.")]
    [NoScriptTags(ErrorMessage = "The Notes cannot contain script tags.")]
    public string? Notes { get; set; }
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// ADJUSTMENT
// ═══════════════════════════════════════════════════════════════════

#region Adjustment

public sealed class InStockAdjustView
{
    [Display(Name = "Store")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Store.")]
    public int FkStore { get; set; }

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

    [Display(Name = "Direction")]
    [Required(ErrorMessage = "The Direction field is required.")]
    [AllowedValues("add", "remove", ErrorMessage = "Something went wrong/Please select a valid Direction.")]
    public string Direction { get; set; } = "add";

    [Display(Name = "Notes")]
    [StringLengthValidation(0, 500, ErrorMessage = "The Notes cannot exceed 500 characters.")]
    [NoScriptTags(ErrorMessage = "The Notes cannot contain script tags.")]
    public string? Notes { get; set; }
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// TRANSFER
// ═══════════════════════════════════════════════════════════════════

#region Transfer

public sealed class InStockTransferView
{
    [Display(Name = "From Store")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid From Store.")]
    public int FromStoreId { get; set; }

    [Display(Name = "To Store")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid To Store.")]
    public int ToStoreId { get; set; }

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

    [Display(Name = "Notes")]
    [StringLengthValidation(0, 500, ErrorMessage = "The Notes cannot exceed 500 characters.")]
    [NoScriptTags(ErrorMessage = "The Notes cannot contain script tags.")]
    public string? Notes { get; set; }
}

public sealed class InputStockTransfer
{
    public int FromStoreId { get; set; }
    public int ToStoreId { get; set; }
    public int FkProduct { get; set; }
    public int? FkProductVariant { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
    public int? CreatedBy { get; set; }
}

public sealed class OutputStockTransfer
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// MOVEMENT HISTORY
// ═══════════════════════════════════════════════════════════════════

#region Movement History

public sealed class InputStockMovementList
{
    public int? FkStore { get; set; }
    public int? FkProduct { get; set; }
    public string? MovementType { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed class OutputStockMovementItem
{
    public int IdStockMovement { get; set; }
    public int FkStore { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int FkProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public string MovementType { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class OutputStockMovementList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputStockMovementItem> Items { get; set; } = new();
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// LOW STOCK
// ═══════════════════════════════════════════════════════════════════

#region Low Stock

public sealed class InputLowStockList
{
    public int FkStore { get; set; }
    public int? FkCategory { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed class OutputLowStockList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputInventorySelect> Items { get; set; } = new();
}

#endregion
