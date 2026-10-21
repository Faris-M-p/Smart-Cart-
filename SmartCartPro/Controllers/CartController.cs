using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCartPro.Exceptions;
using SmartCartPro.Helpers;
using SmartCartPro.Models;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Controllers;

/// <summary>
/// Thin JSON endpoints for Cart and Wishlist (storefront). No MVC views.
/// The store id always comes from the signed-in user's claims; a posted store id is ignored.
/// </summary>
[Authorize]
public class CartController : Controller
{
    private readonly ICartService _cartSvc;
    private readonly IWishlistService _wishlistSvc;
    private int StoreId => AuthClaims.GetFkStore(User) ?? 0;

    public CartController(ICartService cartSvc, IWishlistService wishlistSvc)
    {
        _cartSvc = cartSvc;
        _wishlistSvc = wishlistSvc;
    }

    private int RequireStore()
    {
        if (StoreId <= 0)
            throw new ForbiddenException("Sign in as a store user to use the cart.", "Cart called with fk_store=0.");
        return StoreId;
    }

    /// <summary>
    /// Returns the caller's cart lines and totals as JSON.
    /// Used by storefront checkout and cart widgets.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCart([FromQuery] InputCartSelect input, CancellationToken ct)
    {
        input.FkStore = RequireStore();
        var result = await _cartSvc.GetCartAsync(input, ct);
        return result is null ? Json(new OutputCartSelect()) : Json(result);
    }

    /// <summary>
    /// Adds or updates a line quantity in the cart.
    /// Used when shoppers change items before checkout.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpsertItem([FromBody] InputCartUpsertItem input, CancellationToken ct)
    {
        if (input is null)
            throw new ValidationException("Request body is required.", "Null body on UpsertItem.");
        input.FkStore = RequireStore();
        var result = await _cartSvc.UpsertItemAsync(input, ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to update cart.", result.ResponseMsg ?? "Unable to update cart.");

        return CommonHelpers.SuccessResponse("Cart updated.", new { id = result.IdCart });
    }

    /// <summary>
    /// Removes all lines from the caller's cart.
    /// Used after order placement or explicit empty cart.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClearCart([FromBody] InputCartClear input, CancellationToken ct)
    {
        if (input is null)
            throw new ValidationException("Request body is required.", "Null body on ClearCart.");
        input.FkStore = RequireStore();
        var result = await _cartSvc.ClearCartAsync(input, ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to clear cart.", result.ResponseMsg ?? "Unable to clear cart.");

        return CommonHelpers.SuccessResponse("Cart cleared.");
    }

    /// <summary>
    /// Returns wishlist items for the caller as JSON.
    /// Used on account and product pages in storefront.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetWishlist([FromQuery] InputWishlistSelect input, CancellationToken ct)
    {
        input.FkStore = RequireStore();
        var result = await _wishlistSvc.GetWishlistAsync(input, ct);
        return result is null ? Json(new OutputWishlistSelect()) : Json(result);
    }

    /// <summary>
    /// Adds or removes a product on the wishlist.
    /// Used when shoppers tap save or heart on a SKU.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleWishlist([FromBody] InputWishlistToggle input, CancellationToken ct)
    {
        if (input is null)
            throw new ValidationException("Request body is required.", "Null body on ToggleWishlist.");
        input.FkStore = RequireStore();
        var result = await _wishlistSvc.ToggleItemAsync(input, ct);
        if (!result.Ok)
            throw new UpdateFailedException("Unable to update wishlist.", result.ResponseMsg ?? "Unable to update wishlist.");

        return CommonHelpers.SuccessResponse(
            result.IsWishlisted ? "Added to wishlist." : "Removed from wishlist.",
            new { isWishlisted = result.IsWishlisted });
    }
}
