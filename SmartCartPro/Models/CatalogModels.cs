using System.ComponentModel.DataAnnotations;
using SmartCartPro.CustomModelValidation;

namespace SmartCartPro.Models;

// ═══════════════════════════════════════════════════════════════════
// CATEGORY
// ═══════════════════════════════════════════════════════════════════

#region Category Create

public sealed class InCategoryCreateView
{
    [Display(Name = "Category Code")]
    [StringLengthValidation(0, 20, ErrorMessage = "The Category Code cannot exceed 20 characters.")]
    [NoScriptTags(ErrorMessage = "The Category Code cannot contain script tags.")]
    public string? CategoryCode { get; set; }

    [Display(Name = "Category Name")]
    [Required(ErrorMessage = "The Category Name field is required.")]
    [TrimmedString(2, 200, ErrorMessage = "The Category Name must be between 2 and 200 characters.")]
    [NoScriptTags(ErrorMessage = "The Category Name cannot contain script tags.")]
    public string CategoryName { get; set; } = string.Empty;

    [Display(Name = "Description")]
    [StringLengthValidation(0, 1000, ErrorMessage = "The Description cannot exceed 1000 characters.")]
    [NoScriptTags(ErrorMessage = "The Description cannot contain script tags.")]
    public string? Description { get; set; }

    [Display(Name = "Display Order")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Display Order must be zero or greater.")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public sealed class InputCategoryCreate
{
    public string? CategoryCode { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class OutputCategoryCreate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdCategory { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Category Update

public sealed class InCategoryUpdateView
{
    [Display(Name = "Category")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Category.")]
    public int IdCategory { get; set; }

    [Display(Name = "Category Code")]
    [StringLengthValidation(0, 20, ErrorMessage = "The Category Code cannot exceed 20 characters.")]
    [NoScriptTags(ErrorMessage = "The Category Code cannot contain script tags.")]
    public string? CategoryCode { get; set; }

    [Display(Name = "Category Name")]
    [Required(ErrorMessage = "The Category Name field is required.")]
    [TrimmedString(2, 200, ErrorMessage = "The Category Name must be between 2 and 200 characters.")]
    [NoScriptTags(ErrorMessage = "The Category Name cannot contain script tags.")]
    public string CategoryName { get; set; } = string.Empty;

    [Display(Name = "Description")]
    [StringLengthValidation(0, 1000, ErrorMessage = "The Description cannot exceed 1000 characters.")]
    [NoScriptTags(ErrorMessage = "The Description cannot contain script tags.")]
    public string? Description { get; set; }

    [Display(Name = "Display Order")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Display Order must be zero or greater.")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public sealed class InputCategoryUpdate
{
    public int IdCategory { get; set; }
    public string? CategoryCode { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class OutputCategoryUpdate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdCategory { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Category Select

public sealed class InputCategorySelect { public int IdCategory { get; set; } }

public sealed class OutputCategorySelect
{
    public int IdCategory { get; set; }
    public string? CategoryCode { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }

    public InCategoryUpdateView ToUpdateView() => new()
    {
        IdCategory = IdCategory,
        CategoryCode = CategoryCode,
        CategoryName = CategoryName,
        Description = Description,
        DisplayOrder = DisplayOrder,
        IsActive = IsActive
    };
}

#endregion

#region Category List

public sealed class InputCategoryList
{
    public string? Search { get; set; }
    public bool? ActiveOnly { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
    public string SortBy { get; set; } = "categoryname";
    public bool SortDesc { get; set; }
}

public sealed class OutputCategoryList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputCategorySelect> Items { get; set; } = new();
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// SUB-CATEGORY
// ═══════════════════════════════════════════════════════════════════

#region SubCategory Create

public sealed class InSubCategoryCreateView
{
    [Display(Name = "Category")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Category.")]
    public int FkCategory { get; set; }

    [Display(Name = "Sub-Category Name")]
    [Required(ErrorMessage = "The Sub-Category Name field is required.")]
    [TrimmedString(2, 200, ErrorMessage = "The Sub-Category Name must be between 2 and 200 characters.")]
    [NoScriptTags(ErrorMessage = "The Sub-Category Name cannot contain script tags.")]
    public string SubCategoryName { get; set; } = string.Empty;

    [Display(Name = "Description")]
    [StringLengthValidation(0, 1000, ErrorMessage = "The Description cannot exceed 1000 characters.")]
    [NoScriptTags(ErrorMessage = "The Description cannot contain script tags.")]
    public string? Description { get; set; }

    [Display(Name = "Display Order")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Display Order must be zero or greater.")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public sealed class InputSubCategoryCreate
{
    public int FkCategory { get; set; }
    public string SubCategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class OutputSubCategoryCreate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdSubCategory { get; set; }
    public string SubCategoryName { get; set; } = string.Empty;
    public bool Ok => ResponseCode == 0;
}

#endregion

#region SubCategory Update

public sealed class InSubCategoryUpdateView
{
    [Display(Name = "Sub-Category")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Sub-Category.")]
    public int IdSubCategory { get; set; }

    [Display(Name = "Category")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Category.")]
    public int FkCategory { get; set; }

    [Display(Name = "Sub-Category Name")]
    [Required(ErrorMessage = "The Sub-Category Name field is required.")]
    [TrimmedString(2, 200, ErrorMessage = "The Sub-Category Name must be between 2 and 200 characters.")]
    [NoScriptTags(ErrorMessage = "The Sub-Category Name cannot contain script tags.")]
    public string SubCategoryName { get; set; } = string.Empty;

    [Display(Name = "Description")]
    [StringLengthValidation(0, 1000, ErrorMessage = "The Description cannot exceed 1000 characters.")]
    [NoScriptTags(ErrorMessage = "The Description cannot contain script tags.")]
    public string? Description { get; set; }

    [Display(Name = "Display Order")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Display Order must be zero or greater.")]
    public int DisplayOrder { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public sealed class InputSubCategoryUpdate
{
    public int IdSubCategory { get; set; }
    public int FkCategory { get; set; }
    public string SubCategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class OutputSubCategoryUpdate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdSubCategory { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

#region SubCategory Select

public sealed class InputSubCategorySelect { public int IdSubCategory { get; set; } }

public sealed class OutputSubCategorySelect
{
    public int IdSubCategory { get; set; }
    public int FkCategory { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string SubCategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }

    public InSubCategoryUpdateView ToUpdateView() => new()
    {
        IdSubCategory = IdSubCategory,
        FkCategory = FkCategory,
        SubCategoryName = SubCategoryName,
        Description = Description,
        DisplayOrder = DisplayOrder,
        IsActive = IsActive
    };
}

#endregion

#region SubCategory List

public sealed class InputSubCategoryList
{
    public int? FkCategory { get; set; }
    public string? Search { get; set; }
    public bool? ActiveOnly { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed class OutputSubCategoryList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputSubCategorySelect> Items { get; set; } = new();
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// BRAND
// ═══════════════════════════════════════════════════════════════════

#region Brand Create

public sealed class InBrandCreateView
{
    [Display(Name = "Brand Name")]
    [Required(ErrorMessage = "The Brand Name field is required.")]
    [TrimmedString(2, 200, ErrorMessage = "The Brand Name must be between 2 and 200 characters.")]
    [NoScriptTags(ErrorMessage = "The Brand Name cannot contain script tags.")]
    public string BrandName { get; set; } = string.Empty;

    [Display(Name = "Description")]
    [StringLengthValidation(0, 1000, ErrorMessage = "The Description cannot exceed 1000 characters.")]
    [NoScriptTags(ErrorMessage = "The Description cannot contain script tags.")]
    public string? Description { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public sealed class InputBrandCreate
{
    public string BrandName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class OutputBrandCreate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdBrand { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Brand Update

public sealed class InBrandUpdateView
{
    [Display(Name = "Brand")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Brand.")]
    public int IdBrand { get; set; }

    [Display(Name = "Brand Name")]
    [Required(ErrorMessage = "The Brand Name field is required.")]
    [TrimmedString(2, 200, ErrorMessage = "The Brand Name must be between 2 and 200 characters.")]
    [NoScriptTags(ErrorMessage = "The Brand Name cannot contain script tags.")]
    public string BrandName { get; set; } = string.Empty;

    [Display(Name = "Description")]
    [StringLengthValidation(0, 1000, ErrorMessage = "The Description cannot exceed 1000 characters.")]
    [NoScriptTags(ErrorMessage = "The Description cannot contain script tags.")]
    public string? Description { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public sealed class InputBrandUpdate
{
    public int IdBrand { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class OutputBrandUpdate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdBrand { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Brand Select

public sealed class InputBrandSelect { public int IdBrand { get; set; } }

public sealed class OutputBrandSelect
{
    public int IdBrand { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }

    public InBrandUpdateView ToUpdateView() => new()
    {
        IdBrand = IdBrand,
        BrandName = BrandName,
        Description = Description,
        IsActive = IsActive
    };
}

#endregion

#region Brand List

public sealed class InputBrandList
{
    public string? Search { get; set; }
    public bool? ActiveOnly { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed class OutputBrandList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputBrandSelect> Items { get; set; } = new();
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// UNIT
// ═══════════════════════════════════════════════════════════════════

#region Unit Create

public sealed class InUnitCreateView
{
    [Display(Name = "Unit Code")]
    [Required(ErrorMessage = "The Unit Code field is required.")]
    [StringLengthValidation(1, 20, ErrorMessage = "The Unit Code must be between 1 and 20 characters.")]
    [NoScriptTags(ErrorMessage = "The Unit Code cannot contain script tags.")]
    public string UnitCode { get; set; } = string.Empty;

    [Display(Name = "Unit Name")]
    [Required(ErrorMessage = "The Unit Name field is required.")]
    [TrimmedString(1, 100, ErrorMessage = "The Unit Name must be between 1 and 100 characters.")]
    [NoScriptTags(ErrorMessage = "The Unit Name cannot contain script tags.")]
    public string UnitName { get; set; } = string.Empty;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public sealed class InputUnitCreate
{
    public string UnitCode { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public sealed class OutputUnitCreate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdUnit { get; set; }
    public string UnitCode { get; set; } = string.Empty;
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Unit Update

public sealed class InUnitUpdateView
{
    [Display(Name = "Unit")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Unit.")]
    public int IdUnit { get; set; }

    [Display(Name = "Unit Code")]
    [Required(ErrorMessage = "The Unit Code field is required.")]
    [StringLengthValidation(1, 20, ErrorMessage = "The Unit Code must be between 1 and 20 characters.")]
    [NoScriptTags(ErrorMessage = "The Unit Code cannot contain script tags.")]
    public string UnitCode { get; set; } = string.Empty;

    [Display(Name = "Unit Name")]
    [Required(ErrorMessage = "The Unit Name field is required.")]
    [TrimmedString(1, 100, ErrorMessage = "The Unit Name must be between 1 and 100 characters.")]
    [NoScriptTags(ErrorMessage = "The Unit Name cannot contain script tags.")]
    public string UnitName { get; set; } = string.Empty;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public sealed class InputUnitUpdate
{
    public int IdUnit { get; set; }
    public string UnitCode { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public sealed class OutputUnitUpdate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdUnit { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Unit Select

public sealed class InputUnitSelect { public int IdUnit { get; set; } }

public sealed class OutputUnitSelect
{
    public int IdUnit { get; set; }
    public string UnitCode { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public InUnitUpdateView ToUpdateView() => new()
    {
        IdUnit = IdUnit,
        UnitCode = UnitCode,
        UnitName = UnitName,
        IsActive = IsActive
    };
}

#endregion

#region Unit List

public sealed class InputUnitList
{
    public string? Search { get; set; }
    public bool? ActiveOnly { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed class OutputUnitList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputUnitSelect> Items { get; set; } = new();
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// TAX
// ═══════════════════════════════════════════════════════════════════

#region Tax Create

public sealed class InTaxCreateView
{
    [Display(Name = "Tax Code")]
    [Required(ErrorMessage = "The Tax Code field is required.")]
    [StringLengthValidation(1, 20, ErrorMessage = "The Tax Code must be between 1 and 20 characters.")]
    [NoScriptTags(ErrorMessage = "The Tax Code cannot contain script tags.")]
    public string TaxCode { get; set; } = string.Empty;

    [Display(Name = "Tax Name")]
    [Required(ErrorMessage = "The Tax Name field is required.")]
    [TrimmedString(1, 100, ErrorMessage = "The Tax Name must be between 1 and 100 characters.")]
    [NoScriptTags(ErrorMessage = "The Tax Name cannot contain script tags.")]
    public string TaxName { get; set; } = string.Empty;

    [Display(Name = "Tax Rate (%)")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Tax Rate (%) must be greater than or equal to zero.")]
    [MaxPercentage(100, ErrorMessage = "The Tax Rate (%) must not exceed 100.")]
    public decimal TaxRate { get; set; }

    [Display(Name = "Inclusive")]
    public bool IsInclusive { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public sealed class InputTaxCreate
{
    public string TaxCode { get; set; } = string.Empty;
    public string TaxName { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public bool IsInclusive { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class OutputTaxCreate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdTax { get; set; }
    public string TaxCode { get; set; } = string.Empty;
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Tax Update

public sealed class InTaxUpdateView
{
    [Display(Name = "Tax")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Tax.")]
    public int IdTax { get; set; }

    [Display(Name = "Tax Code")]
    [Required(ErrorMessage = "The Tax Code field is required.")]
    [StringLengthValidation(1, 20, ErrorMessage = "The Tax Code must be between 1 and 20 characters.")]
    [NoScriptTags(ErrorMessage = "The Tax Code cannot contain script tags.")]
    public string TaxCode { get; set; } = string.Empty;

    [Display(Name = "Tax Name")]
    [Required(ErrorMessage = "The Tax Name field is required.")]
    [TrimmedString(1, 100, ErrorMessage = "The Tax Name must be between 1 and 100 characters.")]
    [NoScriptTags(ErrorMessage = "The Tax Name cannot contain script tags.")]
    public string TaxName { get; set; } = string.Empty;

    [Display(Name = "Tax Rate (%)")]
    [DecimalGreaterThanOrEqualToZero(ErrorMessage = "The Tax Rate (%) must be greater than or equal to zero.")]
    [MaxPercentage(100, ErrorMessage = "The Tax Rate (%) must not exceed 100.")]
    public decimal TaxRate { get; set; }

    [Display(Name = "Inclusive")]
    public bool IsInclusive { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public sealed class InputTaxUpdate
{
    public int IdTax { get; set; }
    public string TaxCode { get; set; } = string.Empty;
    public string TaxName { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public bool IsInclusive { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class OutputTaxUpdate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdTax { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Tax Select

public sealed class InputTaxSelect { public int IdTax { get; set; } }

public sealed class OutputTaxSelect
{
    public int IdTax { get; set; }
    public string TaxCode { get; set; } = string.Empty;
    public string TaxName { get; set; } = string.Empty;
    public decimal TaxRate { get; set; }
    public bool IsInclusive { get; set; }
    public bool IsActive { get; set; }

    public InTaxUpdateView ToUpdateView() => new()
    {
        IdTax = IdTax,
        TaxCode = TaxCode,
        TaxName = TaxName,
        TaxRate = TaxRate,
        IsInclusive = IsInclusive,
        IsActive = IsActive
    };
}

#endregion

#region Tax List

public sealed class InputTaxList
{
    public string? Search { get; set; }
    public bool? ActiveOnly { get; set; } = true;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed class OutputTaxList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputTaxSelect> Items { get; set; } = new();
}

#endregion
