using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.Models;

// ═══════════════════════════════════════════════════════════════════
// CATEGORY
// ═══════════════════════════════════════════════════════════════════

#region Category Create

public sealed class InCategoryCreateView
{
    [MaxLength(20)]
    [Display(Name = "Category code")]
    public string? CategoryCode { get; set; }

    [Required, MaxLength(200)]
    [Display(Name = "Category name")]
    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Display(Name = "Display order")]
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

    public static InputCategoryCreate From(InCategoryCreateView v) => new()
    {
        CategoryCode = v.CategoryCode,
        CategoryName = v.CategoryName,
        Description = v.Description,
        DisplayOrder = v.DisplayOrder,
        IsActive = v.IsActive
    };
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
    public int IdCategory { get; set; }

    [MaxLength(20)]
    [Display(Name = "Category code")]
    public string? CategoryCode { get; set; }

    [Required, MaxLength(200)]
    [Display(Name = "Category name")]
    public string CategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Display(Name = "Display order")]
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

    public static InputCategoryUpdate From(InCategoryUpdateView v) => new()
    {
        IdCategory = v.IdCategory,
        CategoryCode = v.CategoryCode,
        CategoryName = v.CategoryName,
        Description = v.Description,
        DisplayOrder = v.DisplayOrder,
        IsActive = v.IsActive
    };
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
    [Required]
    [Display(Name = "Category")]
    public int FkCategory { get; set; }

    [Required, MaxLength(200)]
    [Display(Name = "Sub-category name")]
    public string SubCategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Display(Name = "Display order")]
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

    public static InputSubCategoryCreate From(InSubCategoryCreateView v) => new()
    {
        FkCategory = v.FkCategory,
        SubCategoryName = v.SubCategoryName,
        Description = v.Description,
        DisplayOrder = v.DisplayOrder,
        IsActive = v.IsActive
    };
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
    public int IdSubCategory { get; set; }

    [Required]
    [Display(Name = "Category")]
    public int FkCategory { get; set; }

    [Required, MaxLength(200)]
    [Display(Name = "Sub-category name")]
    public string SubCategoryName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Display(Name = "Display order")]
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

    public static InputSubCategoryUpdate From(InSubCategoryUpdateView v) => new()
    {
        IdSubCategory = v.IdSubCategory,
        FkCategory = v.FkCategory,
        SubCategoryName = v.SubCategoryName,
        Description = v.Description,
        DisplayOrder = v.DisplayOrder,
        IsActive = v.IsActive
    };
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
    [Required, MaxLength(200)]
    [Display(Name = "Brand name")]
    public string BrandName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public sealed class InputBrandCreate
{
    public string BrandName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public static InputBrandCreate From(InBrandCreateView v) => new()
    {
        BrandName = v.BrandName,
        Description = v.Description,
        IsActive = v.IsActive
    };
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
    public int IdBrand { get; set; }

    [Required, MaxLength(200)]
    [Display(Name = "Brand name")]
    public string BrandName { get; set; } = string.Empty;

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

    public static InputBrandUpdate From(InBrandUpdateView v) => new()
    {
        IdBrand = v.IdBrand,
        BrandName = v.BrandName,
        Description = v.Description,
        IsActive = v.IsActive
    };
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
    [Required, MaxLength(20)]
    [Display(Name = "Unit code")]
    public string UnitCode { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    [Display(Name = "Unit name")]
    public string UnitName { get; set; } = string.Empty;

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public sealed class InputUnitCreate
{
    public string UnitCode { get; set; } = string.Empty;
    public string UnitName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public static InputUnitCreate From(InUnitCreateView v) => new()
    {
        UnitCode = v.UnitCode,
        UnitName = v.UnitName,
        IsActive = v.IsActive
    };
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
    public int IdUnit { get; set; }

    [Required, MaxLength(20)]
    [Display(Name = "Unit code")]
    public string UnitCode { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    [Display(Name = "Unit name")]
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

    public static InputUnitUpdate From(InUnitUpdateView v) => new()
    {
        IdUnit = v.IdUnit,
        UnitCode = v.UnitCode,
        UnitName = v.UnitName,
        IsActive = v.IsActive
    };
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
    [Required, MaxLength(20)]
    [Display(Name = "Tax code")]
    public string TaxCode { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    [Display(Name = "Tax name")]
    public string TaxName { get; set; } = string.Empty;

    [Range(0, 100)]
    [Display(Name = "Tax rate (%)")]
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

    public static InputTaxCreate From(InTaxCreateView v) => new()
    {
        TaxCode = v.TaxCode,
        TaxName = v.TaxName,
        TaxRate = v.TaxRate,
        IsInclusive = v.IsInclusive,
        IsActive = v.IsActive
    };
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
    public int IdTax { get; set; }

    [Required, MaxLength(20)]
    [Display(Name = "Tax code")]
    public string TaxCode { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    [Display(Name = "Tax name")]
    public string TaxName { get; set; } = string.Empty;

    [Range(0, 100)]
    [Display(Name = "Tax rate (%)")]
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

    public static InputTaxUpdate From(InTaxUpdateView v) => new()
    {
        IdTax = v.IdTax,
        TaxCode = v.TaxCode,
        TaxName = v.TaxName,
        TaxRate = v.TaxRate,
        IsInclusive = v.IsInclusive,
        IsActive = v.IsActive
    };
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
