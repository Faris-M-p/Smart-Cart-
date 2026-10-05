using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Interface;
using Ecommerce.Models;
using Microsoft.Extensions.Configuration;
using Razorpay.Api;

namespace Ecommerce.Repository
{
    public class RazorpayService : IRazorpayService
    {
        private readonly string _keyId;
        private readonly string _keySecret;

        public RazorpayService(IConfiguration configuration)
        {
            _keyId = configuration["Razorpay:KeyId"] 
                ?? configuration["RAZORPAY_KEY_ID"] 
                ?? string.Empty;
            _keySecret = configuration["Razorpay:KeySecret"] 
                ?? configuration["RAZORPAY_KEY_SECRET"] 
                ?? string.Empty;
        }

        public string GetKeyId() => _keyId;

        public Task<RazorpayOrderResponse> CreateOrderAsync(decimal amountInRupees, string receiptId)
        {
            if (string.IsNullOrEmpty(_keyId) || string.IsNullOrEmpty(_keySecret))
            {
                throw new InvalidOperationException("Razorpay credentials are not configured.");
            }

            // Amount in paise
            long amountInPaise = (long)(amountInRupees * 100);

            if (amountInPaise < 100)
            {
                throw new ArgumentException("Amount must be at least 1 INR.");
            }

            var client = new RazorpayClient(_keyId, _keySecret);

            var options = new Dictionary<string, object>
            {
                { "amount", amountInPaise },
                { "currency", "INR" },
                { "receipt", receiptId }
            };

            var order = client.Order.Create(options);

            return Task.FromResult(new RazorpayOrderResponse
            {
                OrderId = order["id"].ToString(),
                Amount = amountInPaise,
                Currency = "INR",
                KeyId = _keyId
            });
        }

        public bool VerifySignature(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature)
        {
            if (string.IsNullOrEmpty(_keyId) || string.IsNullOrEmpty(_keySecret))
            {
                return false;
            }

            try
            {
                var attributes = new Dictionary<string, string>
                {
                    { "razorpay_order_id", razorpayOrderId },
                    { "razorpay_payment_id", razorpayPaymentId },
                    { "razorpay_signature", razorpaySignature }
                };

                Utils.verifyPaymentSignature(attributes);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
