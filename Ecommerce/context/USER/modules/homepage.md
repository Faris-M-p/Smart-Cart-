# User Module — Homepage Management & Storefront Landing

## Module Overview
- **Area:** USER (Storefront)
- **Module Name:** Homepage
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Renders dynamic storefront home page content configured by admins, including hero banner slider, category shortcuts grid, and featured products showcase.

## Implemented Features
- **Dynamic Hero Banner Slider:**
  - Fetches active banners ordered by display order.
  - Displays banner image, title, subtitle.
  - Clicking banner navigates to Shop listing page with attached category filters applied (`/Shop?categoryIds=1,2,3`) or custom target URL.
- **Shop by Categories Grid:**
  - Displays active featured categories configured in Admin (one category per entry).
  - Clicking category navigates to Shop page (`/Shop?category={categoryId}`).
- **Featured Products Showcase:**
  - Displays active featured products configured in Admin.
  - Reuses catalog products with image, name, category, starting price.
  - Clicking product navigates to Product Details page (`/Shop/Details/{slug}`).

## Database Dependencies
- Tables: `homepage_banners`, `homepage_banner_categories`, `homepage_categories`, `homepage_products`, `categories`, `products`, `product_media`, `product_variants`.
- Access: Dapper (`IDataAccessDapper`) via `HomeRepository.cs`.

## API & Data Access
- Controller: `Controllers/HomeController.cs`.
- Interface & Repository: `HomeInterface.cs` / `HomeRepository.cs`.
- ViewModel: `HomeViewModel`, `HomeBannerDto`, `HomeCategoryDto`, `HomeProductDto`.
