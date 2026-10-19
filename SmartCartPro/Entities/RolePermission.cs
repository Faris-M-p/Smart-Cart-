using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>Maps to <c>role_permissions</c>. Join table: role ↔ permission.</summary>
[Table("role_permissions")]
public class RolePermission
{
    [Key]
    [Column("id_rolepermission")]
    public int IdRolePermission { get; set; }

    [Column("fk_role")]
    public int FkRole { get; set; }

    [Column("fk_permission")]
    public int FkPermission { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(FkRole))]
    public Role? Role { get; set; }

    [ForeignKey(nameof(FkPermission))]
    public Permission? Permission { get; set; }
}
