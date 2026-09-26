# User Module — Home

## Module Overview
- **Area:** USER (Storefront)
- **Module Name:** Home
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Provides the public storefront home landing page. Features hero banner, highlighted categories, featured products, and navigation links.

## Implemented Features
- Storefront landing page rendering (`HomeController.Index`).
- Static hero section with branding ("SmartCart Supermarket").
- Category shortcuts and promotional banners.
- Direct entry points to Shop, Cart, Wishlist, Account login/register.

## Filters / Search / Sorting
- N/A on Home page (navigates to Shop page for catalog search/filter).

## Data
- Models: Storefront layout DTOs.

## Database Dependencies
- Tables: `Categories`, `Products`.
- Stored Procedures: None called directly by `HomeController`; navigation links pass route parameters to `ShopController`.

## API & Data Access
- Controller: `Controllers/HomeController.cs` (`[HttpGet] Index`, `About`, `Contact`).
- Repository: `HomeRepository.cs` (stub, not active).

## UI Rules & Conventions
- Uses `views/Shared/_Layout.cshtml`.
- Custom CSS styles loaded from `wwwroot/css/storefront.css`.

## Business Rules
- Does not require customer authentication.
- Accessible to all public site visitors.

## Dependencies & Related Modules
- `USER/modules/shop.md` — Catalog browsing.
- `USER/modules/account.md` — Login/Register links.
