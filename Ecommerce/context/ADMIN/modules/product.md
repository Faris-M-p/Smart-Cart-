# Admin Module — Product

## Module Overview
- **Area:** ADMIN (Back Office)
- **Module Name:** Product
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Manages the master product catalog, product basic information, category/brand classification, online visibility toggles (`SellOnline`), and product media (images and video).

## Implemented Features
- **Product Listing:** Paginated table with text search, category filter, brand filter, and online status toggle.
- **Create Product:** Name, Slug generation, Description, `FK_SubCategory`, `FK_Brand`, `IsActive`, `SellOnline`.
- **Update Product:** Edit product header information.
- **Product Media Management:** Upload up to 5 media items (images JPG/PNG/WebP max 2MB, optional single video MP4/WebM max 20MB) stored in `ProductMedia` table.
- **Toggle SellOnline:** Individual toggle controlling storefront visibility.
- **Soft Delete:** Sets `Cancelled = 1`.

## Filters / Search / Sorting
- **Search:** `SearchText` matching Product Name, Description, or Slug.
- **Filters:** `CategoryId`, `SubCategoryId`, `BrandId`, `IsActive`, `SellOnline`.
- **Response Format:** `ApiResponse<TableOutput<ProductViewModel>>`.

## Data
- Entities / DTOs: `Products`, `ProductMedia`, `ProductViewModel`, `ProductInputModel`.

## Database Dependencies
- Tables: `Products` (PK `ID_Product`), `ProductMedia` (PK `ID_ProductMedia`), `SubCategory`, `Brands`.
- Access: EF Core (`EcommerceDbContext`) & `ProductRepository`.

## API & Data Access
- Controller: `Controllers/Admin/ProductController.cs` (`[Route("Admin/Product")]`).
- Interface & Repository: `IProductInterface.cs` / `ProductRepository.cs`.
- Media Service: `CommonImageService.cs`.
- Helper: `ProductHelper.cs`.
- Permissions: `Product.View`, `Product.Create`, `Product.Update`, `Product.Delete`, `Product.Media`.

## UI Rules & Conventions
- Views: `views/Admin/Product/Index.cshtml`, `views/Admin/Product/Media.cshtml`.
- Media uploaded to `wwwroot/uploads/products/`.

## Business Rules
- `SellOnline` defaults to `0` (Off). Product will NOT appear on storefront until `SellOnline = 1` AND `IsActive = 1` AND at least one active SKU exists.
- Slug must be unique (`UQ_Products_Slug`).
- Soft delete (`Cancelled = 1`).

## Dependencies & Related Modules
- `ADMIN/modules/subcategory.md` & `ADMIN/modules/brand.md` — Foreign keys.
- `ADMIN/modules/product-variant.md` — Product contains one or more SKUs.
- `USER/modules/shop.md` — Storefront reads active `SellOnline` products.
