# User Module — Cart

## Module Overview
- **Area:** USER (Storefront)
- **Module Name:** Cart
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Manages the customer's active shopping cart, item addition, quantity updates, item removal, and stock verification.

## Implemented Features
- **Add to Cart:** Adds a specific SKU (`ProductVariantId`) to customer cart. Checks stock availability (`Stock.QuantityAvailable`).
- **Update Quantity:** Increases or decreases SKU quantity in cart.
- **Remove Item:** Removes specific SKU line item from cart.
- **Clear Cart:** Empty all items from customer cart.
- **Cart Summary:** Calculates total quantity, subtotal, shipping fee, and grand total.

## Filters / Search / Sorting
- N/A.

## Data
- DTOs: `CartItemView`, `CartView`, `CartCountView`, `CommonResponse`.

## Database Dependencies
- Tables: `Cart`, `CartItems`, `ProductVariants`, `Products`, `Stock`, `SkuMedia`.
- Stored Procedures: `GetCart`, `GetBagCounts`, `AddCartItem`, `UpdateCartItem`, `RemoveCartItem`, `ClearCart`.

## API & Data Access
- Controller: `Controllers/CartController.cs` (`Index`, `GetCart`, `Add`, `Update`, `Remove`, `Clear`, `GetCounts`).
- Repository: `CartRepository.cs` via `IDataAccessDapper`.

## UI Rules & Conventions
- Rendered in `views/Cart/Index.cshtml`.
- Dynamic AJAX updates for line totals, cart counts, and error toasts.

## Business Rules
- **Requires Customer Authentication:** `UserId` extracted from JWT cookie. Returns 401 if unauthenticated.
- Cart lines operate strictly on **SKU level** (`ProductVariantId`), not product header level.
- `SessionKey` column exists in schema for future guest support, but active APIs strictly enforce logged-in `UserId`.
- Quantity requested cannot exceed available stock (`QuantityAvailable`) or SKU `MaxOrderQty`.

## Dependencies & Related Modules
- `USER/modules/account.md` — Customer login requirement.
- `USER/modules/shop.md` — Source of SKU selection.
- `USER/modules/checkout.md` — Passes cart contents to order placement.
