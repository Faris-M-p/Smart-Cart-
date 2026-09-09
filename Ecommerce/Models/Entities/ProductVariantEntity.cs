using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to Database-Postgres/01_Tables/product_variants.sql (productvariants).</summary>
    [Table("productvariants")]
    public class ProductVariantEntity
    {
        [Key]
        [Column("id_productvariant")]
        public int ID_ProductVariant { get; set; }

        [Column("fk_product")]
        public int FK_Product { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("sku")]
        public string SKU { get; set; } = string.Empty;

        [MaxLength(100)]
        [Column("barcode")]
        public string? Barcode { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("variantlabel")]
        public string VariantLabel { get; set; } = string.Empty;

        [MaxLength(1000)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("mrp")]
        public decimal MRP { get; set; }

        [Column("sellingprice", TypeName = "decimal(10,2)")]
        public decimal SellingPrice { get; set; }

        [MaxLength(20)]
        [Column("unitofmeasure")]
        public string? UnitOfMeasure { get; set; }

        [Column("unitvalue", TypeName = "decimal(10,3)")]
        public decimal? UnitValue { get; set; }

        [Column("isdefault")]
        public bool IsDefault { get; set; }

        [Column("maxorderqty")]
        public int MaxOrderQty { get; set; } = 10;

        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        [Column("sellonline")]
        public bool SellOnline { get; set; }

        [Column("createdat")]
        public DateTime? CreatedAt { get; set; }

        [Column("cancelled")]
        public bool Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }
    }
}
