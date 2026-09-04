using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>
    /// Maps to [dbo].[Permissions] (see Database/01_Tables/Permissions.sql).
    /// </summary>
    [Table("Permissions")]
    public class PermissionEntity
    {
        [Key]
        public int ID_Permission { get; set; }

        public int FK_Module { get; set; }

        [MaxLength(100)]
        public string PermissionName { get; set; } = string.Empty;

        [MaxLength(150)]
        public string PermissionCode { get; set; } = string.Empty;

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
