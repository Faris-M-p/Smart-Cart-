using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCartPro.Entities;

/// <summary>Company-wide product master. Maps to <c>products</c>.</summary>
[Table("products")]
public class Product
{
    [Key]
    [Column("id_product")]
    public int IdProduct { get; set; }

    [Column("fk_category")]
    public int? FkCategory { get; set; }

    [Column("fk_subcategory")]
    public int? FkSubcategory { get; set; }

    [Column("fk_brand")]
    public int? FkBrand { get; set; }

    [Column("fk_unit")]
    public int? FkUnit { get; set; }

    [Column("fk_tax")]
    public int? FkTax { get; set; }

    [Required]
    [Column("productcode")]
    public string ProductCode { get; set; } = string.Empty;

    [Required]
    [Column("productname")]
    public string ProductName { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("sku")]
    public string? Sku { get; set; }

    [Column("barcode")]
    public string? Barcode { get; set; }

    [Precision(12, 2)]
    [Column("costprice", TypeName = "numeric(12,2)")]
    public decimal CostPrice { get; set; }

    [Precision(12, 2)]
    [Column("sellingprice", TypeName = "numeric(12,2)")]
    public decimal SellingPrice { get; set; }

    [Precision(12, 2)]
    [Column("mrp", TypeName = "numeric(12,2)")]
    public decimal? Mrp { get; set; }

    [Precision(12, 3)]
    [Column("reorderlevel", TypeName = "numeric(12,3)")]
    public decimal ReorderLevel { get; set; }

    [Column("isactive")]
    public bool IsActive { get; set; } = true;

    [Column("isonline")]
    public bool IsOnline { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime? UpdatedAt { get; set; }

    [Column("cancelled")]
    public bool Cancelled { get; set; }

    [Column("cancelledon")]
    public DateTime? CancelledOn { get; set; }

    [Column("cancelledreason")]
    public string? CancelledReason { get; set; }

    [ForeignKey(nameof(FkCategory))]
    public Category? Category { get; set; }
}

