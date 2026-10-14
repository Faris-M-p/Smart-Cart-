namespace SmartCartPro.Entities;

/// <summary>
/// Sample storefront order entity mapped to <c>online_orders</c> (store-scoped via <c>fk_store</c>).
/// </summary>
public class Order
{
    public int IdOnlineOrder { get; set; }

    /// <summary>SQL column <c>fk_store</c>.</summary>
    public int FkStore { get; set; }

    /// <summary>SQL column <c>fk_customer</c>.</summary>
    public int FkCustomer { get; set; }

    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = "pending";
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? ShippingAddress { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool Cancelled { get; set; }
    public DateTime? CancelledOn { get; set; }
    public string? CancelledReason { get; set; }

    public Store? Store { get; set; }
}
