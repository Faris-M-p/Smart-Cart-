using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>Maps to <c>user_roles</c>.</summary>
[Table("user_roles")]
public class UserRole
{
    [Key]
    [Column("id_userrole")]
    public int IdUserRole { get; set; }

    [Column("fk_user")]
    public int FkUser { get; set; }

    [Column("fk_role")]
    public int FkRole { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(FkUser))]
    public User? User { get; set; }

    [ForeignKey(nameof(FkRole))]
    public Role? Role { get; set; }
}

