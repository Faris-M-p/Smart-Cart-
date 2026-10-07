using Ecommerce.Helpers.UserAuth;
using Ecommerce.Interface;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.ReviewModel;

namespace Ecommerce.Controllers
{
    /// <summary>
    /// Customer ratings &amp; reviews API (existing <c>ratings</c> table), based on purchased ORDER ITEMS.
    /// The customer is always the signed-in user from the session token — a client-supplied
    /// customer id is never read, so one customer cannot act as another. Order / order item / product
    /// ids sent by the browser are re-validated against each other and against the customer in the database,
    /// and the 7-day review window (from delivery) is enforced there too.
    /// </summary>
    public class ReviewsController : Controller
    {
        private readonly ReviewInterface _reviewInterface;

        public ReviewsController(ReviewInterface reviewInterface)
        {
            _reviewInterface = reviewInterface;
        }

        /// <summary>Public, read-only: summary, star breakdown and one page of reviews for the Product Details page.</summary>
        [HttpGet]
        public async Task<IActionResult> Get(int productId, int pageIndex = 1, int pageSize = DefaultPageSize)
        {
            if (productId < 1)
            {
                return BadRequest(Fail("Product is missing."));
            }

            // Guests are allowed; the id only pins the viewer's own review to the top.
            var userId = UserAuthHelper.TryGetUserId(User, out var id) ? id : 0;

            var result = await _reviewInterface.GetProductReviewsAsync(productId, userId, pageIndex, pageSize);
            if (result == null)
            {
                return NotFound(Fail("This product is not available."));
            }

            return Ok(result);
        }

        /// <summary>
        /// Customer only: review status of every item of one of the customer's own orders
        /// (eligibility, review window, can add / edit / delete). Another customer's order is a 404.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Order(int orderId)
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            if (orderId < 1)
            {
                return BadRequest(Fail("Order is missing."));
            }

            var items = await _reviewInterface.GetOrderItemReviewsAsync(userId, orderId);
            if (items.Count == 0)
            {
                return NotFound(Fail("Order not found."));
            }

            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> Submit([FromBody] ReviewSubmitInput input)
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            if (input == null || !ModelState.IsValid)
            {
                return BadRequest(Fail(FirstModelError()));
            }

            var result = await _reviewInterface.SubmitReviewAsync(userId, input);
            return ToResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] ReviewUpdateInput input)
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            if (input == null || !ModelState.IsValid)
            {
                return BadRequest(Fail(FirstModelError()));
            }

            var result = await _reviewInterface.UpdateReviewAsync(userId, input.ReviewId, input.Rating, input.Review);
            return ToResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] ReviewDeleteInput input)
        {
            if (!TryGetUserId(out var userId, out var unauthorized))
            {
                return unauthorized;
            }

            if (input == null || !ModelState.IsValid)
            {
                return BadRequest(Fail(FirstModelError()));
            }

            var result = await _reviewInterface.DeleteReviewAsync(userId, input.ReviewId);
            return ToResult(result);
        }

        private IActionResult ToResult(CommonResponse result)
        {
            if (result.StatusCode)
            {
                return Ok(result);
            }

            return result.ResponseCode switch
            {
                -2 => Conflict(result),
                // -3 order not delivered, -5 not the customer's item/review, -6 review window over (locked)
                -3 or -5 or -6 => StatusCode(StatusCodes.Status403Forbidden, result),
                -4 => NotFound(result),
                _ => BadRequest(result)
            };
        }

        private string FirstModelError()
        {
            var message = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));

            return message ?? "Please check your review and try again.";
        }

        private static CommonResponse Fail(string message) => new()
        {
            ResponseCode = -1,
            StatusCode = false,
            ResponseMsg = message
        };

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
