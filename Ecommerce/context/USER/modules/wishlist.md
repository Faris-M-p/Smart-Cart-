# User Module — Wishlist

## Module Overview
- **Area:** USER (Storefront)
- **Module Name:** Wishlist
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Allows logged-in customers to save products to a personal wishlist for future purchasing and move wishlist items into the shopping cart.

## Implemented Features
- **Toggle Wishlist:** Adds or removes a product from the customer's wishlist.
- **View Wishlist:** Displays all active products in the customer's wishlist with thumbnail, title, starting price, and stock status.
- **Remove Item:** Explicitly removes a product line from wishlist.
- **Check Status:** Queries whether a specific product is currently wishlisted by the logged-in customer.

## Filters / Search / Sorting
- N/A.

## Data
- DTOs: `WishlistItemView`, `WishlistView`, `CommonResponse`.

## Database Dependencies
- Tables: `WishList`, `WishlistItems`, `Products`, `ProductMedia`.
- Stored Procedures: `GetWishlist`, `GetWishlistStatus`, `ToggleWishlist`, `RemoveWishlistItem`.

## API & Data Access
- Controller: `Controllers/WishlistController.cs` (`Index`, `GetWishlist`, `Toggle`, `Remove`, `GetStatus`).
- Repository: `WishlistRepository.cs` via `IDataAccessDapper`.

## UI Rules & Conventions
- Rendered in `views/Wishlist/Index.cshtml`.
- Heart icon buttons on Shop listing and Product detail pages trigger AJAX toggle.

## Business Rules
- **Requires Customer Authentication:** `UserId` extracted from JWT cookie. Returns 401 if unauthenticated.
- Wishlist is maintained at the **Product level** (`ProductId`), not SKU level.
- Soft delete via `Cancelled = 1` on `WishlistItems`.

## Dependencies & Related Modules
- `USER/modules/account.md` — Customer login requirement.
- `USER/modules/shop.md` — Wishlist heart toggle trigger.
- `USER/modules/cart.md` — Action to move wishlisted item to cart.
