# User Module — Home

## Module Overview
- **Area:** USER (Storefront)
- **Module Name:** Home
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Provides the public storefront home landing page. Features hero banner slider, highlighted categories grid, featured products showcase, and navigation links.

## Implemented Features
- Storefront landing page rendering (`HomeController.Index`).
- Dynamic hero banner slider with category filter links (`/Shop?categoryIds=...`).
- Dynamic "Shop by Categories" grid (`/Shop?category=...`).
- Dynamic "Featured Products" showcase (`/Shop/Details/{slug}`).
- Direct entry points to Shop, Cart, Wishlist, Account login/register.

## Filters / Search / Sorting
- N/A on Home page (navigates to Shop page for catalog search/filter).

## Data
- Models: `HomeViewModel`, `HomeBannerDto`, `HomeCategoryDto`, `HomeProductDto`.

## Database Dependencies
- Tables: `homepage_banners`, `homepage_banner_categories`, `homepage_categories`, `homepage_products`, `categories`, `products`, `product_media`, `product_variants`.
- Access: Dapper (`IDataAccessDapper`) via `HomeRepository.cs`.

## API & Data Access
- Controller: `Controllers/HomeController.cs` (`[HttpGet] Index`, `About`, `Contact`).
- Repository: `HomeRepository.cs` implementing `HomeInterface.cs`.

## UI Rules & Conventions
- Uses `views/Shared/_Layout.cshtml` and `views/Home/Index.cshtml`.

## Business Rules
- Does not require customer authentication.
- Accessible to all public site visitors.

## Dependencies & Related Modules
- `USER/modules/homepage.md` — Storefront homepage dynamic rendering specifications.
- `USER/modules/shop.md` — Catalog browsing.
- `USER/modules/account.md` — Login/Register links.
