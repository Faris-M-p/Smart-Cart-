using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    [Table("ProductVariant")]
    public class ProductVariantEntity
    {
        [Key]
        [Column("ID_ProductVariant")]
        public int IdProductVariant { get; set; }

        [Column("FK_Product")]
        public int FkProduct { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceAdjustment { get; set; }

        public bool IsDefault { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool? Cancelled { get; set; }
    }
}
