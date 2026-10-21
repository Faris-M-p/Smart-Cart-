using System.ComponentModel.DataAnnotations;
using SmartCartPro.CustomModelValidation;

namespace SmartCartPro.Models;

// ═══════════════════════════════════════════════════════════════════
// PRODUCT
// ═══════════════════════════════════════════════════════════════════

#region Product Create

public sealed class InProductCreateView
{
    [Display(Name = "Product Code")]
    [Required(ErrorMessage = "The Product Code field is required.")]
    [StringLengthValidation(1, 50, ErrorMessage = "The Product Code must be between 1 and 50 characters.")]
    [NoScriptTags(ErrorMessage = "The Product Code cannot contain script tags.")]
    public string ProductCode { get; set; } = string.Empty;

    [Display(Name = "Product Name")]
    [Required(ErrorMessage = "The Product Name field is required.")]
    [TrimmedString(2, 300, ErrorMessage = "The Product Name must be between 2 and 300 characters.")]
    [NoScriptTags(ErrorMessage = "The Product Name cannot contain script tags.")]
    public string ProductName { get; set; } = string.Empty;

    [Display(Name = "Description")]
    [StringLengthValidation(0, 2000, ErrorMessage = "The Description cannot exceed 2000 characters.")]
    [NoScriptTags(ErrorMessage = "The Description cannot contain script tags.")]
    public string? Description { get; set; }

    [Display(Name = "SKU")]
    [StringLengthValidation(0, 50, ErrorMessage = "The SKU cannot exceed 50 characters.")]
    [NoScriptTags(ErrorMessage = "The SKU cannot contain script tags.")]
    public string? Sku { get; set; }

    [Display(Name = "Barcode")]
    [StringLengthValidation(0, 50, ErrorMessage = "The Barcode cannot exceed 50 characters.")]
    [NoScriptTags(ErrorMessage = "The Barcode cannot contain script tags.")]
    public string? Barcode { get; set; }

    [Display(Name = "Category")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Category.")]
    public int? FkCategory { get; set; }

    [Display(Name = "Sub-Category")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Sub-Category.")]
    public int? FkSubcategory { get; set; }

    [Display(Name = "Brand")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Brand.")]
    public int? FkBrand { get; set; }

    [Display(Name = "Unit")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Unit.")]
    public int? FkUnit { get; set; }

    [Display(Name = "Tax")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Tax.")]
    public int? FkTax { get; set; }

    [Display(Name = "Cost Price")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Cost Price must be greater than or equal to zero.")]
    [DecimalLength(10, 2, ErrorMessage = "The Cost Price can have up to 10 digits before and 2 after the decimal point.")]
    public decimal CostPrice { get; set; }

    [Display(Name = "Selling Price")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Selling Price must be greater than or equal to zero.")]
    [DecimalLength(10, 2, ErrorMessage = "The Selling Price can have up to 10 digits before and 2 after the decimal point.")]
    public decimal SellingPrice { get; set; }

    [Display(Name = "MRP")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The MRP must be greater than or equal to zero.")]
    [DecimalLength(10, 2, ErrorMessage = "The MRP can have up to 10 digits before and 2 after the decimal point.")]
    public decimal? Mrp { get; set; }

    [Display(Name = "Reorder Level")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Reorder Level must be greater than or equal to zero.")]
    [DecimalLength(9, 3, ErrorMessage = "The Reorder Level can have up to 9 digits before and 3 after the decimal point.")]
    public decimal ReorderLevel { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Available Online")]
    public bool IsOnline { get; set; }
}

public sealed class InputProductCreate
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public int? FkCategory { get; set; }
    public int? FkSubcategory { get; set; }
    public int? FkBrand { get; set; }
    public int? FkUnit { get; set; }
    public int? FkTax { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal? Mrp { get; set; }
    public decimal ReorderLevel { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsOnline { get; set; }
}

public sealed class OutputProductCreate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdProduct { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Product Update

public sealed class InProductUpdateView
{
    [Display(Name = "Product")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Product.")]
    public int IdProduct { get; set; }

    [Display(Name = "Product Code")]
    [Required(ErrorMessage = "The Product Code field is required.")]
    [StringLengthValidation(1, 50, ErrorMessage = "The Product Code must be between 1 and 50 characters.")]
    [NoScriptTags(ErrorMessage = "The Product Code cannot contain script tags.")]
    public string ProductCode { get; set; } = string.Empty;

    [Display(Name = "Product Name")]
    [Required(ErrorMessage = "The Product Name field is required.")]
    [TrimmedString(2, 300, ErrorMessage = "The Product Name must be between 2 and 300 characters.")]
    [NoScriptTags(ErrorMessage = "The Product Name cannot contain script tags.")]
    public string ProductName { get; set; } = string.Empty;

    [Display(Name = "Description")]
    [StringLengthValidation(0, 2000, ErrorMessage = "The Description cannot exceed 2000 characters.")]
    [NoScriptTags(ErrorMessage = "The Description cannot contain script tags.")]
    public string? Description { get; set; }

    [Display(Name = "SKU")]
    [StringLengthValidation(0, 50, ErrorMessage = "The SKU cannot exceed 50 characters.")]
    [NoScriptTags(ErrorMessage = "The SKU cannot contain script tags.")]
    public string? Sku { get; set; }

    [Display(Name = "Barcode")]
    [StringLengthValidation(0, 50, ErrorMessage = "The Barcode cannot exceed 50 characters.")]
    [NoScriptTags(ErrorMessage = "The Barcode cannot contain script tags.")]
    public string? Barcode { get; set; }

    [Display(Name = "Category")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Category.")]
    public int? FkCategory { get; set; }

    [Display(Name = "Sub-Category")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Sub-Category.")]
    public int? FkSubcategory { get; set; }

    [Display(Name = "Brand")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Brand.")]
    public int? FkBrand { get; set; }

    [Display(Name = "Unit")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Unit.")]
    public int? FkUnit { get; set; }

    [Display(Name = "Tax")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Tax.")]
    public int? FkTax { get; set; }

    [Display(Name = "Cost Price")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Cost Price must be greater than or equal to zero.")]
    [DecimalLength(10, 2, ErrorMessage = "The Cost Price can have up to 10 digits before and 2 after the decimal point.")]
    public decimal CostPrice { get; set; }

    [Display(Name = "Selling Price")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Selling Price must be greater than or equal to zero.")]
    [DecimalLength(10, 2, ErrorMessage = "The Selling Price can have up to 10 digits before and 2 after the decimal point.")]
    public decimal SellingPrice { get; set; }

    [Display(Name = "MRP")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The MRP must be greater than or equal to zero.")]
    [DecimalLength(10, 2, ErrorMessage = "The MRP can have up to 10 digits before and 2 after the decimal point.")]
    public decimal? Mrp { get; set; }

    [Display(Name = "Reorder Level")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Reorder Level must be greater than or equal to zero.")]
    [DecimalLength(9, 3, ErrorMessage = "The Reorder Level can have up to 9 digits before and 3 after the decimal point.")]
    public decimal ReorderLevel { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Available Online")]
    public bool IsOnline { get; set; }
}

public sealed class InputProductUpdate
{
    public int IdProduct { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public int? FkCategory { get; set; }
    public int? FkSubcategory { get; set; }
    public int? FkBrand { get; set; }
    public int? FkUnit { get; set; }
    public int? FkTax { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal? Mrp { get; set; }
    public decimal ReorderLevel { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsOnline { get; set; }
}

public sealed class OutputProductUpdate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdProduct { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Product Select

public sealed class InputProductSelect { public int IdProduct { get; set; } }

public sealed class OutputProductSelect
{
    public int IdProduct { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public int? FkCategory { get; set; }
    public string? CategoryName { get; set; }
    public int? FkSubcategory { get; set; }
    public string? SubCategoryName { get; set; }
    public int? FkBrand { get; set; }
    public string? BrandName { get; set; }
    public int? FkUnit { get; set; }
    public string? UnitName { get; set; }
    public int? FkTax { get; set; }
    public string? TaxName { get; set; }
    public decimal TaxRate { get; set; }
    public bool TaxInclusive { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal? Mrp { get; set; }
    public decimal ReorderLevel { get; set; }
    public bool IsActive { get; set; }
    public bool IsOnline { get; set; }
    /// <summary>Number of non-cancelled variants (SKUs) under this product.</summary>
    public int VariantCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public InProductUpdateView ToUpdateView() => new()
    {
        IdProduct = IdProduct,
        ProductCode = ProductCode,
        ProductName = ProductName,
        Description = Description,
        Sku = Sku,
        Barcode = Barcode,
        FkCategory = FkCategory,
        FkSubcategory = FkSubcategory,
        FkBrand = FkBrand,
        FkUnit = FkUnit,
        FkTax = FkTax,
        CostPrice = CostPrice,
        SellingPrice = SellingPrice,
        Mrp = Mrp,
        ReorderLevel = ReorderLevel,
        IsActive = IsActive,
        IsOnline = IsOnline
    };
}

#endregion

#region Product Detail

public sealed class InputProductDetail
{
    public int IdProduct { get; set; }
    /// <summary>Store of the signed-in user (0 for the platform admin); stock is only summarised when the user may see it.</summary>
    public int FkStore { get; set; }
}

/// <summary>Product header + its variants + optional stock of the signed-in store (read-only detail page).</summary>
public sealed class OutputProductDetail
{
    public OutputProductSelect Product { get; set; } = new();
    public List<OutputVariantSelect> Variants { get; set; } = new();
    /// <summary>Total number of variants (the list above is capped at one page).</summary>
    public int VariantTotal { get; set; }
    /// <summary>True when <see cref="Stock"/> was loaded for a store (store user with inventory access).</summary>
    public bool HasStock { get; set; }
    public List<OutputProductStockItem> Stock { get; set; } = new();
}

public sealed class InputProductStockList
{
    public int FkStore { get; set; }
    public int FkProduct { get; set; }
}

/// <summary>One inventory row (base product when <see cref="FkProductVariant"/> is null, otherwise a variant).</summary>
public sealed class OutputProductStockItem
{
    public int? FkProductVariant { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal ReorderLevel { get; set; }
}

#endregion

#region Product List

public sealed class InputProductList
{
    public string? Search { get; set; }
    public int? FkCategory { get; set; }
    public int? FkSubcategory { get; set; }
    public int? FkBrand { get; set; }
    /// <summary>null = All, true = Active only, false = Inactive only.</summary>
    public bool? ActiveOnly { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string SortBy { get; set; } = "productname";
    public bool SortDesc { get; set; }
}

public sealed class OutputProductList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputProductSelect> Items { get; set; } = new();
    /// <summary>Echo of the normalised list filter for the view (query-string state).</summary>
    public InputProductList Filter { get; set; } = new();
    /// <summary>Dropdown data for the filter bar (categories, sub-categories of the chosen category, brands).</summary>
    public OutputProductLookup Lookup { get; set; } = new();
}

#endregion

#region Product SetActive

public sealed class InputProductSetActive
{
    public int IdProduct { get; set; }
    public bool IsActive { get; set; }
}

public sealed class OutputProductSetActive
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdProduct { get; set; }
    public bool IsActive { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Product Check

/// <summary>Facts the service needs to validate a product before create or update (unique keys, references).</summary>
public sealed class InputProductCheck
{
    /// <summary>0 on create; on update the product itself is excluded from the duplicate lookups.</summary>
    public int IdProduct { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public int? FkCategory { get; set; }
    public int? FkSubcategory { get; set; }
    public int? FkBrand { get; set; }
    public int? FkUnit { get; set; }
    public int? FkTax { get; set; }
}

public sealed class OutputProductCheck
{
    /// <summary>False when an update targets a product that does not exist or is cancelled.</summary>
    public bool ProductExists { get; set; } = true;
    public bool CodeInUse { get; set; }
    /// <summary>SKU already used by another product or by any variant.</summary>
    public bool SkuInUse { get; set; }
    /// <summary>Barcode already used by another product or by any variant.</summary>
    public bool BarcodeInUse { get; set; }
    public bool CategoryMissing { get; set; }
    public bool SubCategoryMissing { get; set; }
    /// <summary>Sub-category exists but belongs to a different category than the one selected.</summary>
    public bool SubCategoryOutsideCategory { get; set; }
    public bool BrandMissing { get; set; }
    public bool UnitMissing { get; set; }
    public bool TaxMissing { get; set; }
}

#endregion

#region Product Lookup

public sealed class InputProductLookup
{
    /// <summary>false = active rows only (plus any currently selected id); true = every non-cancelled row.</summary>
    public bool IncludeInactive { get; set; }
    /// <summary>Selected category; also scopes the sub-category list (none selected = no sub-categories).</summary>
    public int? FkCategory { get; set; }
    public int? FkSubcategory { get; set; }
    public int? FkBrand { get; set; }
    public int? FkUnit { get; set; }
    public int? FkTax { get; set; }
    /// <summary>false skips the unit and tax lists (the list-page filter bar only needs categories and brands).</summary>
    public bool IncludeUnitsAndTaxes { get; set; } = true;
}

public sealed class OutputProductLookup
{
    public List<OutputProductLookupItem> Categories { get; set; } = new();
    public List<OutputProductLookupItem> SubCategories { get; set; } = new();
    public List<OutputProductLookupItem> Brands { get; set; } = new();
    public List<OutputProductLookupItem> Units { get; set; } = new();
    public List<OutputProductLookupItem> Taxes { get; set; } = new();
}

/// <summary>Id/name pair shared by every product dropdown (rate and inclusive flag are filled for taxes only).</summary>
public sealed class OutputProductLookupItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public decimal Rate { get; set; }
    public bool IsInclusive { get; set; }
    public bool IsActive { get; set; }
}

#endregion

#region Product Search

public sealed class InputProductSearch
{
    /// <summary>Matches product name/code/SKU/barcode and variant name/SKU/barcode; empty returns the first items by name.</summary>
    public string? Term { get; set; }
    /// <summary>Maximum rows returned (1-50).</summary>
    public int Take { get; set; } = 20;
    /// <summary>true = products that have active variants are returned as one row per variant; false = product rows only.</summary>
    public bool ExpandVariants { get; set; } = true;
}

public sealed class InputProductBarcode
{
    public string Barcode { get; set; } = string.Empty;
}

/// <summary>
/// Flat picker row used by purchases, stock and POS. One row per sellable item:
/// a product (IdProductVariant null) or one of its variants. Field names follow
/// <see cref="OutputProductSelect"/> so existing barcode-lookup consumers keep working.
/// </summary>
public sealed class OutputProductSearchItem
{
    public int IdProduct { get; set; }
    /// <summary>Set on variant rows; null on plain product rows.</summary>
    public int? IdProductVariant { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    /// <summary>Variant SKU on variant rows, otherwise the product SKU.</summary>
    public string? Sku { get; set; }
    /// <summary>Variant barcode on variant rows, otherwise the product barcode.</summary>
    public string? Barcode { get; set; }
    /// <summary>Selling price: the product price, plus the additional price on variant rows.</summary>
    public decimal SellingPrice { get; set; }
    /// <summary>Maximum retail price (variant rows add the additional price); null when not set.</summary>
    public decimal? Mrp { get; set; }
    /// <summary>Default cost price; null when the caller is not allowed to see costs.</summary>
    public decimal? CostPrice { get; set; }
    public int? FkUnit { get; set; }
    public string? UnitName { get; set; }
    public string? UnitCode { get; set; }
    public int? FkTax { get; set; }
    public decimal TaxRate { get; set; }
    public bool TaxInclusive { get; set; }
    public bool IsActive { get; set; }
    /// <summary>"Product" or "Product - Variant", ready to show in a picker.</summary>
    public string DisplayName => string.IsNullOrEmpty(VariantName) ? ProductName : ProductName + " - " + VariantName;
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// PRODUCT VARIANT
// ═══════════════════════════════════════════════════════════════════

#region Variant Create

public sealed class InVariantCreateView
{
    [Display(Name = "Product")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Product.")]
    public int FkProduct { get; set; }

    [Display(Name = "Variant Name")]
    [Required(ErrorMessage = "The Variant Name field is required.")]
    [TrimmedString(1, 200, ErrorMessage = "The Variant Name must be between 1 and 200 characters.")]
    [NoScriptTags(ErrorMessage = "The Variant Name cannot contain script tags.")]
    public string VariantName { get; set; } = string.Empty;

    [Display(Name = "SKU Code")]
    [StringLengthValidation(0, 50, ErrorMessage = "The SKU Code cannot exceed 50 characters.")]
    [NoScriptTags(ErrorMessage = "The SKU Code cannot contain script tags.")]
    public string? SkuCode { get; set; }

    [Display(Name = "Barcode")]
    [StringLengthValidation(0, 50, ErrorMessage = "The Barcode cannot exceed 50 characters.")]
    [NoScriptTags(ErrorMessage = "The Barcode cannot contain script tags.")]
    public string? Barcode { get; set; }

    [Display(Name = "Additional Price")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Additional Price must be greater than or equal to zero.")]
    [DecimalLength(10, 2, ErrorMessage = "The Additional Price can have up to 10 digits before and 2 after the decimal point.")]
    public decimal AdditionalPrice { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public sealed class InputVariantCreate
{
    public int FkProduct { get; set; }
    public string VariantName { get; set; } = string.Empty;
    public string? SkuCode { get; set; }
    public string? Barcode { get; set; }
    public decimal AdditionalPrice { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class OutputVariantCreate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdProductVariant { get; set; }
    public int FkProduct { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Variant Update

public sealed class InVariantUpdateView
{
    [Display(Name = "Variant")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Variant.")]
    public int IdProductVariant { get; set; }

    [Display(Name = "Product")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Product.")]
    public int FkProduct { get; set; }

    [Display(Name = "Variant Name")]
    [Required(ErrorMessage = "The Variant Name field is required.")]
    [TrimmedString(1, 200, ErrorMessage = "The Variant Name must be between 1 and 200 characters.")]
    [NoScriptTags(ErrorMessage = "The Variant Name cannot contain script tags.")]
    public string VariantName { get; set; } = string.Empty;

    [Display(Name = "SKU Code")]
    [StringLengthValidation(0, 50, ErrorMessage = "The SKU Code cannot exceed 50 characters.")]
    [NoScriptTags(ErrorMessage = "The SKU Code cannot contain script tags.")]
    public string? SkuCode { get; set; }

    [Display(Name = "Barcode")]
    [StringLengthValidation(0, 50, ErrorMessage = "The Barcode cannot exceed 50 characters.")]
    [NoScriptTags(ErrorMessage = "The Barcode cannot contain script tags.")]
    public string? Barcode { get; set; }

    [Display(Name = "Additional Price")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Additional Price must be greater than or equal to zero.")]
    [DecimalLength(10, 2, ErrorMessage = "The Additional Price can have up to 10 digits before and 2 after the decimal point.")]
    public decimal AdditionalPrice { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public sealed class InputVariantUpdate
{
    public int IdProductVariant { get; set; }
    public int FkProduct { get; set; }
    public string VariantName { get; set; } = string.Empty;
    public string? SkuCode { get; set; }
    public string? Barcode { get; set; }
    public decimal AdditionalPrice { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class OutputVariantUpdate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdProductVariant { get; set; }
    /// <summary>Parent product as stored in the database (the client-sent value is never trusted).</summary>
    public int FkProduct { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Variant Select

public sealed class InputVariantSelect { public int IdProductVariant { get; set; } }

public sealed class OutputVariantSelect
{
    public int IdProductVariant { get; set; }
    public int FkProduct { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public bool ProductIsActive { get; set; } = true;
    public decimal ProductSellingPrice { get; set; }
    public string VariantName { get; set; } = string.Empty;
    public string? SkuCode { get; set; }
    public string? Barcode { get; set; }
    public decimal AdditionalPrice { get; set; }
    public bool IsActive { get; set; }
    /// <summary>Selling price of the variant: product price plus the additional price.</summary>
    public decimal EffectivePrice => ProductSellingPrice + AdditionalPrice;

    public InVariantUpdateView ToUpdateView() => new()
    {
        IdProductVariant = IdProductVariant,
        FkProduct = FkProduct,
        VariantName = VariantName,
        SkuCode = SkuCode,
        Barcode = Barcode,
        AdditionalPrice = AdditionalPrice,
        IsActive = IsActive
    };
}

#endregion

#region Variant List

public sealed class InputVariantList
{
    /// <summary>null = variants of every product (global SKU list).</summary>
    public int? FkProduct { get; set; }
    public string? Search { get; set; }
    /// <summary>null = All, true = Active only, false = Inactive only.</summary>
    public bool? ActiveOnly { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string SortBy { get; set; } = string.Empty;
    public bool SortDesc { get; set; }
}

public sealed class OutputVariantList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    /// <summary>Product the list is scoped to; 0 for the global SKU list.</summary>
    public int FkProduct { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    /// <summary>Empty when the list is global or the requested product does not exist.</summary>
    public string ProductName { get; set; } = string.Empty;
    public bool ProductIsActive { get; set; } = true;
    public List<OutputVariantSelect> Items { get; set; } = new();
    /// <summary>Echo of the normalised list filter for the view (query-string state).</summary>
    public InputVariantList Filter { get; set; } = new();
}

#endregion

#region Variant SetActive

public sealed class InputVariantSetActive
{
    public int IdProductVariant { get; set; }
    public bool IsActive { get; set; }
}

public sealed class OutputVariantSetActive
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdProductVariant { get; set; }
    public int FkProduct { get; set; }
    public bool IsActive { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Variant Check

/// <summary>Facts the service needs to validate a variant before create or update.</summary>
public sealed class InputVariantCheck
{
    /// <summary>0 on create; on update the variant itself is excluded and its stored parent product wins.</summary>
    public int IdProductVariant { get; set; }
    public int FkProduct { get; set; }
    public string VariantName { get; set; } = string.Empty;
    public string? SkuCode { get; set; }
    public string? Barcode { get; set; }
}

public sealed class OutputVariantCheck
{
    /// <summary>False when an update targets a variant that does not exist or is cancelled.</summary>
    public bool VariantExists { get; set; } = true;
    public bool ProductExists { get; set; }
    /// <summary>Effective parent product (stored parent on update, requested product on create).</summary>
    public int FkProduct { get; set; }
    public bool NameInUse { get; set; }
    /// <summary>SKU code already used by another variant or by any product.</summary>
    public bool SkuInUse { get; set; }
    /// <summary>Barcode already used by another variant or by any product.</summary>
    public bool BarcodeInUse { get; set; }
}

#endregion
