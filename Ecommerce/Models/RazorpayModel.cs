namespace Ecommerce.Models
{
    public class RazorpayOrderResponse
    {
        public string OrderId { get; set; } = string.Empty;
        public long Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string KeyId { get; set; } = string.Empty;
    }

    public class RazorpayVerifyPaymentRequest
    {
        public string RazorpayPaymentId { get; set; } = string.Empty;
        public string RazorpayOrderId { get; set; } = string.Empty;
        public string RazorpaySignature { get; set; } = string.Empty;
        public int ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
    }
}
