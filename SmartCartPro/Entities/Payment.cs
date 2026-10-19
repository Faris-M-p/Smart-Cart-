using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCartPro.Entities;

/// <summary>Payment record linked to a sale. Maps to <c>payments</c>.</summary>
[Table("payments")]
public class Payment
{
    [Key]
    [Column("id_payment")]
    public int IdPayment { get; set; }

    [Column("fk_store")]
    public int FkStore { get; set; }

    [Column("fk_sale")]
    public int? FkSale { get; set; }

    [Column("fk_cashsession")]
    public int? FkCashSession { get; set; }

    /// <summary>Values: cash|card|upi|wallet|bank|other</summary>
    [Required]
    [Column("paymentmethod")]
    public string PaymentMethod { get; set; } = string.Empty;

    [Precision(12, 2)]
    [Column("amount", TypeName = "numeric(12,2)")]
    public decimal Amount { get; set; }

    [Column("referenceno")]
    public string? ReferenceNo { get; set; }

    [Column("paidat")]
    public DateTime PaidAt { get; set; }

    [Column("createdby")]
    public int? CreatedBy { get; set; }

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

    [ForeignKey(nameof(FkStore))]
    public Store? Store { get; set; }

    [ForeignKey(nameof(FkSale))]
    public Sale? Sale { get; set; }

    [ForeignKey(nameof(FkCashSession))]
    public CashSession? CashSession { get; set; }
}
