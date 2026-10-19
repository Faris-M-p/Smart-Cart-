using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCartPro.Entities;

/// <summary>
/// Sample storefront order entity mapped to <c>online_orders</c> (store-scoped via <c>fk_store</c>).
/// </summary>
[Table("online_orders")]
public class Order
{
    [Key]
    [Column("id_onlineorder")]
    public int IdOnlineOrder { get; set; }

    /// <summary>SQL column <c>fk_store</c>.</summary>
    [Column("fk_store")]
    public int FkStore { get; set; }

    /// <summary>SQL column <c>fk_customer</c>.</summary>
    [Column("fk_customer")]
    public int FkCustomer { get; set; }

    [Required]
    [Column("ordernumber")]
    public string OrderNumber { get; set; } = string.Empty;

    [Column("orderdate")]
    public DateTime OrderDate { get; set; }

    [Required]
    [Column("status")]
    public string Status { get; set; } = "pending";

    [Precision(12, 2)]
    [Column("subtotal", TypeName = "numeric(12,2)")]
    public decimal Subtotal { get; set; }

    [Precision(12, 2)]
    [Column("taxamount", TypeName = "numeric(12,2)")]
    public decimal TaxAmount { get; set; }

    [Precision(12, 2)]
    [Column("shippingamount", TypeName = "numeric(12,2)")]
    public decimal ShippingAmount { get; set; }

    [Precision(12, 2)]
    [Column("discountamount", TypeName = "numeric(12,2)")]
    public decimal DiscountAmount { get; set; }

    [Precision(12, 2)]
    [Column("totalamount", TypeName = "numeric(12,2)")]
    public decimal TotalAmount { get; set; }

    [Column("shippingaddress")]
    public string? ShippingAddress { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

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

    public ICollection<OnlineOrderItem> Items { get; set; } = new List<OnlineOrderItem>();
}

