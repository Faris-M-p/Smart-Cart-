using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>
    /// Maps to [dbo].[Category] (see xPROCEDURExTABLES/Tables/Categories.sql).
    /// </summary>
    [Table("Category")]
    public class CategoryEntity
    {
        [Key]
        [Column("ID_Category")]
        public int IdCategory { get; set; }

        [Column("Name")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public bool Cancelled { get; set; }

        public DateTime? CancelledOn { get; set; }

        [MaxLength(255)]
        public string? CancelledReason { get; set; }
    }
}
