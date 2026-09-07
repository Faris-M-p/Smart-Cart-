using Ecommerce.Helpers.UserAuth;
using Ecommerce.Interface;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.OrderModel;

namespace Ecommerce.Controllers
{
    public class OrdersController : Controller
    {
        private readonly OrderInterface _orderInterface;

        public OrdersController(OrderInterface orderInterface)
        {
            _orderInterface = orderInterface;
        }

        public IActionResult Index()
        {
            ViewBag.Title = "My Orders";
            return View();
        }

        [HttpGet("Orders/Details/{orderId:int}")]
        public IActionResult Details(int orderId)
        {
            ViewBag.Title = "Order details";
            ViewBag.OrderId = orderId;
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            return Ok(await _orderInterface.GetOrdersAsync(userId));
        }

        [HttpGet]
        public async Task<IActionResult> Get(int orderId)
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

        [HttpPost]
        public async Task<IActionResult> Cancel([FromBody] CancelOrderInput input)
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            if (input == null || input.OrderId < 1)
            {
                return BadRequest(new CommonResponse
                {
                    ResponseCode = -1,
                    StatusCode = false,
                    ResponseMsg = "Order is missing."
                });
            }

            return Ok(await _orderInterface.CancelOrderAsync(userId, input));
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
