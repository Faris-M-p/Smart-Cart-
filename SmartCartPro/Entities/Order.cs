namespace SmartCartPro.Entities;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "Placed";
    public DateTimeOffset CreatedAt { get; set; }
}
