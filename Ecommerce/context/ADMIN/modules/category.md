# Admin Module — Category

## Module Overview
- **Area:** ADMIN (Back Office)
- **Module Name:** Category
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Manages top-level product categories in the catalog (e.g., Electronics, Grocery, Dairy). Handles Category CRUD operations, active status toggles, image uploads, and soft deletes.

## Implemented Features
- **Category Listing:** AJAX POST `GetCategoryList` with pagination, text search, and sorting.
- **Create Category:** Modal form to create category with Name, Description, Image upload, and IsActive flag.
- **Update Category:** Modal form to edit category properties and image.
- **Toggle Active:** Single-click toggle for `IsActive` state.
- **Soft Delete:** Soft deletes category by setting `Cancelled = 1` with cancellation reason.

## Filters / Search / Sorting
- **Search:** `SearchText` matching Name and Description.
- **Pagination:** `PageIndex`, `PageSize` normalized via `CategoryHelper`.
- **Response Format:** `ApiResponse<TableOutput<CategoryViewModel>>`.

## Data
- Entities / DTOs: `Category`, `CategoryViewModel`, `CategoryInputModel`.

## Database Dependencies
- Tables: `Categories` (PK `ID_Category`).
- Access: EF Core (`EcommerceDbContext`) & `CategoryRepository`.

## API & Data Access
- Controller: `Controllers/Admin/CategoryController.cs` (`[Route("Admin/Category")]`).
- Interface & Repository: `ICategoryInterface.cs` / `CategoryRepository.cs`.
- Helper: `CategoryHelper.cs`.
- Permissions: `Category.View`, `Category.Create`, `Category.Update`, `Category.Delete`.

## UI Rules & Conventions
- Single Razor view: `views/Admin/Category/Index.cshtml` with modal dialogs and dynamic table reloading.
- Shared toast notifications (`toast.js`) and empty state handler (`emptyState`).
- Category images uploaded via `CommonImageService` into `wwwroot/uploads/categories/`.

## Business Rules
- Soft delete (`Cancelled = 1`). Hard deletion is forbidden.
- Deactivating or cancelling a category affects visibility of associated subcategories and products in the storefront.

## Dependencies & Related Modules
- `ADMIN/modules/subcategory.md` — SubCategories belong to a Category.
- `ADMIN/modules/product.md` — Products belong to SubCategories under Categories.
