using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to Database-Postgres/01_Tables/brand.sql (brand).</summary>
    [Table("brand")]
    public class BrandEntity
    {
        [Key]
        [Column("id_brand")]
        public int ID_Brand { get; set; }

        [MaxLength(100)]
        [Column("brandname")]
        public string BrandName { get; set; } = string.Empty;

        [MaxLength(1000)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; }

        [MaxLength(1000)]
        [Column("imageurl")]
        public string? ImageUrl { get; set; }

        [Column("cancelled")]
        public bool Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }

        [MaxLength(255)]
        [Column("cancelledreason")]
        public string? CancelledReason { get; set; }
    }
}
