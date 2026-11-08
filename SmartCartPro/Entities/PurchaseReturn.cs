using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCartPro.Entities;

/// <summary>Purchase return header. Maps to <c>purchase_returns</c>.</summary>
[Table("purchase_returns")]
public class PurchaseReturn
{
    [Key]
    [Column("id_purchasereturn")]
    public int IdPurchaseReturn { get; set; }

    [Column("fk_store")]
    public int FkStore { get; set; }

    [Column("fk_purchase")]
    public int FkPurchase { get; set; }

    [Column("fk_supplier")]
    public int FkSupplier { get; set; }

    [Required]
    [Column("returnnumber")]
    public string ReturnNumber { get; set; } = string.Empty;

    [Column("returndate")]
    public DateOnly ReturnDate { get; set; }

    /// <summary>Values: Pending | Approved | Cancelled</summary>
    [Column("status")]
    public string Status { get; set; } = "Pending";

    [Precision(12, 2)]
    [Column("totalamount", TypeName = "numeric(12,2)")]
    public decimal TotalAmount { get; set; }

    [Column("reason")]
    public string? Reason { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("createdby")]
    public int CreatedBy { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("postedby")]
    public int? PostedBy { get; set; }

    [Column("postedat")]
    public DateTime? PostedAt { get; set; }

    [Column("cancelled")]
    public bool Cancelled { get; set; }

    [Column("cancelledon")]
    public DateTime? CancelledOn { get; set; }

    [Column("cancelledreason")]
    public string? CancelledReason { get; set; }

    [ForeignKey(nameof(FkStore))]
    public Store? Store { get; set; }

    [ForeignKey(nameof(FkPurchase))]
    public Purchase? Purchase { get; set; }

    [ForeignKey(nameof(FkSupplier))]
    public Supplier? Supplier { get; set; }

    [ForeignKey(nameof(CreatedBy))]
    public User? Creator { get; set; }

    [ForeignKey(nameof(PostedBy))]
    public User? Poster { get; set; }

    public ICollection<PurchaseReturnDetail> Details { get; set; } = new List<PurchaseReturnDetail>();
}

/// <summary>Purchase return item. Maps to <c>purchase_return_details</c>.</summary>
[Table("purchase_return_details")]
public class PurchaseReturnDetail
{
    [Key]
    [Column("id_purchasereturndetail")]
    public int IdPurchaseReturnDetail { get; set; }

    [Column("fk_purchasereturn")]
    public int FkPurchaseReturn { get; set; }

    [Column("fk_purchasedetail")]
    public int FkPurchaseDetail { get; set; }

    [Column("fk_store")]
    public int FkStore { get; set; }

    [Column("fk_product")]
    public int FkProduct { get; set; }

    [Column("fk_productvariant")]
    public int? FkProductVariant { get; set; }

    [Precision(14, 3)]
    [Column("quantity", TypeName = "numeric(14,3)")]
    public decimal Quantity { get; set; }

    [Precision(12, 2)]
    [Column("unitcost", TypeName = "numeric(12,2)")]
    public decimal UnitCost { get; set; }

    [Precision(12, 2)]
    [Column("taxamount", TypeName = "numeric(12,2)")]
    public decimal TaxAmount { get; set; }

    [Precision(12, 2)]
    [Column("linetotal", TypeName = "numeric(12,2)")]
    public decimal LineTotal { get; set; }

    [ForeignKey(nameof(FkPurchaseReturn))]
    public PurchaseReturn? PurchaseReturn { get; set; }

    [ForeignKey(nameof(FkPurchaseDetail))]
    public PurchaseDetail? PurchaseDetail { get; set; }

    [ForeignKey(nameof(FkProduct))]
    public Product? Product { get; set; }

    [ForeignKey(nameof(FkProductVariant))]
    public ProductVariant? ProductVariant { get; set; }
}

