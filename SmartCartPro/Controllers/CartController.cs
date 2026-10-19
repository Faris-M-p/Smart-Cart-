using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Exceptions;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>
/// Thin admin / storefront JSON endpoints for Cart and Wishlist.
/// No MVC views — all actions return JSON.
/// Authentication is caller-managed (e.g., cookie / JWT from the storefront).
/// No [ApiController] — uses standard MVC result helpers.
/// </summary>
public class CartController : Controller
{
    private readonly ICartService _cartSvc;
    private readonly IWishlistService _wishlistSvc;

    public CartController(ICartService cartSvc, IWishlistService wishlistSvc)
    {
        _cartSvc     = cartSvc;
        _wishlistSvc = wishlistSvc;
    }

    // ── CART ─────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetCart([FromQuery] InputCartSelect input, CancellationToken ct)
    {
        var result = await _cartSvc.GetCartAsync(input, ct);
        return result is null ? Json(new OutputCartSelect()) : Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> UpsertItem([FromBody] InputCartUpsertItem input, CancellationToken ct)
    {
        if (input is null)
            throw new ValidationException("Request body is required.", "Null body on UpsertItem.");
        var result = await _cartSvc.UpsertItemAsync(input, ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to update cart.", result.ResponseMsg ?? "Unable to update cart.");

        return CommonHelpers.SuccessResponse("Cart updated.", new { id = result.IdCart });
    }

    [HttpPost]
    public async Task<IActionResult> ClearCart([FromBody] InputCartClear input, CancellationToken ct)
    {
        if (input is null)
            throw new ValidationException("Request body is required.", "Null body on ClearCart.");
        var result = await _cartSvc.ClearCartAsync(input, ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to clear cart.", result.ResponseMsg ?? "Unable to clear cart.");

        return CommonHelpers.SuccessResponse("Cart cleared.");
    }

    // ── WISHLIST ─────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetWishlist([FromQuery] InputWishlistSelect input, CancellationToken ct)
    {
        var result = await _wishlistSvc.GetWishlistAsync(input, ct);
        return result is null ? Json(new OutputWishlistSelect()) : Json(result);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleWishlist([FromBody] InputWishlistToggle input, CancellationToken ct)
    {
        if (input is null)
            throw new ValidationException("Request body is required.", "Null body on ToggleWishlist.");
        var result = await _wishlistSvc.ToggleItemAsync(input, ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to update wishlist.", result.ResponseMsg ?? "Unable to update wishlist.");

        return CommonHelpers.SuccessResponse(
            result.IsWishlisted ? "Added to wishlist." : "Removed from wishlist.",
            new { isWishlisted = result.IsWishlisted });
    }
}
