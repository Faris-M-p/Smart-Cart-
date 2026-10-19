using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCartPro.Entities;

/// <summary>Product variant (e.g. size/colour combo). Maps to <c>product_variants</c>.</summary>
[Table("product_variants")]
public class ProductVariant
{
    [Key]
    [Column("id_productvariant")]
    public int IdProductVariant { get; set; }

    [Column("fk_product")]
    public int FkProduct { get; set; }

    [Required]
    [Column("variantname")]
    public string VariantName { get; set; } = string.Empty;

    [Column("skucode")]
    public string? SkuCode { get; set; }

    [Column("barcode")]
    public string? Barcode { get; set; }

    [Precision(12, 2)]
    [Column("additionalprice", TypeName = "numeric(12,2)")]
    public decimal AdditionalPrice { get; set; }

    [Column("isactive")]
    public bool IsActive { get; set; } = true;

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

    [ForeignKey(nameof(FkProduct))]
    public Product? Product { get; set; }
}
