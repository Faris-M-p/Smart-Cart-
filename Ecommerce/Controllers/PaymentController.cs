using System;
using System.Threading.Tasks;
using Ecommerce.Helpers.UserAuth;
using Ecommerce.Interface;
using Ecommerce.Models;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.OrderModel;

namespace Ecommerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IRazorpayService _razorpayService;
        private readonly OrderInterface _orderInterface;

        public PaymentController(
            IRazorpayService razorpayService,
            OrderInterface orderInterface)
        {
            _razorpayService = razorpayService;
            _orderInterface = orderInterface;
        }

        [HttpPost("create-order")]
        public async Task<IActionResult> CreateOrder([FromBody] PlaceOrderInput input)
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            try
            {
                var preview = await _orderInterface.GetPreviewAsync(userId, input.ProductVariantId, input.Quantity);
                
                if (preview == null || preview.Summary == null || preview.Summary.Subtotal <= 0)
                {
                    return BadRequest(new { message = "Invalid cart amount." });
                }

                if (preview.Summary.Subtotal < 1)
                {
                    return BadRequest(new { message = "Minimum amount must be at least 1 INR." });
                }

                string receiptId = $"SC-RECEIPT-{Guid.NewGuid().ToString().Substring(0, 8)}";
                
                var order = await _razorpayService.CreateOrderAsync(preview.Summary.Subtotal, receiptId);
                
                return Ok(order);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(401, new { message = "Payment gateway not configured properly." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to create payment order." });
            }
        }

        [HttpPost("verify-payment")]
        public async Task<IActionResult> VerifyPayment([FromBody] RazorpayVerifyPaymentRequest request)
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            if (string.IsNullOrEmpty(request.RazorpayPaymentId) || 
                string.IsNullOrEmpty(request.RazorpayOrderId) || 
                string.IsNullOrEmpty(request.RazorpaySignature))
            {
                return BadRequest(new { message = "Missing payment verification fields." });
            }

            bool isValid = _razorpayService.VerifySignature(
                request.RazorpayOrderId, 
                request.RazorpayPaymentId, 
                request.RazorpaySignature);

            if (!isValid)
            {
                return BadRequest(new { message = "Payment signature verification failed." });
            }

            try
            {
                var placeOrderInput = new PlaceOrderInput
                {
                    ProductVariantId = request.ProductVariantId,
                    Quantity = request.Quantity,
                    ReceiverName = request.ReceiverName,
                    Phone = request.Phone,
                    AddressLine = request.AddressLine,
                    City = request.City,
                    Pincode = request.Pincode,
                    PaymentMethod = "Razorpay",
                    RazorpayOrderId = request.RazorpayOrderId,
                    RazorpayPaymentId = request.RazorpayPaymentId
                };

                var result = await _orderInterface.PlaceOrderAsync(userId, placeOrderInput);

                if (result != null && result.ResponseCode > 0)
                {
                    return Ok(new { success = true, orderId = result.ResponseCode, message = result.ResponseMsg });
                }

                return BadRequest(new
                {
                    success = false,
                    message = result?.ResponseMsg ?? "Could not place the order after payment."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "An error occurred while finalizing the order." });
            }
        }

        private bool TryGetUserId(out int userId, out IActionResult unauthorized)
        {
            if (UserAuthHelper.TryGetUserId(User, out userId))
            {
                unauthorized = null!;
                return true;
            }

            unauthorized = Unauthorized(new { message = UserAuthHelper.UnauthorizedMessage });
            return false;
        }
    }
}
