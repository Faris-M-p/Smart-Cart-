using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>Maps to PostgreSQL table <c>company</c> (company-wide root in a client DB).</summary>
[Table("company")]
public class Company
{
    [Key]
    [Column("id_company")]
    public int IdCompany { get; set; }

    [Required]
    [Column("companycode")]
    public string CompanyCode { get; set; } = string.Empty;

    [Required]
    [Column("companyname")]
    public string CompanyName { get; set; } = string.Empty;

    [Column("legalname")]
    public string? LegalName { get; set; }

    [Column("taxnumber")]
    public string? TaxNumber { get; set; }

    [Column("registrationno")]
    public string? RegistrationNo { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("website")]
    public string? Website { get; set; }

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

    [Required]
    [MaxLength(3)]
    [Column("currencycode", TypeName = "char(3)")]
    public string CurrencyCode { get; set; } = "INR";

    [Required]
    [Column("timezone")]
    public string TimeZone { get; set; } = "Asia/Kolkata";

    [Required]
    [Column("locale")]
    public string Locale { get; set; } = "en-IN";

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

    public ICollection<Store> Stores { get; set; } = new List<Store>();
}

