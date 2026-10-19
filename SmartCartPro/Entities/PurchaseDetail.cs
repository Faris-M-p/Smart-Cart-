using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCartPro.Entities;

/// <summary>Purchase order line item. Maps to <c>purchase_details</c>.</summary>
[Table("purchase_details")]
public class PurchaseDetail
{
    [Key]
    [Column("id_purchasedetail")]
    public int IdPurchaseDetail { get; set; }

    [Column("fk_purchase")]
    public int FkPurchase { get; set; }

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

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(FkPurchase))]
    public Purchase? Purchase { get; set; }

    [ForeignKey(nameof(FkProduct))]
    public Product? Product { get; set; }
}
