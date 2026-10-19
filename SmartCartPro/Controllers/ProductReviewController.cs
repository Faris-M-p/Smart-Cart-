using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Filters;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>
/// Admin controller for product review moderation.
/// Customer-facing review creation is available as service methods (IProductReviewService)
/// callable from a future customer portal without an MVC view.
/// </summary>
[Authorize]
[ModuleAccess("PRODUCTS")]
public class ProductReviewController : Controller
{
    private readonly IProductReviewService _svc;
    private int ActorId => AuthClaims.GetUserId(User) ?? 0;

    public ProductReviewController(IProductReviewService svc) => _svc = svc;

    // ── ADMIN LIST ───────────────────────────────────────────────

    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> Index([FromQuery] InputProductReviewList input, CancellationToken ct)
    {
        input.Page     = Math.Max(1, input.Page);
        input.PageSize = input.PageSize is <= 0 or > 200 ? 50 : input.PageSize;
        var result = await _svc.ListReviewsAsync(input, ct);
        ViewBag.Filter = input;
        return View(result);
    }

    /// <summary>JSON list for AJAX.</summary>
    [HttpGet]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> ListJson([FromQuery] InputProductReviewList input, CancellationToken ct)
    {
        input.Page     = Math.Max(1, input.Page);
        input.PageSize = input.PageSize is <= 0 or > 200 ? 50 : input.PageSize;
        return Json(await _svc.ListReviewsAsync(input, ct));
    }

    // ── ADMIN HIDE / SHOW ────────────────────────────────────────

    /// <summary>Hide a review (set isapproved = false).</summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> Hide(int id, CancellationToken ct)
    {
        var result = await _svc.HideReviewAsync(new InputProductReviewHide
        {
            IdProductReview = id,
            IsApproved      = false,
            UpdatedBy       = ActorId
        }, ct);

        TempData[result.Ok ? "SuccessMessage" : "ErrorMessage"] = result.ResponseMsg;
        return RedirectToAction(nameof(Index));
    }

    /// <summary>Approve (make visible) a review.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> Approve(int id, CancellationToken ct)
    {
        var result = await _svc.HideReviewAsync(new InputProductReviewHide
        {
            IdProductReview = id,
            IsApproved      = true,
            UpdatedBy       = ActorId
        }, ct);

        TempData[result.Ok ? "SuccessMessage" : "ErrorMessage"] = result.ResponseMsg;
        return RedirectToAction(nameof(Index));
    }

    /// <summary>JSON hide/show toggle for AJAX.</summary>
    [HttpPost]
    [RequirePermission("products.manage")]
    public async Task<IActionResult> HideJson([FromBody] InputProductReviewHide input, CancellationToken ct)
    {
        if (input is null) return BadRequest();
        input.UpdatedBy = ActorId;
        var result = await _svc.HideReviewAsync(input, ct);
        if (!result.Ok) return StatusCode(result.StatusCode, new { result.ResponseMsg });
        return Ok(result);
    }
}
