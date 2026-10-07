# User Module — Shop

## Module Overview
- **Area:** USER (Storefront)
- **Module Name:** Shop
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Provides catalog browsing, product search, filter sidebar, pagination, product detail views, and variant SKU price/stock resolution.

## Implemented Features
- **Product Listing:** Paginated list of active products with online visibility enabled.
- **Search & Filter:** Keyword search, Category filter, SubCategory filter, Brand filter, Price range filter.
- **Product Details:** Detailed view displaying primary image/video (`ProductMedia`), description, starting price (MIN selling price of active SKUs), and available SKU variants.
- **SKU Picker:** Resolves specific SKU combination, MRP, selling price, and stock availability (`QuantityAvailable`).

## Filters / Search / Sorting
- **Filters Implemented:** `CategoryId`, `SubCategoryId`, `BrandId`, `MinPrice`, `MaxPrice`.
- **Search Implemented:** `SearchText` matching product name/description.
- **Paging:** `PageIndex`, `PageSize` returning `TableOutput<Product>`.

## Data
- DTOs: `InputProduct`, `Product`, `ProductDetails`, `ShopFilter`, `TableSettings`.

## Database Dependencies
- Tables: `Products`, `ProductVariants`, `Categories`, `SubCategory`, `Brands`, `ProductMedia`, `SkuMedia`, `Stock`.
- Stored Procedures: `GetProducts`, `GetProductDetails`, `GetShopFilters`.

## API & Data Access
- Controller: `Controllers/ShopController.cs` (`Index`, `GetProducts`, `GetFilters`, `Details`, `GetProduct`).
- Repository: `ShopRepository.cs` via `IDataAccessDapper`.

## UI Rules & Conventions
- Listing page: `views/Shop/Index.cshtml` with AJAX fetch and dynamic pagination.
- Details page: `views/Shop/Details.cshtml` with media gallery and interactive variant selector, a rating summary under the product name and the **read-only** Ratings & Reviews section (no write form; customers review purchased items from My Orders → Order Details — see `USER/modules/reviews.md`).

## Business Rules
- Only displays products where `IsActive = 1`, `SellOnline = 1`, and `Cancelled = 0`.
- Only displays SKUs (variants) where `ProductVariants.IsActive = 1`, `ProductVariants.SellOnline = 1`, and `Cancelled = 0`.
- Does **not** require customer login to browse products or view details.

## Dependencies & Related Modules
- `ADMIN/modules/product.md` — Creates products and uploads `ProductMedia`.
- `ADMIN/modules/product-variant.md` — Defines SKUs and pricing.
- `USER/modules/cart.md` — Customer adds selected SKU to cart.
- `USER/modules/wishlist.md` — Customer adds product to wishlist.
