using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCartPro.Entities;

/// <summary>Company-wide tax configuration. Maps to <c>taxes</c>.</summary>
[Table("taxes")]
public class Tax
{
    [Key]
    [Column("id_tax")]
    public int IdTax { get; set; }

    [Required]
    [Column("taxcode")]
    public string TaxCode { get; set; } = string.Empty;

    [Required]
    [Column("taxname")]
    public string TaxName { get; set; } = string.Empty;

    [Precision(7, 4)]
    [Column("taxrate", TypeName = "numeric(7,4)")]
    public decimal TaxRate { get; set; }

    [Column("isinclusive")]
    public bool IsInclusive { get; set; }

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
