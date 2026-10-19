using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>Company-wide supplier master. Maps to <c>suppliers</c>.</summary>
[Table("suppliers")]
public class Supplier
{
    [Key]
    [Column("id_supplier")]
    public int IdSupplier { get; set; }

    [Column("suppliercode")]
    public string? SupplierCode { get; set; }

    [Required]
    [Column("suppliername")]
    public string SupplierName { get; set; } = string.Empty;

    [Column("contactperson")]
    public string? ContactPerson { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("addressline1")]
    public string? AddressLine1 { get; set; }

    [Column("city")]
    public string? City { get; set; }

    [Column("state")]
    public string? State { get; set; }

    [Column("postalcode")]
    public string? PostalCode { get; set; }

    [Column("country")]
    public string? Country { get; set; }

    [Column("taxnumber")]
    public string? TaxNumber { get; set; }

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
