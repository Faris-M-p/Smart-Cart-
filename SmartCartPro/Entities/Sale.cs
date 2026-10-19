using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCartPro.Entities;

/// <summary>POS invoice / sale header. Maps to <c>sales</c>.</summary>
[Table("sales")]
public class Sale
{
    [Key]
    [Column("id_sale")]
    public int IdSale { get; set; }

    [Column("fk_store")]
    public int FkStore { get; set; }

    [Column("fk_customer")]
    public int? FkCustomer { get; set; }

    [Column("fk_cashsession")]
    public int? FkCashSession { get; set; }

    [Required]
    [Column("invoicenumber")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Column("saledate")]
    public DateTime SaleDate { get; set; }

    [Precision(12, 2)]
    [Column("subtotal", TypeName = "numeric(12,2)")]
    public decimal SubTotal { get; set; }

    [Precision(12, 2)]
    [Column("taxamount", TypeName = "numeric(12,2)")]
    public decimal TaxAmount { get; set; }

    [Precision(12, 2)]
    [Column("discountamount", TypeName = "numeric(12,2)")]
    public decimal DiscountAmount { get; set; }

    [Precision(12, 2)]
    [Column("totalamount", TypeName = "numeric(12,2)")]
    public decimal TotalAmount { get; set; }

    [Precision(12, 2)]
    [Column("paidamount", TypeName = "numeric(12,2)")]
    public decimal PaidAmount { get; set; }

    [Precision(12, 2)]
    [Column("changegiven", TypeName = "numeric(12,2)")]
    public decimal ChangeGiven { get; set; }

    /// <summary>Values: completed | cancelled | held</summary>
    [Column("status")]
    public string Status { get; set; } = "completed";

    [Column("notes")]
    public string? Notes { get; set; }

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

    [ForeignKey(nameof(FkCustomer))]
    public Customer? Customer { get; set; }

    [ForeignKey(nameof(FkCashSession))]
    public CashSession? CashSession { get; set; }

    public ICollection<SaleDetail> Details { get; set; } = new List<SaleDetail>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
