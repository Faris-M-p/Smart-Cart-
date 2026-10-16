using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>Company-wide subcategory. Maps to <c>subcategories</c>.</summary>
[Table("subcategories")]
public class SubCategory
{
    [Key]
    [Column("id_subcategory")]
    public int IdSubcategory { get; set; }

    [Column("fk_category")]
    public int FkCategory { get; set; }

    [Required]
    [Column("subcategoryname")]
    public string SubCategoryName { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

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

    [ForeignKey(nameof(FkCategory))]
    public Category? Category { get; set; }
}

