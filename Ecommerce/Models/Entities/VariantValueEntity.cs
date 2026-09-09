using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to Database-Postgres/01_Tables/variant_values.sql (variantvalues).</summary>
    [Table("variantvalues")]
    public class VariantValueEntity
    {
        [Key]
        [Column("id_variantvalue")]
        public int ID_VariantValue { get; set; }

        [Column("fk_variant")]
        public int FK_Variant { get; set; }

        [Required]
        [MaxLength(255)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("displayorder")]
        public int DisplayOrder { get; set; }

        [Column("cancelled")]
        public bool Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }
    }
}
