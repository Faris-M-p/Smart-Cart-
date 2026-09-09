using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to Database-Postgres/01_Tables/permissions.sql (permissions).</summary>
    [Table("permissions")]
    public class PermissionEntity
    {
        [Key]
        [Column("id_permission")]
        public int ID_Permission { get; set; }

        [Column("fk_module")]
        public int FK_Module { get; set; }

        [MaxLength(100)]
        [Column("permissionname")]
        public string PermissionName { get; set; } = string.Empty;

        [MaxLength(150)]
        [Column("permissioncode")]
        public string PermissionCode { get; set; } = string.Empty;

        [Column("displayorder")]
        public int DisplayOrder { get; set; }

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
