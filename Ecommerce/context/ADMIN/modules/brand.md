# Admin Module — Brand

## Module Overview
- **Area:** ADMIN (Back Office)
- **Module Name:** Brand
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Manages manufacturer and supplier brands (e.g., Apple, Samsung, Nestle). Handles Brand CRUD operations, logo uploads, active state toggles, and soft deletes.

## Implemented Features
- **Brand Listing:** Paginated list with keyword search and status filter.
- **Create Brand:** Modal form with BrandName, Description, Logo Image upload, and `IsActive` flag.
- **Update Brand:** Edit brand info and replace logo image.
- **Toggle Active:** Toggle `IsActive` state.
- **Soft Delete:** Sets `Cancelled = 1` with cancellation reason.

## Filters / Search / Sorting
- **Search:** `SearchText` matching BrandName.
- **Response Format:** `ApiResponse<TableOutput<BrandViewModel>>`.

## Data
- Entities / DTOs: `Brand`, `BrandViewModel`, `BrandInputModel`.

## Database Dependencies
- Tables: `Brands` (PK `ID_Brand`).
- Access: EF Core (`EcommerceDbContext`) & `BrandRepository`.

## API & Data Access
- Controller: `Controllers/Admin/BrandController.cs` (`[Route("Admin/Brand")]`).
- Interface & Repository: `IBrandInterface.cs` / `BrandRepository.cs`.
- Helper: `BrandHelper.cs`.
- Permissions: `Brand.View`, `Brand.Create`, `Brand.Update`, `Brand.Delete`.

## UI Rules & Conventions
- Razor view: `views/Admin/Brand/Index.cshtml`.
- Brand logos uploaded via `CommonImageService` into `wwwroot/uploads/brands/`.

## Business Rules
- Soft delete (`Cancelled = 1`).
- Brands can be associated with products across multiple categories.

## Dependencies & Related Modules
- `ADMIN/modules/product.md` — Products reference optional `FK_Brand`.
