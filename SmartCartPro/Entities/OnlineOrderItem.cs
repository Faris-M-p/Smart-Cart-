using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCartPro.Entities;

/// <summary>Line item for an online storefront order. Maps to <c>online_order_items</c>.</summary>
[Table("online_order_items")]
public class OnlineOrderItem
{
    [Key]
    [Column("id_onlineorderitem")]
    public int IdOnlineOrderItem { get; set; }

    [Column("fk_onlineorder")]
    public int FkOnlineOrder { get; set; }

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
    [Column("unitprice", TypeName = "numeric(12,2)")]
    public decimal UnitPrice { get; set; }

    [Precision(12, 2)]
    [Column("linetotal", TypeName = "numeric(12,2)")]
    public decimal LineTotal { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(FkOnlineOrder))]
    public Order? Order { get; set; }

    [ForeignKey(nameof(FkProduct))]
    public Product? Product { get; set; }
}
