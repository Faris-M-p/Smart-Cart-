# Admin Module — SubCategory

## Module Overview
- **Area:** ADMIN (Back Office)
- **Module Name:** SubCategory
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Manages second-level subcategories linked to a parent Category (e.g., Mobile Phones under Electronics). Handles SubCategory CRUD, parent category association, active status, image uploads, and soft deletes.

## Implemented Features
- **SubCategory Listing:** Paginated list with text search, parent category filter, and sorting.
- **Create SubCategory:** Select parent `FK_Category`, enter Name, Description, Image upload, `IsActive`.
- **Update SubCategory:** Edit subcategory details and parent association.
- **Toggle Active:** Toggle `IsActive` flag.
- **Soft Delete:** Sets `Cancelled = 1` with cancellation reason.

## Filters / Search / Sorting
- **Search:** `SearchText` matching SubCategory Name / Description.
- **Filter:** `CategoryId` parent filter.
- **Response Format:** `ApiResponse<TableOutput<SubCategoryViewModel>>`.

## Data
- Entities / DTOs: `SubCategory`, `SubCategoryViewModel`, `SubCategoryInputModel`.

## Database Dependencies
- Tables: `SubCategory` (PK `ID_SubCategory`, FK `FK_Category`), `Categories`.
- Access: EF Core (`EcommerceDbContext`) & `SubCategoryRepository`.

## API & Data Access
- Controller: `Controllers/Admin/SubCategoryController.cs` (`[Route("Admin/SubCategory")]`).
- Interface & Repository: `ISubCategoryInterface.cs` / `SubCategoryRepository.cs`.
- Helper: `SubCategoryHelper.cs`.
- Permissions: `SubCategory.View`, `SubCategory.Create`, `SubCategory.Update`, `SubCategory.Delete`.

## UI Rules & Conventions
- Razor view: `views/Admin/SubCategory/Index.cshtml`.
- Parent category selector populated via dynamic AJAX endpoint.
- Images uploaded via `CommonImageService` into `wwwroot/uploads/subcategories/`.

## Business Rules
- `FK_Category` is required.
- Soft delete (`Cancelled = 1`).

## Dependencies & Related Modules
- `ADMIN/modules/category.md` — Parent Category reference.
- `ADMIN/modules/product.md` — Products reference SubCategory.
