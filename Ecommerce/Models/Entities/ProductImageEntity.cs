using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to Database-Postgres/01_Tables/product_images.sql (productimages).</summary>
    [Table("productimages")]
    public class ProductImageEntity
    {
        [Key]
        [Column("id_productimage")]
        public int ID_ProductImage { get; set; }

        [Column("fk_product")]
        public int FK_Product { get; set; }

        [Column("imageurl")]
        public string ImageUrl { get; set; } = string.Empty;

        [Column("cancelled")]
        public bool? Cancelled { get; set; }
    }
}
