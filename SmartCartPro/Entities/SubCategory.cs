namespace SmartCartPro.Entities;

/// <summary>Company-wide subcategory. Maps to <c>subcategories</c>.</summary>
public class SubCategory
{
    public int IdSubcategory { get; set; }
    public int FkCategory { get; set; }
    public string SubCategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool Cancelled { get; set; }
    public DateTime? CancelledOn { get; set; }
    public string? CancelledReason { get; set; }

    public Category? Category { get; set; }
}
