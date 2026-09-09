using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to Database-Postgres/01_Tables/sku_media.sql (skumedia).</summary>
    [Table("skumedia")]
    public class ProductVariantImageEntity
    {
        [Key]
        [Column("id_skumedia")]
        public int ID_ProductVariantImage { get; set; }

        [Column("fk_productsku")]
        public int FK_ProductVariant { get; set; }

        [Required]
        [MaxLength(20)]
        [Column("mediatype")]
        public string MediaType { get; set; } = "Image";

        [Required]
        [MaxLength(500)]
        [Column("mediaurl")]
        public string ImageUrl { get; set; } = string.Empty;

        [Column("isprimary")]
        public bool IsPrimary { get; set; }

        [Column("displayorder")]
        public int DisplayOrder { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updatedat")]
        public DateTime? UpdatedAt { get; set; }
    }
}
