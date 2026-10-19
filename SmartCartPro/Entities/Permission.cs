using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>Maps to <c>permissions</c>. Platform-wide permission catalog (one per module action).</summary>
[Table("permissions")]
public class Permission
{
    [Key]
    [Column("id_permission")]
    public int IdPermission { get; set; }

    [Column("fk_module")]
    public int FkModule { get; set; }

    [Required]
    [Column("permissioncode")]
    public string PermissionCode { get; set; } = string.Empty;

    [Required]
    [Column("permissionname")]
    public string PermissionName { get; set; } = string.Empty;

    [Column("displayorder")]
    public int DisplayOrder { get; set; }

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

    [ForeignKey(nameof(FkModule))]
    public Module? Module { get; set; }
}
