# Admin Module — Homepage Management

## Module Overview
- **Area:** ADMIN (Back Office)
- **Module Name:** Homepage Management
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Manages storefront home page dynamic content including hero banner slides, attached category filters for banners, featured categories grid, and featured products showcase.

## Implemented Features
- **Banners Management:**
  - Create, update, soft delete banner slides.
  - Image upload via `CommonImageService` (`wwwroot/uploads/banners/{bannerId}/`).
  - Attach multiple categories to a banner slide. Clicking banner navigates to Storefront Shop with applied category filters (`/Shop?categoryIds=1,2,3`).
  - Optional custom target URL override.
  - Interactive display order updating and active status toggle.
- **Featured Categories Management:**
  - Select active catalog categories to feature on storefront homepage (strictly one category per item).
  - Clicking category navigates to Storefront Shop page (`/Shop?category={categoryId}`).
  - Custom display ordering and active status toggle.
- **Featured Products Management:**
  - Select active products to feature on storefront homepage (reuses existing catalog products without duplicating records).
  - Displays product image, title, category, starting price.
  - Clicking product navigates to Product Details page (`/Shop/Details/{slug}`).
  - Custom display ordering and active status toggle.

## Database Dependencies
- Tables: `homepage_banners`, `homepage_banner_categories`, `homepage_categories`, `homepage_products`.
- Access: EF Core (`EcommerceDbContext`) via `HomepageRepository.cs`.

## API & Data Access
- Controller: `Controllers/Admin/HomepageController.cs` (`[Route("Admin/Homepage")]`).
- Interface & Repository: `IHomepageInterface.cs` / `HomepageRepository.cs`.
- Model: `HomepageModel.cs` & `HomepageEntities.cs`.
- Permissions: `Homepage.View`, `Homepage.Edit`.

## UI Rules & Conventions
- Mantis Admin tabbed layout: `Views/Admin/Homepage/Index.cshtml` (3 tabs: Banners, Featured Categories, Featured Products).
- Client script: `wwwroot/Admin/assets/js/pages/homepage-management.js`.
- Modals for creating/editing banners, featured categories, and featured products.
