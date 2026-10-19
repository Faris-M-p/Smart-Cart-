using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>Company-wide brand master. Maps to <c>brands</c>.</summary>
[Table("brands")]
public class Brand
{
    [Key]
    [Column("id_brand")]
    public int IdBrand { get; set; }

    [Required]
    [Column("brandname")]
    public string BrandName { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

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
