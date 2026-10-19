using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.Models;

// ═══════════════════════════════════════════════════════════════════
// ONLINE ORDER — STATUS FLOW
//   pending → confirmed → packed → shipped → delivered
//                                          → cancelled  (from any non-terminal)
// ═══════════════════════════════════════════════════════════════════

#region Online Order List

public sealed class InputOnlineOrderList
{
    public int? FkStore { get; set; }
    public string? Status { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed class OutputOnlineOrderListItem
{
    public int IdOnlineOrder { get; set; }
    public int FkStore { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int FkCustomer { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public bool Cancelled { get; set; }
}

public sealed class OutputOnlineOrderList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputOnlineOrderListItem> Items { get; set; } = new();
}

#endregion

#region Online Order Select

public sealed class InputOnlineOrderSelect
{
    public int IdOnlineOrder { get; set; }
}

public sealed class OutputOnlineOrderItem
{
    public int IdOnlineOrderItem { get; set; }
    public int FkProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public int? FkProductVariant { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class OutputOnlineOrderSelect
{
    public int IdOnlineOrder { get; set; }
    public int FkStore { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public int FkCustomer { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? ShippingAddress { get; set; }
    public string? Notes { get; set; }
    public bool Cancelled { get; set; }
    public string? CancelledReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OutputOnlineOrderItem> Items { get; set; } = new();
}

#endregion

#region Online Order Status Update

public sealed class InOnlineOrderStatusView
{
    [Required]
    public int IdOnlineOrder { get; set; }

    [Required]
    [Display(Name = "New Status")]
    public string NewStatus { get; set; } = string.Empty;

    [MaxLength(500)]
    [Display(Name = "Reason / Notes")]
    public string? Reason { get; set; }
}

public sealed class InputOnlineOrderStatusUpdate
{
    public int IdOnlineOrder { get; set; }
    public string NewStatus { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public int UpdatedBy { get; set; }
}

public sealed class OutputOnlineOrderStatusUpdate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string? NewStatus { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion
