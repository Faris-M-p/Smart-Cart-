using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

/// <summary>
/// Cart repository — returns/accepts only Input/Output models. Entities stay internal.
/// Keyed by (fk_store, fk_customer) for auth customers or (fk_store, sessionkey) for guests.
/// </summary>
public class CartRepository : ICartRepository
{
    private readonly SmartCartDbContext _db;

    public CartRepository(SmartCartDbContext db) => _db = db;

    private async Task<Cart?> FindCartAsync(InputCartSelect key, CancellationToken ct)
    {
        if (key.FkCustomer.HasValue)
            return await _db.Carts
                .Include(c => c.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.FkStore == key.FkStore && c.FkCustomer == key.FkCustomer, ct);

        if (!string.IsNullOrWhiteSpace(key.SessionKey))
            return await _db.Carts
                .Include(c => c.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.FkStore == key.FkStore && c.SessionKey == key.SessionKey, ct);

        return null;
    }

    public async Task<OutputCartSelect?> GetCartAsync(InputCartSelect input, CancellationToken ct = default)
    {
        var cart = await FindCartAsync(input, ct);
        if (cart is null) return null;

        var items = cart.Items.Select(i => new OutputCartItem
        {
            IdCartItem       = i.IdCartItem,
            FkProduct        = i.FkProduct,
            ProductName      = i.Product?.ProductName ?? string.Empty,
            ProductCode      = i.Product?.ProductCode,
            FkProductVariant = i.FkProductVariant,
            Quantity         = i.Quantity,
            UnitPrice        = i.Product?.SellingPrice ?? 0,
            LineTotal        = i.Quantity * (i.Product?.SellingPrice ?? 0)
        }).ToList();

        return new OutputCartSelect
        {
            IdCart      = cart.IdCart,
            FkStore     = cart.FkStore,
            FkCustomer  = cart.FkCustomer,
            SessionKey  = cart.SessionKey,
            Items       = items,
            GrandTotal  = items.Sum(x => x.LineTotal)
        };
    }

    public async Task<OutputCartUpsertItem> UpsertItemAsync(InputCartUpsertItem input, CancellationToken ct = default)
    {
        // Find or create cart
        Cart? cart;
        if (input.FkCustomer.HasValue)
            cart = await _db.Carts.Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.FkStore == input.FkStore && c.FkCustomer == input.FkCustomer, ct);
        else
            cart = await _db.Carts.Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.FkStore == input.FkStore && c.SessionKey == input.SessionKey, ct);

        if (cart is null)
        {
            cart = new Cart
            {
                FkStore    = input.FkStore,
                FkCustomer = input.FkCustomer,
                SessionKey = input.SessionKey,
                CreatedAt  = DateTime.UtcNow
            };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync(ct); // flush to get IdCart
        }

        var existingItem = cart.Items.FirstOrDefault(
            i => i.FkProduct == input.FkProduct && i.FkProductVariant == input.FkProductVariant);

        if (input.Quantity <= 0)
        {
            // Remove line
            if (existingItem is not null)
            {
                _db.CartItems.Remove(existingItem);
                cart.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync(ct);
            }
        }
        else if (existingItem is not null)
        {
            existingItem.Quantity  = input.Quantity;
            existingItem.UpdatedAt = DateTime.UtcNow;
            cart.UpdatedAt         = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }
        else
        {
            _db.CartItems.Add(new CartItem
            {
                FkCart           = cart.IdCart,
                FkStore          = input.FkStore,
                FkProduct        = input.FkProduct,
                FkProductVariant = input.FkProductVariant,
                Quantity         = input.Quantity,
                CreatedAt        = DateTime.UtcNow
            });
            cart.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);
        }

        return new OutputCartUpsertItem
        {
            ResponseCode = 0,
            ResponseMsg  = "Cart updated.",
            StatusCode   = 200,
            IdCart       = cart.IdCart
        };
    }

    public async Task<OutputCartClear> ClearCartAsync(InputCartClear input, CancellationToken ct = default)
    {
        var key = new InputCartSelect
        {
            FkStore    = input.FkStore,
            FkCustomer = input.FkCustomer,
            SessionKey = input.SessionKey
        };
        var cart = await FindCartAsync(key, ct);
        if (cart is null)
            return new OutputCartClear { ResponseCode = 0, ResponseMsg = "Cart already empty.", StatusCode = 200 };

        _db.CartItems.RemoveRange(cart.Items);
        cart.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new OutputCartClear { ResponseCode = 0, ResponseMsg = "Cart cleared.", StatusCode = 200 };
    }
}

/// <summary>
/// Wishlist repository — one wishlist per (fk_store, fk_customer).
/// Toggle adds if absent, removes if present.
/// </summary>
public class WishlistRepository : IWishlistRepository
{
    private readonly SmartCartDbContext _db;

    public WishlistRepository(SmartCartDbContext db) => _db = db;

    public async Task<OutputWishlistSelect?> GetWishlistAsync(InputWishlistSelect input, CancellationToken ct = default)
    {
        var wl = await _db.Wishlists
            .Include(w => w.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(w => w.FkStore == input.FkStore && w.FkCustomer == input.FkCustomer, ct);

        if (wl is null) return null;

        return new OutputWishlistSelect
        {
            IdWishlist  = wl.IdWishlist,
            FkStore     = wl.FkStore,
            FkCustomer  = wl.FkCustomer,
            Items = wl.Items.Select(i => new OutputWishlistItem
            {
                IdWishlistItem = i.IdWishlistItem,
                FkProduct      = i.FkProduct,
                ProductName    = i.Product?.ProductName ?? string.Empty,
                ProductCode    = i.Product?.ProductCode,
                AddedAt        = i.CreatedAt
            }).ToList()
        };
    }

    public async Task<OutputWishlistToggle> ToggleItemAsync(InputWishlistToggle input, CancellationToken ct = default)
    {
        var wl = await _db.Wishlists
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w => w.FkStore == input.FkStore && w.FkCustomer == input.FkCustomer, ct);

        if (wl is null)
        {
            wl = new Wishlist
            {
                FkStore    = input.FkStore,
                FkCustomer = input.FkCustomer,
                CreatedAt  = DateTime.UtcNow
            };
            _db.Wishlists.Add(wl);
            await _db.SaveChangesAsync(ct); // flush to get IdWishlist
        }

        var existingItem = wl.Items.FirstOrDefault(i => i.FkProduct == input.FkProduct);

        bool isWishlisted;
        if (existingItem is not null)
        {
            _db.WishlistItems.Remove(existingItem);
            isWishlisted = false;
        }
        else
        {
            _db.WishlistItems.Add(new WishlistItem
            {
                FkWishlist = wl.IdWishlist,
                FkStore    = input.FkStore,
                FkProduct  = input.FkProduct,
                CreatedAt  = DateTime.UtcNow
            });
            isWishlisted = true;
        }

        wl.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new OutputWishlistToggle
        {
            ResponseCode = 0,
            ResponseMsg  = isWishlisted ? "Added to wishlist." : "Removed from wishlist.",
            StatusCode   = 200,
            IsWishlisted = isWishlisted
        };
    }
}
