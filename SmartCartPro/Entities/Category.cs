namespace SmartCartPro.Entities;

/// <summary>Company-wide category. Maps to <c>categories</c>.</summary>
public class Category
{
    public int IdCategory { get; set; }
    public string? CategoryCode { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool Cancelled { get; set; }
    public DateTime? CancelledOn { get; set; }
    public string? CancelledReason { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
