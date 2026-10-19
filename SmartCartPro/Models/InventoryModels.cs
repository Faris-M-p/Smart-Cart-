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
    /// <summary>Always positive. Direction determined by movement type (adjustment is the only signed type).</summary>
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
    public string? CategoryName { get; set; }
    public string? UnitCode { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    /// <summary>Effective reorder level: the store row's own level when set (above zero), otherwise the product's level.</summary>
    public decimal ReorderLevel { get; set; }
    /// <summary>Filled by the stock service on the low-stock screen only (0 elsewhere). Blank when no reorder level is set.</summary>
    public decimal SuggestedOrderQty { get; set; }
    public bool BelowReorder => QuantityOnHand <= ReorderLevel;
    public decimal AvailableQuantity => QuantityOnHand - QuantityReserved;
    /// <summary>"out" (zero or less), "low" (above zero but at/below the reorder level) or "ok".</summary>
    public string StockStatus => QuantityOnHand <= 0 ? "out" : QuantityOnHand <= ReorderLevel ? "low" : "ok";
}

#endregion

#region Inventory List

public sealed class InputInventoryList
{
    public int FkStore { get; set; }
    public string? Search { get; set; }
    public int? FkCategory { get; set; }
    /// <summary>Legacy flag: true = only rows at or below the reorder level (low + out). Prefer <see cref="StockStatus"/>.</summary>
    public bool BelowReorderOnly { get; set; }
    /// <summary>"" = all, "low" = above zero but at/below the reorder level, "out" = zero or less.</summary>
    public string? StockStatus { get; set; }
    /// <summary>productname | productcode | quantityonhand | reorderlevel</summary>
    public string SortBy { get; set; } = "productname";
    public bool SortDesc { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class OutputInventoryList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputInventorySelect> Items { get; set; } = new();
    /// <summary>Rows in the current search/category scope, ignoring the status filter (used for the "All" chip).</summary>
    public int ScopeCount { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public List<OutputStockLookupItem> Categories { get; set; } = new();
    /// <summary>Normalised filter echoed back so the view can repopulate its form and build links.</summary>
    public InputInventoryList Filter { get; set; } = new();
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
    [GreaterThanZero(ErrorMessage = "Please select a product from the search list.")]
    public int FkProduct { get; set; }

    [Display(Name = "Variant")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Variant.")]
    public int? FkProductVariant { get; set; }

    [Display(Name = "Opening Qty")]
    [Required]
    [DecimalGreaterThanZero(ErrorMessage = "The Opening Qty must be greater than zero.")]
    [DecimalLength(11, 3, ErrorMessage = "The Opening Qty can have at most 11 digits before and 3 digits after the decimal point.")]
    public decimal Quantity { get; set; }

    [Display(Name = "Notes")]
    [StringLengthValidation(0, 500, ErrorMessage = "The Notes cannot exceed 500 characters.")]
    [NoScriptTags(ErrorMessage = "The Notes cannot contain script tags.")]
    public string? Notes { get; set; }
}

/// <summary>Controller → Service: first stock entry for one SKU at one store.</summary>
public sealed class InputStockOpening
{
    public int FkStore { get; set; }
    public int FkProduct { get; set; }
    public int? FkProductVariant { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
    public int? CreatedBy { get; set; }
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// ADJUSTMENT
// ═══════════════════════════════════════════════════════════════════

#region Adjustment

public sealed class InStockAdjustView
{
    /// <summary>Allowed adjustment reasons; also the dropdown options on the adjust screen.</summary>
    public static readonly string[] ReasonOptions =
    {
        "Damaged", "Expired", "Lost or stolen", "Found stock", "Count correction", "Other"
    };

    [Display(Name = "Store")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Store.")]
    public int FkStore { get; set; }

    [Display(Name = "Product")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Please select a product from the search list.")]
    public int FkProduct { get; set; }

    [Display(Name = "Variant")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Variant.")]
    public int? FkProductVariant { get; set; }

    [Display(Name = "Quantity")]
    [Required]
    [DecimalGreaterThanZero(ErrorMessage = "The Quantity must be greater than zero.")]
    [DecimalLength(11, 3, ErrorMessage = "The Quantity can have at most 11 digits before and 3 digits after the decimal point.")]
    public decimal Quantity { get; set; }

    [Display(Name = "Direction")]
    [Required(ErrorMessage = "The Direction field is required.")]
    [AllowedValues("add", "remove", ErrorMessage = "Something went wrong/Please select a valid Direction.")]
    public string Direction { get; set; } = "add";

    [Display(Name = "Reason")]
    [Required(ErrorMessage = "Please choose a reason.")]
    [AllowedValues("Damaged", "Expired", "Lost or stolen", "Found stock", "Count correction", "Other",
        ErrorMessage = "Please choose a reason from the list.")]
    public string Reason { get; set; } = string.Empty;

    [Display(Name = "Notes")]
    [StringLengthValidation(0, 500, ErrorMessage = "The Notes cannot exceed 500 characters.")]
    [NoScriptTags(ErrorMessage = "The Notes cannot contain script tags.")]
    public string? Notes { get; set; }
}

/// <summary>Controller → Service: add or remove stock for one SKU at one store.</summary>
public sealed class InputStockAdjust
{
    public int FkStore { get; set; }
    public int FkProduct { get; set; }
    public int? FkProductVariant { get; set; }
    /// <summary>Always positive; <see cref="Direction"/> decides the sign.</summary>
    public decimal Quantity { get; set; }
    /// <summary>"add" or "remove".</summary>
    public string Direction { get; set; } = "add";
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int? CreatedBy { get; set; }
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// TRANSFER
// ═══════════════════════════════════════════════════════════════════

#region Transfer

/// <summary>Document-style transfer form: destination store plus one or more product lines (posted as JSON).</summary>
public sealed class InStockTransferCreateView
{
    [Display(Name = "To Store")]
    [Required(ErrorMessage = "Please choose the destination store.")]
    [GreaterThanZero(ErrorMessage = "Please choose the destination store.")]
    public int ToStoreId { get; set; }

    [Display(Name = "Notes")]
    [StringLengthValidation(0, 500, ErrorMessage = "The Notes cannot exceed 500 characters.")]
    [NoScriptTags(ErrorMessage = "The Notes cannot contain script tags.")]
    public string? Notes { get; set; }

    [Display(Name = "Items")]
    [NotEmptyList(ErrorMessage = "Add at least one product to transfer.")]
    public List<InStockTransferLineView> Lines { get; set; } = new();
}

public sealed class InStockTransferLineView
{
    [Display(Name = "Product")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Please select a product on every line.")]
    public int FkProduct { get; set; }

    [Display(Name = "Variant")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Variant.")]
    public int? FkProductVariant { get; set; }

    [Display(Name = "Quantity")]
    [Required]
    [DecimalGreaterThanZero(ErrorMessage = "The Quantity must be greater than zero on every line.")]
    [DecimalLength(11, 3, ErrorMessage = "The Quantity can have at most 11 digits before and 3 digits after the decimal point.")]
    public decimal Quantity { get; set; }
}

/// <summary>Single-line transfer (kept for existing callers). New code should use <see cref="InputStockTransferCreate"/>.</summary>
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

/// <summary>Controller → Service → Repository: multi-line transfer applied in ONE database transaction.</summary>
public sealed class InputStockTransferCreate
{
    public int FromStoreId { get; set; }
    public int ToStoreId { get; set; }
    public string? Notes { get; set; }
    public int? CreatedBy { get; set; }
    public List<InputStockTransferLine> Lines { get; set; } = new();
}

public sealed class InputStockTransferLine
{
    public int FkProduct { get; set; }
    public int? FkProductVariant { get; set; }
    public decimal Quantity { get; set; }
}

public sealed class OutputStockTransferCreate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int LineCount { get; set; }
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
    /// <summary>First day to include (the user's local calendar day).</summary>
    public DateTime? From { get; set; }
    /// <summary>Last day to include (the user's local calendar day, whole day).</summary>
    public DateTime? To { get; set; }
    /// <summary>Browser UTC offset in minutes (JavaScript <c>getTimezoneOffset()</c>) so day filters follow the user's local day.</summary>
    public int TzOffsetMinutes { get; set; }
    /// <summary>Prepared by the stock service from <see cref="From"/> + <see cref="TzOffsetMinutes"/> (UTC, inclusive).</summary>
    public DateTime? FromUtc { get; set; }
    /// <summary>Prepared by the stock service from <see cref="To"/> + <see cref="TzOffsetMinutes"/> (UTC, exclusive).</summary>
    public DateTime? ToUtcExclusive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class OutputStockMovementItem
{
    public int IdStockMovement { get; set; }
    public int FkStore { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int FkProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public int? FkProductVariant { get; set; }
    public string? VariantName { get; set; }
    public string MovementType { get; set; } = string.Empty;
    /// <summary>Stored value: positive for every type except <c>adjustment</c>, which is signed.</summary>
    public decimal Quantity { get; set; }
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public string? Notes { get; set; }
    /// <summary>UTC timestamp as stored.</summary>
    public DateTime CreatedAt { get; set; }
    public string? CreatedByName { get; set; }
    /// <summary>Effect on the balance: negative for sale / transfer_out / return_out, positive for the rest.</summary>
    public decimal SignedQuantity
    {
        get
        {
            if (MovementType == "sale" || MovementType == "transfer_out" || MovementType == "return_out")
                return -Math.Abs(Quantity);

            return MovementType == "adjustment" ? Quantity : Math.Abs(Quantity);
        }
    }
}

public sealed class OutputStockMovementList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputStockMovementItem> Items { get; set; } = new();
    /// <summary>Normalised filter echoed back so the view can repopulate its form and build pager links.</summary>
    public InputStockMovementList Filter { get; set; } = new();
    /// <summary>Name of the product filter (for the picker text box), when a product filter is active.</summary>
    public string? FilterProductName { get; set; }
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
    public string? Search { get; set; }
    /// <summary>productname | quantityonhand | reorderlevel | shortage</summary>
    public string SortBy { get; set; } = "quantityonhand";
    public bool SortDesc { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class OutputLowStockList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputInventorySelect> Items { get; set; } = new();
    public List<OutputStockLookupItem> Categories { get; set; } = new();
    /// <summary>Normalised filter echoed back so the view can repopulate its form and build links.</summary>
    public InputLowStockList Filter { get; set; } = new();
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// PRODUCT PICKER (typeahead JSON)
// ═══════════════════════════════════════════════════════════════════

#region Product Search

public sealed class InputStockProductSearch
{
    public int FkStore { get; set; }
    public string? Term { get; set; }
    public int Limit { get; set; } = 15;
}

/// <summary>One sellable SKU (a product, or one variant of a product) with the quantity on hand at the searching store.</summary>
public sealed class OutputStockProductSearchItem
{
    public int IdProduct { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int? IdProductVariant { get; set; }
    public string? VariantName { get; set; }
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public string? UnitCode { get; set; }
    public decimal QuantityOnHand { get; set; }
    public bool IsActive { get; set; }
}

public sealed class OutputStockProductSearch
{
    public List<OutputStockProductSearchItem> Items { get; set; } = new();
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// LOOKUPS (category filter, destination stores)
// ═══════════════════════════════════════════════════════════════════

#region Lookups

public sealed class OutputStockLookupItem
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
}

#endregion
