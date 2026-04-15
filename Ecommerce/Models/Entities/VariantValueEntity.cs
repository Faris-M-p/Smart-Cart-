using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to <c>VariantValues</c> (ID_VariantValue, FK_Variant, Name, Description, DisplayOrder, Cancelled, CancelledOn).</summary>
    [Table("VariantValues")]
    public class VariantValueEntity
    {
        [Key]
        public int ID_VariantValue { get; set; }

        public int FK_Variant { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; }

        public bool Cancelled { get; set; }

        public DateTime? CancelledOn { get; set; }
    }
}
