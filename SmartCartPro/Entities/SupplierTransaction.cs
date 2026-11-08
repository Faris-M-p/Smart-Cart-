using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCartPro.Entities;

/// <summary>
/// Single supplier financial ledger entry. Maps to <c>supplier_transactions</c>.
/// Convention: Positive (+) = supplier owes our business / credit; Negative (-) = our business owes the supplier (payable).
/// </summary>
[Table("supplier_transactions")]
public class SupplierTransaction
{
    [Key]
    [Column("id_suppliertransaction")]
    public int IdSupplierTransaction { get; set; }

    [Column("fk_supplier")]
    public int FkSupplier { get; set; }

    [Column("fk_store")]
    public int FkStore { get; set; }

    [Required]
    [Column("transactionnumber")]
    public string TransactionNumber { get; set; } = string.Empty;

    [Column("transactiondate")]
    public DateOnly TransactionDate { get; set; }

    /// <summary>Values: Purchase | Return | Payment | Refund | Opening | Reversal</summary>
    [Column("transactiontype")]
    public string TransactionType { get; set; } = string.Empty;

    [Column("sourcetype")]
    public string? SourceType { get; set; }

    [Column("sourceid")]
    public int? SourceId { get; set; }

    [Precision(12, 2)]
    [Column("amount", TypeName = "numeric(12,2)")]
    public decimal Amount { get; set; }

    [Precision(12, 2)]
    [Column("signedamount", TypeName = "numeric(12,2)")]
    public decimal SignedAmount { get; set; }

    [Column("paymentmethod")]
    public string? PaymentMethod { get; set; }

    [Column("referenceno")]
    public string? ReferenceNo { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("fk_reversal_of")]
    public int? FkReversalOf { get; set; }

    [Column("createdby")]
    public int CreatedBy { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(FkSupplier))]
    public Supplier? Supplier { get; set; }

    [ForeignKey(nameof(FkStore))]
    public Store? Store { get; set; }

    [ForeignKey(nameof(CreatedBy))]
    public User? Creator { get; set; }

    [ForeignKey(nameof(FkReversalOf))]
    public SupplierTransaction? ReversalOf { get; set; }
}

