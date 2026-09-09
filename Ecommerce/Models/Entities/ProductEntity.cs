using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to Database-Postgres/01_Tables/products.sql (products).</summary>
    [Table("products")]
    public class ProductEntity
    {
        [Key]
        [Column("id_product")]
        public int ID_Product { get; set; }

        [Column("fk_subcategory")]
        public int FK_SubCategory { get; set; }

        [Column("fk_brand")]
        public int? FK_Brand { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        [Column("slug")]
        public string Slug { get; set; } = string.Empty;

        [Column("description")]
        public string? Description { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        [Column("sellonline")]
        public bool SellOnline { get; set; }

        [Column("createdat")]
        public DateTime? CreatedAt { get; set; }

        [Column("modifiedat")]
        public DateTime? ModifiedAt { get; set; }

        [Column("cancelled")]
        public bool Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }
    }
}
