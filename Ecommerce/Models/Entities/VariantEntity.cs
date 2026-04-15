using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to <c>Variants</c> (see user schema: ID_Variant, Name, Description, DisplayOrder, IsActive, Cancelled, CancelledOn).</summary>
    [Table("Variants")]
    public class VariantEntity
    {
        [Key]
        public int ID_Variant { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;

        public bool Cancelled { get; set; }

        public DateTime? CancelledOn { get; set; }
    }
}
