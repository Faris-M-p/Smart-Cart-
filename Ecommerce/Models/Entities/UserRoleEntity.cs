using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to Database-Postgres/01_Tables/user_roles.sql (userroles).</summary>
    [Table("userroles")]
    public class UserRoleEntity
    {
        [Key]
        [Column("id_userrole")]
        public int ID_UserRole { get; set; }

        [MaxLength(100)]
        [Column("rolename")]
        public string RoleName { get; set; } = string.Empty;

        [MaxLength(1000)]
        [Column("description")]
        public string? Description { get; set; }

        [Column("issystemrole")]
        public bool IsSystemRole { get; set; }

        [Column("isactive")]
        public bool IsActive { get; set; }

        [Column("createdat")]
        public DateTime CreatedAt { get; set; }

        [Column("updatedat")]
        public DateTime? UpdatedAt { get; set; }

        [Column("cancelled")]
        public bool Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }

        [MaxLength(255)]
        [Column("cancelledreason")]
        public string? CancelledReason { get; set; }
    }
}
