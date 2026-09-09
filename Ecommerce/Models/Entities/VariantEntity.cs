using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to Database-Postgres/01_Tables/variants.sql (variants).</summary>
    [Table("variants")]
    public class VariantEntity
    {
        [Key]
        [Column("id_variant")]
        public int ID_Variant { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("displayorder")]
        public int DisplayOrder { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        [Column("cancelled")]
        public bool Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }
    }
}
