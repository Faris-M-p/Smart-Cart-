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
    public decimal CostPrice { get; set; }

    [Display(Name = "Selling Price")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Selling Price must be greater than or equal to zero.")]
    public decimal SellingPrice { get; set; }

    [Display(Name = "MRP")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The MRP must be greater than or equal to zero.")]
    public decimal? Mrp { get; set; }

    [Display(Name = "Reorder Level")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Reorder Level must be greater than or equal to zero.")]
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
    public decimal CostPrice { get; set; }

    [Display(Name = "Selling Price")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Selling Price must be greater than or equal to zero.")]
    public decimal SellingPrice { get; set; }

    [Display(Name = "MRP")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The MRP must be greater than or equal to zero.")]
    public decimal? Mrp { get; set; }

    [Display(Name = "Reorder Level")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Reorder Level must be greater than or equal to zero.")]
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
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal? Mrp { get; set; }
    public decimal ReorderLevel { get; set; }
    public bool IsActive { get; set; }
    public bool IsOnline { get; set; }

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

#region Product List

public sealed class InputProductList
{
    public string? Search { get; set; }
    public int? FkCategory { get; set; }
    public int? FkBrand { get; set; }
    public bool? ActiveOnly { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string SortBy { get; set; } = "productname";
    public bool SortDesc { get; set; }
}

public sealed class OutputProductList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputProductSelect> Items { get; set; } = new();
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
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Variant Select

public sealed class InputVariantSelect { public int IdProductVariant { get; set; } }

public sealed class OutputVariantSelect
{
    public int IdProductVariant { get; set; }
    public int FkProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public string? SkuCode { get; set; }
    public string? Barcode { get; set; }
    public decimal AdditionalPrice { get; set; }
    public bool IsActive { get; set; }

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
    public int FkProduct { get; set; }
    public bool? ActiveOnly { get; set; } = true;
}

public sealed class OutputVariantList
{
    public int FkProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public List<OutputVariantSelect> Items { get; set; } = new();
}

#endregion
