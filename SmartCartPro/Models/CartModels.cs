using System.ComponentModel.DataAnnotations;

namespace SmartCartPro.Models;

// ═══════════════════════════════════════════════════════════════════
// CART
// Keyed by (fk_store, fk_customer) for logged-in customers
// or (fk_store, sessionkey) for guests.
// ═══════════════════════════════════════════════════════════════════

#region Cart Select

public sealed class InputCartSelect
{
    public int FkStore { get; set; }
    public int? FkCustomer { get; set; }
    public string? SessionKey { get; set; }
}

public sealed class OutputCartItem
{
    public int IdCartItem { get; set; }
    public int FkProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public int? FkProductVariant { get; set; }
    public string? VariantName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public sealed class OutputCartSelect
{
    public int IdCart { get; set; }
    public int FkStore { get; set; }
    public int? FkCustomer { get; set; }
    public string? SessionKey { get; set; }
    public List<OutputCartItem> Items { get; set; } = new();
    public decimal GrandTotal { get; set; }
}

#endregion

#region Cart Add / Update

public sealed class InputCartUpsertItem
{
    public int FkStore { get; set; }
    public int? FkCustomer { get; set; }
    public string? SessionKey { get; set; }
    public int FkProduct { get; set; }
    public int? FkProductVariant { get; set; }
    /// <summary>Absolute quantity to set (replaces existing line). Remove line if 0.</summary>
    public decimal Quantity { get; set; }
}

public sealed class OutputCartUpsertItem
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdCart { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Cart Clear

public sealed class InputCartClear
{
    public int FkStore { get; set; }
    public int? FkCustomer { get; set; }
    public string? SessionKey { get; set; }
}

public sealed class OutputCartClear
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

// ═══════════════════════════════════════════════════════════════════
// WISHLIST
// One wishlist per (fk_store, fk_customer). No guest wishlist.
// ═══════════════════════════════════════════════════════════════════

#region Wishlist Select

public sealed class InputWishlistSelect
{
    public int FkStore { get; set; }
    public int FkCustomer { get; set; }
}

public sealed class OutputWishlistItem
{
    public int IdWishlistItem { get; set; }
    public int FkProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public DateTime AddedAt { get; set; }
}

public sealed class OutputWishlistSelect
{
    public int IdWishlist { get; set; }
    public int FkStore { get; set; }
    public int FkCustomer { get; set; }
    public List<OutputWishlistItem> Items { get; set; } = new();
}

#endregion

#region Wishlist Toggle

public sealed class InputWishlistToggle
{
    public int FkStore { get; set; }
    public int FkCustomer { get; set; }
    public int FkProduct { get; set; }
}

public sealed class OutputWishlistToggle
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    /// <summary>True = product is now in wishlist; false = removed.</summary>
    public bool IsWishlisted { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion
