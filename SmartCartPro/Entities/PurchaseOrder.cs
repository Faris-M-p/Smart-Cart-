using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCartPro.Entities;

/// <summary>Purchase order requirement list. Maps to <c>purchase_orders</c>.</summary>
[Table("purchase_orders")]
public class PurchaseOrder
{
    [Key]
    [Column("id_purchaseorder")]
    public int IdPurchaseOrder { get; set; }

    [Column("fk_store")]
    public int FkStore { get; set; }

    [Required]
    [Column("ordernumber")]
    public string OrderNumber { get; set; } = string.Empty;

    [Column("orderdate")]
    public DateOnly OrderDate { get; set; }

    /// <summary>Optional supplier for internal requirement list.</summary>
    [Column("fk_supplier")]
    public int? FkSupplier { get; set; }

    /// <summary>Values: Open | Closed | Cancelled</summary>
    [Column("status")]
    public string Status { get; set; } = "Open";

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("createdby")]
    public int CreatedBy { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(FkStore))]
    public Store? Store { get; set; }

    [ForeignKey(nameof(FkSupplier))]
    public Supplier? Supplier { get; set; }

    [ForeignKey(nameof(CreatedBy))]
    public User? Creator { get; set; }

    public ICollection<PurchaseOrderDetail> Details { get; set; } = new List<PurchaseOrderDetail>();
}

/// <summary>Purchase order item detail. Maps to <c>purchase_order_details</c>.</summary>
[Table("purchase_order_details")]
public class PurchaseOrderDetail
{
    [Key]
    [Column("id_purchaseorderdetail")]
    public int IdPurchaseOrderDetail { get; set; }

    [Column("fk_purchaseorder")]
    public int FkPurchaseOrder { get; set; }

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

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [ForeignKey(nameof(FkPurchaseOrder))]
    public PurchaseOrder? PurchaseOrder { get; set; }

    [ForeignKey(nameof(FkProduct))]
    public Product? Product { get; set; }

    [ForeignKey(nameof(FkProductVariant))]
    public ProductVariant? ProductVariant { get; set; }
}

