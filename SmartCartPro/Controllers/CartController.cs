using Microsoft.AspNetCore.Mvc;
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
        if (input is null) return BadRequest("Request body required.");
        var result = await _cartSvc.UpsertItemAsync(input, ct);
        if (!result.Ok) return StatusCode(result.StatusCode, new { result.ResponseMsg });
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> ClearCart([FromBody] InputCartClear input, CancellationToken ct)
    {
        if (input is null) return BadRequest("Request body required.");
        var result = await _cartSvc.ClearCartAsync(input, ct);
        return Ok(result);
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
        if (input is null) return BadRequest("Request body required.");
        var result = await _wishlistSvc.ToggleItemAsync(input, ct);
        if (!result.Ok) return StatusCode(result.StatusCode, new { result.ResponseMsg });
        return Ok(result);
    }
}
