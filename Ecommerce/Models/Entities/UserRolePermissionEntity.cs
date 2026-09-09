using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to Database-Postgres/01_Tables/user_role_permissions.sql (userrolepermissions).</summary>
    [Table("userrolepermissions")]
    public class UserRolePermissionEntity
    {
        [Key]
        [Column("id_userrolepermission")]
        public int ID_UserRolePermission { get; set; }

        [Column("fk_userrole")]
        public int FK_UserRole { get; set; }

        [Column("fk_permission")]
        public int FK_Permission { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; }

        [Column("cancelled")]
        public bool Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }

        [MaxLength(255)]
        [Column("cancelledreason")]
        public string? CancelledReason { get; set; }
    }
}
