using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCartPro.Entities;

/// <summary>Actual purchase document header. Maps to <c>purchases</c>.</summary>
[Table("purchases")]
public class Purchase
{
    [Key]
    [Column("id_purchase")]
    public int IdPurchase { get; set; }

    [Column("fk_store")]
    public int FkStore { get; set; }

    [Column("fk_supplier")]
    public int FkSupplier { get; set; }

    [Column("purchasedate")]
    public DateOnly PurchaseDate { get; set; }

    [Column("invoicenumber")]
    public string? InvoiceNumber { get; set; }

    [Column("invoicedate")]
    public DateOnly? InvoiceDate { get; set; }

    [Precision(12, 2)]
    [Column("subtotal", TypeName = "numeric(12,2)")]
    public decimal SubTotal { get; set; }

    [Precision(12, 2)]
    [Column("discountamount", TypeName = "numeric(12,2)")]
    public decimal DiscountAmount { get; set; }

    [Precision(12, 2)]
    [Column("taxableamount", TypeName = "numeric(12,2)")]
    public decimal TaxableAmount { get; set; }

    [Precision(12, 2)]
    [Column("taxamount", TypeName = "numeric(12,2)")]
    public decimal TaxAmount { get; set; }

    [Precision(12, 2)]
    [Column("charges", TypeName = "numeric(12,2)")]
    public decimal Charges { get; set; }

    [Precision(12, 2)]
    [Column("roundoff", TypeName = "numeric(12,2)")]
    public decimal RoundOff { get; set; }

    [Precision(12, 2)]
    [Column("totalamount", TypeName = "numeric(12,2)")]
    public decimal TotalAmount { get; set; }

    [Precision(12, 2)]
    [Column("amountpaid", TypeName = "numeric(12,2)")]
    public decimal AmountPaid { get; set; }

    [Column("paymentmethod")]
    public string? PaymentMethod { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    /// <summary>Values: Pending | Approved | Cancelled</summary>
    [Column("status")]
    public string Status { get; set; } = "Pending";

    [Column("createdby")]
    public int? CreatedBy { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime? UpdatedAt { get; set; }

    [Column("approvedby")]
    public int? ApprovedBy { get; set; }

    [Column("approvedat")]
    public DateTime? ApprovedAt { get; set; }

    [Column("cancelled")]
    public bool Cancelled { get; set; }

    [Column("cancelledon")]
    public DateTime? CancelledOn { get; set; }

    [Column("cancelledby")]
    public int? CancelledBy { get; set; }

    [Column("cancelledreason")]
    public string? CancelledReason { get; set; }

    [ForeignKey(nameof(FkStore))]
    public Store? Store { get; set; }

    [ForeignKey(nameof(FkSupplier))]
    public Supplier? Supplier { get; set; }

    [ForeignKey(nameof(CreatedBy))]
    public User? Creator { get; set; }

    [ForeignKey(nameof(ApprovedBy))]
    public User? Approver { get; set; }

    public ICollection<PurchaseDetail> Details { get; set; } = new List<PurchaseDetail>();
}
