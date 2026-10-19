using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>Company-wide unit of measure. Maps to <c>units</c>.</summary>
[Table("units")]
public class Unit
{
    [Key]
    [Column("id_unit")]
    public int IdUnit { get; set; }

    [Required]
    [Column("unitcode")]
    public string UnitCode { get; set; } = string.Empty;

    [Required]
    [Column("unitname")]
    public string UnitName { get; set; } = string.Empty;

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
