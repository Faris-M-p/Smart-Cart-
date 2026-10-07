using Ecommerce.Filters;
using Ecommerce.Helpers.AdminAuth;
using Ecommerce.Interface.Admin;
using Microsoft.AspNetCore.Mvc;
using static Ecommerce.Models.Admin.AdminReviewModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Controllers.Admin
{
    /// <summary>Admin view / moderation of customer ratings and reviews.</summary>
    [Route("Admin/Review")]
    public class ReviewController : Controller
    {
        private readonly IAdminReviewInterface _adminReviewInterface;

        public ReviewController(IAdminReviewInterface adminReviewInterface)
        {
            _adminReviewInterface = adminReviewInterface;
        }

        [Route("")]
        [Route("Index")]
        [RequirePermission("Ratings.View")]
        public IActionResult Index()
        {
            return View("~/Views/Admin/Review/Index.cshtml");
        }

        [HttpPost]
        [Route("GetReviewList")]
        [RequirePermission("Ratings.View")]
        public async Task<IActionResult> GetReviewList([FromBody] AdminReviewListInputVIEW viewInput)
        {
            if (viewInput == null || !ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new ApiResponse<TableOutput<AdminReviewListItem>>
                {
                    Success = false,
                    Message = "Validation failed",
                    Errors = errors
                });
            }

            var result = await _adminReviewInterface.GetReviewListAsync(new AdminReviewListInput
            {
                SearchText = viewInput.SearchText,
                Rating = viewInput.Rating,
                Status = viewInput.Status,
                PageIndex = viewInput.PageIndex,
                PageSize = viewInput.PageSize
            });

            return Ok(new ApiResponse<TableOutput<AdminReviewListItem>>
            {
                Success = true,
                Message = "Reviews loaded successfully",
                Data = result
            });
        }

        [HttpPost]
        [Route("Remove")]
        [RequirePermission("Ratings.Delete")]
        public async Task<IActionResult> Remove([FromBody] AdminReviewRemoveInputVIEW viewInput)
        {
            if (viewInput == null || !ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = "Validation failed.", errors });
            }

            // The moderator comes from the admin session token, never from the request body.
            if (!AdminAuthHelper.TryGetEmployeeId(User, out var employeeId))
            {
                return Unauthorized(new CommonResponse
                {
                    ResponseCode = -1,
                    StatusCode = false,
                    ResponseMsg = AdminAuthHelper.UnauthorizedMessage
                });
            }

            var result = await _adminReviewInterface.RemoveReviewAsync(viewInput.ReviewId, employeeId, viewInput.Reason);
            return Ok(result);
        }
    }
}
