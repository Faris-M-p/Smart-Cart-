using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>Company-wide category. Maps to <c>categories</c>.</summary>
[Table("categories")]
public class Category
{
    [Key]
    [Column("id_category")]
    public int IdCategory { get; set; }

    [Column("categorycode")]
    public string? CategoryCode { get; set; }

    [Required]
    [Column("categoryname")]
    public string CategoryName { get; set; } = string.Empty;

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

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

