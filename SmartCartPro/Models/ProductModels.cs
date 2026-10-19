using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.Models;

// ═══════════════════════════════════════════════════════════════════
// PRODUCT
// ═══════════════════════════════════════════════════════════════════

#region Product Create

public sealed class InProductCreateView
{
    [Required, MaxLength(50)]
    [Display(Name = "Product code")]
    public string ProductCode { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    [Display(Name = "Product name")]
    public string ProductName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Sku { get; set; }

    [MaxLength(50)]
    public string? Barcode { get; set; }

    [Display(Name = "Category")]
    public int? FkCategory { get; set; }

    [Display(Name = "Sub-category")]
    public int? FkSubcategory { get; set; }

    [Display(Name = "Brand")]
    public int? FkBrand { get; set; }

    [Display(Name = "Unit")]
    public int? FkUnit { get; set; }

    [Display(Name = "Tax")]
    public int? FkTax { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Cost price")]
    public decimal CostPrice { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Selling price")]
    public decimal SellingPrice { get; set; }

    [Display(Name = "MRP")]
    public decimal? Mrp { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Reorder level")]
    public decimal ReorderLevel { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Available online")]
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

    public static InputProductCreate From(InProductCreateView v) => new()
    {
        ProductCode = v.ProductCode,
        ProductName = v.ProductName,
        Description = v.Description,
        Sku = v.Sku,
        Barcode = v.Barcode,
        FkCategory = v.FkCategory,
        FkSubcategory = v.FkSubcategory,
        FkBrand = v.FkBrand,
        FkUnit = v.FkUnit,
        FkTax = v.FkTax,
        CostPrice = v.CostPrice,
        SellingPrice = v.SellingPrice,
        Mrp = v.Mrp,
        ReorderLevel = v.ReorderLevel,
        IsActive = v.IsActive,
        IsOnline = v.IsOnline
    };
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
    public int IdProduct { get; set; }

    [Required, MaxLength(50)]
    [Display(Name = "Product code")]
    public string ProductCode { get; set; } = string.Empty;

    [Required, MaxLength(300)]
    [Display(Name = "Product name")]
    public string ProductName { get; set; } = string.Empty;

    public string? Description { get; set; }

    [MaxLength(50)]
    public string? Sku { get; set; }

    [MaxLength(50)]
    public string? Barcode { get; set; }

    [Display(Name = "Category")]
    public int? FkCategory { get; set; }

    [Display(Name = "Sub-category")]
    public int? FkSubcategory { get; set; }

    [Display(Name = "Brand")]
    public int? FkBrand { get; set; }

    [Display(Name = "Unit")]
    public int? FkUnit { get; set; }

    [Display(Name = "Tax")]
    public int? FkTax { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Cost price")]
    public decimal CostPrice { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Selling price")]
    public decimal SellingPrice { get; set; }

    [Display(Name = "MRP")]
    public decimal? Mrp { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Reorder level")]
    public decimal ReorderLevel { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Available online")]
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

    public static InputProductUpdate From(InProductUpdateView v) => new()
    {
        IdProduct = v.IdProduct,
        ProductCode = v.ProductCode,
        ProductName = v.ProductName,
        Description = v.Description,
        Sku = v.Sku,
        Barcode = v.Barcode,
        FkCategory = v.FkCategory,
        FkSubcategory = v.FkSubcategory,
        FkBrand = v.FkBrand,
        FkUnit = v.FkUnit,
        FkTax = v.FkTax,
        CostPrice = v.CostPrice,
        SellingPrice = v.SellingPrice,
        Mrp = v.Mrp,
        ReorderLevel = v.ReorderLevel,
        IsActive = v.IsActive,
        IsOnline = v.IsOnline
    };
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
    public int FkProduct { get; set; }

    [Required, MaxLength(200)]
    [Display(Name = "Variant name")]
    public string VariantName { get; set; } = string.Empty;

    [MaxLength(50)]
    [Display(Name = "SKU code")]
    public string? SkuCode { get; set; }

    [MaxLength(50)]
    public string? Barcode { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Additional price")]
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

    public static InputVariantCreate From(InVariantCreateView v) => new()
    {
        FkProduct = v.FkProduct,
        VariantName = v.VariantName,
        SkuCode = v.SkuCode,
        Barcode = v.Barcode,
        AdditionalPrice = v.AdditionalPrice,
        IsActive = v.IsActive
    };
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
    public int IdProductVariant { get; set; }
    public int FkProduct { get; set; }

    [Required, MaxLength(200)]
    [Display(Name = "Variant name")]
    public string VariantName { get; set; } = string.Empty;

    [MaxLength(50)]
    [Display(Name = "SKU code")]
    public string? SkuCode { get; set; }

    [MaxLength(50)]
    public string? Barcode { get; set; }

    [Range(0, double.MaxValue)]
    [Display(Name = "Additional price")]
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

    public static InputVariantUpdate From(InVariantUpdateView v) => new()
    {
        IdProductVariant = v.IdProductVariant,
        FkProduct = v.FkProduct,
        VariantName = v.VariantName,
        SkuCode = v.SkuCode,
        Barcode = v.Barcode,
        AdditionalPrice = v.AdditionalPrice,
        IsActive = v.IsActive
    };
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
