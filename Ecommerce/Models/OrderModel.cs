using static Ecommerce.Models.CartModel;
using static Ecommerce.Models.UserAddressModel;

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
            public string AddressType { get; set; } = "Home";
            public bool SaveAddress { get; set; } = false;
            public int AddressId { get; set; } = 0;
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public string? RazorpayOrderId { get; set; }
            public string? RazorpayPaymentId { get; set; }
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
            public List<UserAddress> Addresses { get; set; } = new();
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

        /// <summary>A purchased line of an order: the cart line plus its order item id (used for reviews).</summary>
        public class OrderLine : CartLine
        {
            public int OrderItemId { get; set; }
        }

        public class OrderPage
        {
            public OrderHeader Order { get; set; } = new();
            public List<OrderLine> Items { get; set; } = new();
        }
    }
}
