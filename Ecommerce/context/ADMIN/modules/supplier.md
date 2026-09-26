# Admin Module — Supplier

## Module Overview
- **Area:** ADMIN (Back Office)
- **Module Name:** Supplier
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Manages vendor/supplier profiles for purchasing and stock intake. Includes company details, contact information, address details, and India location cascading dropdown integration.

## Implemented Features
- **Supplier Listing:** Paginated table with text search and location filters.
- **Create Supplier:** CompanyName, Contact Name, Email, Phone, State, District, City, Address, Pincode, `IsActive`.
- **Update Supplier:** Edit supplier info and address.
- **Location Integration:** Optional HTTP integration (`CountryStateCity` API) to populate India states, districts, and cities dynamically.
- **Soft Delete:** Sets `Cancelled = 1`.

## Filters / Search / Sorting
- **Search:** `SearchText` matching CompanyName, Name, Email, or Phone.

## Data
- Entities / DTOs: `Supplier`, `SupplierViewModel`, `SupplierInputModel`.

## Database Dependencies
- Tables: `Supplier` (PK `ID_Supplier`).
- Access: EF Core (`EcommerceDbContext`) & `SupplierRepository`.

## API & Data Access
- Controller: `Controllers/Admin/SupplierController.cs` (`[Route("Admin/Supplier")]`).
- Interface & Repository: `ISupplierInterface.cs` / `SupplierRepository.cs`.
- Helper: `SupplierHelper.cs`.
- External Service: Optional `CountryStateCity` HTTP client.
- Permissions: `Supplier.View`, `Supplier.Create`, `Supplier.Update`, `Supplier.Delete`.

## UI Rules & Conventions
- View: `views/Admin/Supplier/Index.cshtml`.
- Cascading dropdown JS for State -> District -> City.

## Business Rules
- Soft delete (`Cancelled = 1`).
- Active suppliers are required when creating Purchase stock intake orders.

## Dependencies & Related Modules
- `ADMIN/modules/purchase.md` — Purchase orders reference `FK_Supplier`.
