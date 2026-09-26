# Admin Module — Variant (Axes)

## Module Overview
- **Area:** ADMIN (Back Office)
- **Module Name:** Variant (Variant Axes)
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Manages variant axes definitions (e.g., Size, Color, Flavor, Storage Capacity) used to differentiate SKU variations of a product.

## Implemented Features
- **Variant Listing:** Paginated list of variant axes.
- **Create Variant Axis:** Creates a new variant axis (e.g., "Size", "Color").
- **Update Variant Axis:** Edit axis name and description.
- **Soft Delete:** Sets `Cancelled = 1`.

## Filters / Search / Sorting
- **Search:** `SearchText` matching Variant Name.

## Data
- Entities / DTOs: `Variants`, `VariantViewModel`, `VariantInputModel`.

## Database Dependencies
- Tables: `Variants` (PK `ID_Variant`).
- Access: EF Core (`EcommerceDbContext`) & `VariantRepository`.

## API & Data Access
- Controller: `Controllers/Admin/VariantController.cs` (`[Route("Admin/Variant")]`).
- Interface & Repository: `IVariantInterface.cs` / `VariantRepository.cs`.
- Helper: `VariantHelper.cs`.
- Permissions: `Variant.View`, `Variant.Create`, `Variant.Update`, `Variant.Delete`.

## UI Rules & Conventions
- View: `views/Admin/Variant/Index.cshtml`.

## Business Rules
- Variant axes define the dimension/attribute type (e.g., Color). Specific options (Red, Blue) belong to `VariantValue`.
- Soft delete (`Cancelled = 1`).

## Dependencies & Related Modules
- `ADMIN/modules/variant-value.md` — Variant values belong to a Variant axis.
- `ADMIN/modules/product-variant.md` — Product SKUs map attributes to Variant axes.
