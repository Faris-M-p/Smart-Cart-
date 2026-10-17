using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>Maps to <c>license</c>. One row per installation.</summary>
[Table("license")]
public class License
{
    [Key]
    [Column("id_license")]
    public int IdLicense { get; set; }

    [Required]
    [Column("licensekey")]
    public string LicenseKey { get; set; } = string.Empty;

    [Required]
    [Column("license_type")]
    public string LicenseType { get; set; } = "standard";

    /// <summary>active | suspended | expired | trial</summary>
    [Required]
    [Column("status")]
    public string Status { get; set; } = "active";

    [Column("startdate")]
    public DateOnly StartDate { get; set; }

    [Column("expirydate")]
    public DateOnly ExpiryDate { get; set; }

    [Column("maxstores")]
    public int MaxStores { get; set; } = 1;

    [Column("notes")]
    public string? Notes { get; set; }

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

    /// <summary>Derived: true when active and not expired and not cancelled.</summary>
    [NotMapped]
    public bool IsValid =>
        !Cancelled
        && Status == "active"
        && ExpiryDate >= DateOnly.FromDateTime(DateTime.UtcNow);

    [NotMapped]
    public int DaysUntilExpiry =>
        (int)(ExpiryDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc)
              - DateTime.UtcNow).TotalDays;
}
