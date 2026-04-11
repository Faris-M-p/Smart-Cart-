using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    [Table("VariantValue")]
    public class VariantValueEntity
    {
        [Key]
        [Column("ID_VariantValue")]
        public int IdVariantValue { get; set; }

        [Column("FK_Variant")]
        public int FkVariant { get; set; }

        [MaxLength(255)]
        public string ValueName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ValueIcon { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool Cancelled { get; set; }

        public DateTime? CancelledOn { get; set; }

        [MaxLength(500)]
        public string? CancelledReason { get; set; }
    }
}
