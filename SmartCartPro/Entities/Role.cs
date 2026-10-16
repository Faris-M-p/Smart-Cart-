using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>Maps to <c>roles</c>.</summary>
[Table("roles")]
public class Role
{
    [Key]
    [Column("id_role")]
    public int IdRole { get; set; }

    [Required]
    [Column("rolecode")]
    public string RoleCode { get; set; } = string.Empty;

    [Required]
    [Column("rolename")]
    public string RoleName { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Column("issystem")]
    public bool IsSystem { get; set; }

    [Column("isactive")]
    public bool IsActive { get; set; } = true;

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime? UpdatedAt { get; set; }

    [Column("cancelled")]
    public bool Cancelled { get; set; }

    [Column("cancelledon")]
    public DateTime? CancelledOn { get; set; }

    [Column("cancelledreason")]
    public string? CancelledReason { get; set; }
}

