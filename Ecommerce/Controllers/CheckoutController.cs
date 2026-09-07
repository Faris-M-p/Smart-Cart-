using Ecommerce.Helpers.UserAuth;
using Ecommerce.Interface;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.OrderModel;

namespace Ecommerce.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly OrderInterface _orderInterface;

        public CheckoutController(OrderInterface orderInterface)
        {
            _orderInterface = orderInterface;
        }

        public IActionResult Index()
        {
            ViewBag.Title = "Checkout";
            return View();
        }

        [HttpGet("Checkout/Confirmation/{orderId:int}")]
        public IActionResult Confirmation(int orderId)
        {
            ViewBag.Title = "Order placed";
            ViewBag.OrderId = orderId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Preview([FromQuery] int productVariantId = 0, [FromQuery] int quantity = 1)
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            return Ok(await _orderInterface.GetPreviewAsync(userId, productVariantId, quantity));
        }

        [HttpPost]
        public async Task<IActionResult> Place([FromBody] PlaceOrderInput input)
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            return Ok(await _orderInterface.PlaceOrderAsync(userId, input ?? new PlaceOrderInput()));
        }

        [HttpGet]
        public async Task<IActionResult> GetOrder(int orderId)
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            var page = await _orderInterface.GetOrderAsync(userId, orderId);
            if (page == null)
            {
                return NotFound();
            }

            return Ok(page);
        }

        private bool TryGetUserId(out int userId, out IActionResult unauthorized)
        {
            if (UserAuthHelper.TryGetUserId(User, out userId))
            {
                unauthorized = null!;
                return true;
            }

            unauthorized = Unauthorized(new CommonResponse
            {
                ResponseCode = -1,
                StatusCode = false,
                ResponseMsg = UserAuthHelper.UnauthorizedMessage
            });
            return false;
        }
    }
}
