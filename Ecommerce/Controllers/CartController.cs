using Ecommerce.Helpers.UserAuth;
using Ecommerce.Interface;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Models.CartModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Controllers
{
    public class CartController : Controller
    {
        private readonly CartInterface _cartInterface;

        public CartController(CartInterface cartInterface)
        {
            _cartInterface = cartInterface;
        }

        public IActionResult Index()
        {
            ViewBag.Title = "Cart";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            return Ok(await _cartInterface.GetCartAsync(userId));
        }

        [HttpGet]
        public async Task<IActionResult> GetCounts()
        {
            if (!UserAuthHelper.TryGetUserId(User, out var userId))
            {
                return Ok(new BagCounts());
            }

            return Ok(await _cartInterface.GetCountsAsync(userId));
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CartItemInput input)
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            if (input == null || input.ProductVariantId < 1)
            {
                return BadRequest("Select a product option first.");
            }

            return Ok(await _cartInterface.AddItemAsync(userId, input));
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] CartItemUpdateInput input)
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            if (input == null || input.CartItemId < 1)
            {
                return BadRequest("Cart item is missing.");
            }

            return Ok(await _cartInterface.UpdateItemAsync(userId, input));
        }

        [HttpPost]
        public async Task<IActionResult> Remove([FromBody] CartItemRemoveInput input)
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            if (input == null || input.CartItemId < 1)
            {
                return BadRequest("Cart item is missing.");
            }

            return Ok(await _cartInterface.RemoveItemAsync(userId, input.CartItemId));
        }

        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            return Ok(await _cartInterface.ClearAsync(userId));
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
