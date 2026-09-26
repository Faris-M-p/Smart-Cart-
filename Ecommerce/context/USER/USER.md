# User (storefront)

## Purpose

Public shopping site: browse catalog, maintain a cart and wishlist, check out with COD, view and cancel own orders, register/login.

## Architecture

MVC controllers at the project root (`ShopController`, `CartController`, …), Razor under `views/`, layout `views/Shared/_Layout.cshtml`. JSON APIs are MVC actions, not a `/api` prefix (except admin auth).

Data access: **Dapper + stored procedures** only (`ShopRepository`, `CartRepository`, `WishlistRepository`, `UserAuthRepository`, `OrderRepository`).

## Authentication

- Pages: `/Account/Login`, `/Account/Register`
- Cookie JWT (`UserAuthHelper`); audience `JwtSettings.UserAudience`
- Register hashes password then `RegisterUser`
- Shop listing/detail **does not** require login
- Cart, wishlist mutate, checkout, order list/cancel **require** a customer `UserId` from JWT; otherwise 401

There is no customer role/permission matrix. `Users.IsAdmin` is unused by the admin panel.

## Authorization

None beyond “logged-in customer owns this cart/order” inside SPs (`UserId` parameters).

## Data flow

Admin catalog + stock → shop SPs filter `IsActive`, `SellOnline`, not cancelled → cart/place deduct SKU stock → `Orders` / `OrderItems` / `Payments` / `Shipping`.

## UI structure

Header/footer in `_Layout.cshtml` (SmartCart branding, Kochi copy). Pages: Home, Shop, product details, Cart, Wishlist, Checkout, confirmation, My Orders, order details, Account, plus static About/Blog/Contact.

## Current modules

See `modules/`: HOME, ACCOUNT, PRODUCTS, CATEGORIES, CART, WISHLIST, CHECKOUT, ORDERS.

## Business rules (cross-cutting)

- Online sell is product **and** SKU `SellOnline`
- Cart/checkout lines are **SKU** (`ProductVariantId`)
- Wishlist is **product**-level, not SKU
- COD only; UPI rejected in `PlaceOrder`
- Guest session columns exist; **current APIs do not use SessionKey**

## Conventions

- `CommonResponse` from write SPs
- Shop list uses `TableOutput<Product>` + `TableSettings` paging
- Do not call EF from storefront repositories
