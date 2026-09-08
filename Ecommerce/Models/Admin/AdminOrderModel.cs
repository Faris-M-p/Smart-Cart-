using System.ComponentModel.DataAnnotations;
using Ecommerce.CustomModelValidation;

namespace Ecommerce.Models.Admin
{
    public class AdminOrderModel
    {
        public class AdminOrderListInputVIEW
        {
            [Display(Name = "Search Text")]
            public string SearchText { get; set; } = string.Empty;

            [Display(Name = "Order Status")]
            public string OrderStatus { get; set; } = string.Empty;

            [Display(Name = "From Date")]
            public DateTime? FromDate { get; set; }

            [Display(Name = "To Date")]
            public DateTime? ToDate { get; set; }

            [Display(Name = "Page Index")]
            [GreaterThanZero]
            public int PageIndex { get; set; } = 1;

            [Display(Name = "Page Size")]
            [GreaterThanZero]
            [Range(1, 100, ErrorMessage = "{0} must be between 1 and 100.")]
            public int PageSize { get; set; } = 10;
        }

        public class AdminOrderIdInputVIEW
        {
            [Display(Name = "Order ID")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            public int OrderId { get; set; }
        }

        public class AdminOrderStatusInputVIEW
        {
            [Display(Name = "Order ID")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            public int OrderId { get; set; }

            [Display(Name = "Order Status")]
            [Required(ErrorMessage = "{0} is required.")]
            [RequiredNotEmpty]
            [System.ComponentModel.DataAnnotations.MaxLength(50, ErrorMessage = "{0} cannot exceed 50 characters.")]
            public string OrderStatus { get; set; } = string.Empty;
        }

        public class AdminOrderCancelInputVIEW
        {
            [Display(Name = "Order ID")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            public int OrderId { get; set; }

            [Display(Name = "Reason")]
            [System.ComponentModel.DataAnnotations.MaxLength(255, ErrorMessage = "{0} cannot exceed 255 characters.")]
            public string Reason { get; set; } = string.Empty;
        }

        public class AdminOrderListInput
        {
            public string SearchText { get; set; } = string.Empty;
            public string OrderStatus { get; set; } = string.Empty;
            public DateTime? FromDate { get; set; }
            public DateTime? ToDate { get; set; }
            public int PageIndex { get; set; } = 1;
            public int PageSize { get; set; } = 10;
        }

        public class AdminOrderListItem
        {
            public int OrderId { get; set; }
            public string OrderNumber { get; set; } = string.Empty;
            public DateTime OrderDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string OrderStatus { get; set; } = string.Empty;
            public string PaymentMethod { get; set; } = string.Empty;
            public string PaymentStatus { get; set; } = string.Empty;
            public string ReceiverName { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string CustomerName { get; set; } = string.Empty;
            public bool Cancelled { get; set; }
            public int ItemCount { get; set; }
            public string FirstProductName { get; set; } = string.Empty;
            public string FirstImageUrl { get; set; } = string.Empty;
            public bool CanConfirm { get; set; }
            public bool CanUpdateStatus { get; set; }
            public bool CanDeliver { get; set; }
            public bool CanCancel { get; set; }
        }

        public class AdminOrderHeader
        {
            public int OrderId { get; set; }
            public string OrderNumber { get; set; } = string.Empty;
            public DateTime OrderDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string OrderStatus { get; set; } = string.Empty;
            public string PaymentMethod { get; set; } = string.Empty;
            public string PaymentStatus { get; set; } = string.Empty;
            public string ShippingStatus { get; set; } = string.Empty;
            public string ReceiverName { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string AddressLine { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string Pincode { get; set; } = string.Empty;
            public string ShippingAddress { get; set; } = string.Empty;
            public string CustomerName { get; set; } = string.Empty;
            public string CustomerEmail { get; set; } = string.Empty;
            public bool Cancelled { get; set; }
            public DateTime? CancelledOn { get; set; }
            public string CancelledReason { get; set; } = string.Empty;
            public bool CanConfirm { get; set; }
            public bool CanUpdateStatus { get; set; }
            public bool CanDeliver { get; set; }
            public bool CanCancel { get; set; }
        }

        public class AdminOrderItem
        {
            public int OrderItemId { get; set; }
            public int ProductId { get; set; }
            public int ProductVariantId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Slug { get; set; } = string.Empty;
            public string Label { get; set; } = string.Empty;
            public string SKU { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public decimal MRP { get; set; }
            public int Quantity { get; set; }
            public decimal LineTotal { get; set; }
            public bool InStock { get; set; }
            public string ImageUrl { get; set; } = string.Empty;
        }

        public class AdminOrderDetail
        {
            public AdminOrderHeader Order { get; set; } = new();
            public List<AdminOrderItem> Items { get; set; } = new();
        }
    }
}
