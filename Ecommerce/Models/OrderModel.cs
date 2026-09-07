using static Ecommerce.Models.CartModel;

namespace Ecommerce.Models
{
    public class OrderModel
    {
        public class CheckoutPreviewInput
        {
            public int ProductVariantId { get; set; }
            public int Quantity { get; set; } = 1;
        }

        public class PlaceOrderInput
        {
            public int ProductVariantId { get; set; }
            public int Quantity { get; set; } = 1;
            public string ReceiverName { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string AddressLine { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string Pincode { get; set; } = string.Empty;
            public string PaymentMethod { get; set; } = "COD";
        }

        public class CheckoutCustomer
        {
            public string FullName { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
        }

        public class CheckoutSummary : CartSummary
        {
            public bool CanPlace { get; set; }
        }

        public class CheckoutPage
        {
            public string Source { get; set; } = "cart";
            public List<CartLine> Items { get; set; } = new();
            public CheckoutSummary Summary { get; set; } = new();
            public CheckoutCustomer Customer { get; set; } = new();
        }

        public class OrderHeader
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
            public string AddressLine { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string Pincode { get; set; } = string.Empty;
            public string ShippingAddress { get; set; } = string.Empty;
            public bool Cancelled { get; set; }
            public DateTime? CancelledOn { get; set; }
            public string CancelledReason { get; set; } = string.Empty;
            public bool CanCancel { get; set; }
        }

        public class OrderListItem
        {
            public int OrderId { get; set; }
            public string OrderNumber { get; set; } = string.Empty;
            public DateTime OrderDate { get; set; }
            public decimal TotalAmount { get; set; }
            public string OrderStatus { get; set; } = string.Empty;
            public string PaymentMethod { get; set; } = string.Empty;
            public string PaymentStatus { get; set; } = string.Empty;
            public bool Cancelled { get; set; }
            public bool CanCancel { get; set; }
            public int ItemCount { get; set; }
            public string FirstProductName { get; set; } = string.Empty;
            public string FirstImageUrl { get; set; } = string.Empty;
        }

        public class CancelOrderInput
        {
            public int OrderId { get; set; }
            public string Reason { get; set; } = string.Empty;
        }

        public class OrderPage
        {
            public OrderHeader Order { get; set; } = new();
            public List<CartLine> Items { get; set; } = new();
        }
    }
}
