namespace SmartCartPro.Entities;

/// <summary>Company-wide product master. Maps to <c>products</c>.</summary>
public class Product
{
    public int IdProduct { get; set; }
    public int? FkCategory { get; set; }
    public int? FkSubcategory { get; set; }
    public int? FkBrand { get; set; }
    public int? FkUnit { get; set; }
    public int? FkTax { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Sku { get; set; }
    public string? Barcode { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal? Mrp { get; set; }
    public decimal ReorderLevel { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsOnline { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool Cancelled { get; set; }
    public DateTime? CancelledOn { get; set; }
    public string? CancelledReason { get; set; }

    public Category? Category { get; set; }
}
