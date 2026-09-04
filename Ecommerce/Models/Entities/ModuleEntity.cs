using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>
    /// Maps to [dbo].[Modules] (see Database/01_Tables/Modules.sql).
    /// </summary>
    [Table("Modules")]
    public class ModuleEntity
    {
        [Key]
        public int ID_Module { get; set; }

        [MaxLength(100)]
        public string ModuleName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string DisplayName { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public bool Cancelled { get; set; }

        public DateTime? CancelledOn { get; set; }

        [MaxLength(255)]
        public string? CancelledReason { get; set; }
    }
}
