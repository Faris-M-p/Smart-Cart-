using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    [Table("Variant")]
    public class VariantEntity
    {
        [Key]
        [Column("ID_Variant")]
        public int IdVariant { get; set; }

        [MaxLength(255)]
        public string VariantName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool Cancelled { get; set; }

        public DateTime? CancelledOn { get; set; }

        [MaxLength(500)]
        public string? CancelledReason { get; set; }
    }
}
