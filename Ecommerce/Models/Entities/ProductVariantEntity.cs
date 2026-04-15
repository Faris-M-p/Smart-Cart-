using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to <c>ProductVariants</c> (see xPROCEDURExTABLES/Tables/ProductVariants.sql).</summary>
    [Table("ProductVariants")]
    public class ProductVariantEntity
    {
        [Key]
        public int ID_ProductVariant { get; set; }

        public int FK_Product { get; set; }

        [Required]
        [MaxLength(100)]
        public string SKU { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Barcode { get; set; }

        [Required]
        [MaxLength(255)]
        public string VariantLabel { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public decimal MRP { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "decimal(10,2)")]
        public decimal SellingPrice { get; set; }

        [MaxLength(20)]
        public string? UnitOfMeasure { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.Column(TypeName = "decimal(10,3)")]
        public decimal? UnitValue { get; set; }

        public bool IsDefault { get; set; }

        public int MaxOrderQty { get; set; } = 10;

        public bool IsActive { get; set; } = true;

        public DateTime? CreatedAt { get; set; }

        public bool Cancelled { get; set; }

        public DateTime? CancelledOn { get; set; }
    }
}
