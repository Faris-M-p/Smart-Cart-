using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>Maps to PostgreSQL table <c>store</c>. Column <c>fk_company</c> → <see cref="FkCompany"/>.</summary>
[Table("store")]
public class Store
{
    [Key]
    [Column("id_store")]
    public int IdStore { get; set; }

    /// <summary>SQL column <c>fk_company</c>.</summary>
    [Column("fk_company")]
    public int FkCompany { get; set; }

    [Required]
    [Column("storecode")]
    public string StoreCode { get; set; } = string.Empty;

    [Required]
    [Column("storename")]
    public string StoreName { get; set; } = string.Empty;

    [Column("email")]
    public string? Email { get; set; }

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("addressline1")]
    public string? AddressLine1 { get; set; }

    [Column("addressline2")]
    public string? AddressLine2 { get; set; }

    [Column("city")]
    public string? City { get; set; }

    [Column("state")]
    public string? State { get; set; }

    [Column("postalcode")]
    public string? PostalCode { get; set; }

    [Column("country")]
    public string? Country { get; set; }

    [Column("isdefault")]
    public bool IsDefault { get; set; }

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

    [ForeignKey(nameof(FkCompany))]
    public Company? Company { get; set; }
}

