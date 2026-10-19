# Cart & Wishlist — Phase 7

## Status
IMPLEMENTED (Phase 7) — repository + service + thin JSON CartController. No MVC views.

## Tables
- `carts` — cart header (fk_store, fk_customer nullable, sessionkey for guest)
- `cart_items` — line items (CASCADE delete from cart)
- `wishlists` — one per (fk_store, fk_customer), authenticated only
- `wishlist_items` — products in wishlist

## Cart Identity
- **Auth customer**: keyed by `(fk_store, fk_customer)`
- **Guest**: keyed by `(fk_store, sessionkey)`

## Wishlist Identity
- Always keyed by `(fk_store, fk_customer)` — no guest wishlist.

## Files
| Layer | File |
|---|---|
| Entities | `Entities/Cart.cs`, `CartItem.cs`, `Wishlist.cs`, `WishlistItem.cs` |
| Models | `Models/CartModels.cs` |
| Repository interfaces | `Repositories/Interfaces/ICartRepository.cs` (ICartRepository + IWishlistRepository) |
| Repository impls | `Repositories/CartRepository.cs` (CartRepository + WishlistRepository) |
| Service interfaces | `Services/Interfaces/ICartService.cs` (ICartService + IWishlistService) |
| Service impls | `Services/CartService.cs` (CartService + WishlistService) |
| Controller | `Controllers/CartController.cs` |

## JSON Endpoints
| Method | Route | Description |
|---|---|---|
| GET | `/Cart/GetCart` | Fetch cart (query: fk_store, fk_customer, sessionkey) |
| POST | `/Cart/UpsertItem` | Add/update/remove item (qty=0 removes) |
| POST | `/Cart/ClearCart` | Empty cart |
| GET | `/Cart/GetWishlist` | Fetch wishlist |
| POST | `/Cart/ToggleWishlist` | Add if absent, remove if present |
