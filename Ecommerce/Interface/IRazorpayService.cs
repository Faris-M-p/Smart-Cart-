using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.Models;

namespace Ecommerce.Interface
{
    public interface IRazorpayService
    {
        Task<RazorpayOrderResponse> CreateOrderAsync(decimal amountInRupees, string receiptId);
        bool VerifySignature(string razorpayOrderId, string razorpayPaymentId, string razorpaySignature);
        string GetKeyId();
    }
}
