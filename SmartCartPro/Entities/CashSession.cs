using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCartPro.Entities;

/// <summary>POS cash drawer session. Maps to <c>cash_sessions</c>.</summary>
[Table("cash_sessions")]
public class CashSession
{
    [Key]
    [Column("id_cashsession")]
    public int IdCashSession { get; set; }

    [Column("fk_store")]
    public int FkStore { get; set; }

    [Column("fk_user")]
    public int FkUser { get; set; }

    [Column("openedat")]
    public DateTime OpenedAt { get; set; }

    [Column("closedat")]
    public DateTime? ClosedAt { get; set; }

    [Precision(12, 2)]
    [Column("openingfloat", TypeName = "numeric(12,2)")]
    public decimal OpeningFloat { get; set; }

    [Precision(12, 2)]
    [Column("closingcash", TypeName = "numeric(12,2)")]
    public decimal? ClosingCash { get; set; }

    [Precision(12, 2)]
    [Column("expectedcash", TypeName = "numeric(12,2)")]
    public decimal? ExpectedCash { get; set; }

    [Precision(12, 2)]
    [Column("variance", TypeName = "numeric(12,2)")]
    public decimal? Variance { get; set; }

    /// <summary>Values: open | closed</summary>
    [Column("status")]
    public string Status { get; set; } = "open";

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(FkStore))]
    public Store? Store { get; set; }

    [ForeignKey(nameof(FkUser))]
    public User? User { get; set; }
}
