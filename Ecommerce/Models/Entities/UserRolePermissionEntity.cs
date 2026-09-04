using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>
    /// Maps to [dbo].[UserRolePermissions] (see Database/01_Tables/UserRolePermissions.sql).
    /// </summary>
    [Table("UserRolePermissions")]
    public class UserRolePermissionEntity
    {
        [Key]
        public int ID_UserRolePermission { get; set; }

        public int FK_UserRole { get; set; }

        public int FK_Permission { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool Cancelled { get; set; }

        public DateTime? CancelledOn { get; set; }

        [MaxLength(255)]
        public string? CancelledReason { get; set; }
    }
}
