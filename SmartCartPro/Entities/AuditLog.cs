using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>Maps to <c>audit_logs</c>. Immutable audit trail for admin actions.</summary>
[Table("audit_logs")]
public class AuditLog
{
    [Key]
    [Column("id_auditlog")]
    public int IdAuditLog { get; set; }

    [Column("fk_store")]
    public int? FkStore { get; set; }

    [Column("fk_user")]
    public int? FkUser { get; set; }

    [Required]
    [Column("entityname")]
    public string EntityName { get; set; } = string.Empty;

    [Column("entityid")]
    public int? EntityId { get; set; }

    [Required]
    [Column("action")]
    public string Action { get; set; } = string.Empty;

    [Column("details")]
    public string? Details { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(FkUser))]
    public User? User { get; set; }
}
