using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>Company-wide customer master (walk-in + online). Maps to <c>customers</c>.</summary>
[Table("customers")]
public class Customer
{
    [Key]
    [Column("id_customer")]
    public int IdCustomer { get; set; }

    [Column("customercode")]
    public string? CustomerCode { get; set; }

    [Required]
    [Column("fullname")]
    public string FullName { get; set; } = string.Empty;

    [Column("email")]
    public string? Email { get; set; }

    [Column("phone")]
    public string? Phone { get; set; }

    [Column("passwordhash")]
    public string? PasswordHash { get; set; }

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
