namespace SmartCartPro.Entities;

/// <summary>Maps to PostgreSQL table <c>store</c>. Column <c>fk_company</c> → <see cref="FkCompany"/>.</summary>
public class Store
{
    public int IdStore { get; set; }

    /// <summary>SQL column <c>fk_company</c>.</summary>
    public int FkCompany { get; set; }

    public string StoreCode { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool Cancelled { get; set; }
    public DateTime? CancelledOn { get; set; }
    public string? CancelledReason { get; set; }

    public Company? Company { get; set; }
}
