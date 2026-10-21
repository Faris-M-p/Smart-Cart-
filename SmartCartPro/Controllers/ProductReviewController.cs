using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Exceptions;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>
/// Admin controller for product review moderation.
/// Customer-facing review creation stays on IProductReviewService for a future portal.
/// </summary>
[Authorize]
[ModuleAccess("PRODUCTS")]
public class ProductReviewController : Controller
{
    private readonly IProductReviewService _svc;
    private int ActorId => AuthClaims.GetUserId(User) ?? 0;

    public ProductReviewController(IProductReviewService svc) => _svc = svc;

    /// <summary>
    /// Moderation list of customer product reviews with status, rating, search and date filters.
    /// Catalog managers with products.manage open this from Products to approve or hide reviews.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> Index([FromQuery] InputProductReviewList input, CancellationToken ct)
        => View(await _svc.ListReviewsAsync(input, ct));

    /// <summary>
    /// JSON list of reviews for AJAX refresh of the moderation grid.
    /// Catalog managers with products.manage call this from the reviews screen.
    /// </summary>
    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> ListJson([FromQuery] InputProductReviewList input, CancellationToken ct)
    {
        var result = await _svc.ListReviewsAsync(input, ct);
        return CommonHelpers.SuccessResponse("Reviews loaded.", result);
    }

    /// <summary>
    /// Hides a review from the storefront by clearing approval.
    /// Catalog managers confirm hide from the reviews list.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> Hide(int id, CancellationToken ct)
    {
        var input = new InputProductReviewHide
        {
            IdProductReview = id,
            IsApproved = false,
            UpdatedBy = ActorId
        };
        await _svc.HideReviewAsync(input, ct);
        return CommonHelpers.SuccessResponse("Review hidden.", new { id, isApproved = false });
    }

    /// <summary>
    /// Approves a review so it is visible on the product.
    /// Catalog managers confirm approve from the reviews list.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        var input = new InputProductReviewHide
        {
            IdProductReview = id,
            IsApproved = true,
            UpdatedBy = ActorId
        };
        await _svc.HideReviewAsync(input, ct);
        return CommonHelpers.SuccessResponse("Review approved.", new { id, isApproved = true });
    }

    /// <summary>
    /// JSON hide or approve toggle for AJAX callers that post IsApproved.
    /// Catalog managers with products.manage call this from the reviews screen.
    /// </summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> HideJson([FromBody] InputProductReviewHide input, CancellationToken ct)
    {
        if (input is null)
            throw new ValidationException("Request body is required.", "Null body on HideJson.");
        input.UpdatedBy = ActorId;
        await _svc.HideReviewAsync(input, ct);
        return CommonHelpers.SuccessResponse(
            input.IsApproved ? "Review approved." : "Review hidden.",
            new { id = input.IdProductReview, isApproved = input.IsApproved });
    }
}
