using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to Database-Postgres/01_Tables/product_media.sql (productmedia).</summary>
    [Table("productmedia")]
    public class ProductMediaEntity
    {
        [Key]
        [Column("id_productmedia")]
        public int ID_ProductMedia { get; set; }

        [Column("fk_product")]
        public int FK_Product { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("mediatype")]
        public string MediaType { get; set; } = "Image";

        [Required]
        [MaxLength(500)]
        [Column("mediaurl")]
        public string MediaUrl { get; set; } = string.Empty;

        [Column("displayorder")]
        public int DisplayOrder { get; set; }

        [Column("isprimary")]
        public bool IsPrimary { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updatedat")]
        public DateTime? UpdatedAt { get; set; }
    }
}
