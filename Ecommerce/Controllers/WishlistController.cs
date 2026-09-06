using Ecommerce.Helpers.UserAuth;
using Ecommerce.Interface;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.WishlistModel;

namespace Ecommerce.Controllers
{
    public class WishlistController : Controller
    {
        private readonly WishlistInterface _wishlistInterface;

        public WishlistController(WishlistInterface wishlistInterface)
        {
            _wishlistInterface = wishlistInterface;
        }

        public IActionResult Index()
        {
            ViewBag.Title = "Wishlist";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            return Ok(await _wishlistInterface.GetWishlistAsync(userId));
        }

        [HttpGet]
        public async Task<IActionResult> Status(int productId)
        {
            if (productId < 1 || !UserAuthHelper.TryGetUserId(User, out var userId))
            {
                return Ok(new WishlistStatus());
            }

            return Ok(await _wishlistInterface.GetStatusAsync(userId, productId));
        }

        [HttpPost]
        public async Task<IActionResult> Toggle([FromBody] WishlistToggleInput input)
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            if (input == null || input.ProductId < 1)
            {
                return BadRequest("Product is missing.");
            }

            return Ok(await _wishlistInterface.ToggleAsync(userId, input.ProductId));
        }

        [HttpPost]
        public async Task<IActionResult> Remove([FromBody] WishlistRemoveInput input)
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            if (input == null || input.WishlistItemId < 1)
            {
                return BadRequest("Wishlist item is missing.");
            }

            return Ok(await _wishlistInterface.RemoveItemAsync(userId, input.WishlistItemId));
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
