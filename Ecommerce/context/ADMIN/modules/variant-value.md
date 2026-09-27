# Admin Module — Variant Value

## Module Overview
- **Area:** ADMIN (Back Office)
- **Module Name:** Variant Value
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Manages individual values assigned to a Variant Axis (e.g., "Small", "Medium", "Large" under Size axis; "Red", "Blue" under Color axis).

## Implemented Features
- **Variant Value Listing:** Paginated list filtered by parent Variant axis.
- **Create Value:** Add value (e.g., "500g") linked to `FK_Variant`.
- **Update Value:** Edit value text and display order.
- **Soft Delete:** Sets `Cancelled = 1`.

## Filters / Search / Sorting
- **Filter:** `VariantId` parent axis filter.
- **Search:** `SearchText` matching ValueName.

## Data
- Entities / DTOs: `VariantValues`, `VariantValueViewModel`, `VariantValueInputModel`.

## Database Dependencies
- Tables: `VariantValues` (PK `ID_VariantValue`, FK `FK_Variant`), `Variants`.
- Access: EF Core (`EcommerceDbContext`) & `VariantValueRepository`.

## API & Data Access
- Controller: `Controllers/Admin/VariantValueController.cs` (`[Route("Admin/VariantValue")]`).
- Interface & Repository: `IVariantValueInterface.cs` / `VariantValueRepository.cs`.
- Helper: `VariantValueHelper.cs`.
- Permissions: `VariantValue.View`, `VariantValue.Create`, `VariantValue.Update`, `VariantValue.Delete`.

## UI Rules & Conventions
- View: `views/Admin/VariantValue/Index.cshtml`.

## Business Rules
- Must be linked to a valid non-cancelled `FK_Variant` axis.
- Soft delete (`Cancelled = 1`).

## Dependencies & Related Modules
- `ADMIN/modules/variant.md` — Parent Variant axis reference.
- `ADMIN/modules/product-variant.md` — Selected in `ProductVariantAttributes`.
