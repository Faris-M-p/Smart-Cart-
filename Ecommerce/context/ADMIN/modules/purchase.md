# Admin Module — Purchase

## Module Overview
- **Area:** ADMIN (Back Office)
- **Module Name:** Purchase
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Manages stock intake orders from suppliers. Creates purchase headers and purchase detail line items, automatically inserting matching SKU batch records into `Stock`.

## Implemented Features
- **Purchase Listing:** Paginated table listing purchase orders by date, supplier, invoice number, and total amount.
- **Create Purchase:** Select Supplier (`FK_Supplier`), PurchaseDate, InvoiceNumber, Notes, and line items (SKU `FK_ProductVariant`, Quantity, PurchasePrice).
- **Auto Stock Creation:** On purchase creation, automatically inserts batch rows into `Stock` table (`FK_PurchaseDetail`, `FK_ProductVariant`, `Quantity`).
- **View Purchase Details:** Displays header info and purchase detail line items.
- **Soft Delete:** Sets `Cancelled = 1` on purchase and soft-deletes associated stock batches.

## Filters / Search / Sorting
- **Filters:** `SupplierId`, Date range.
- **Search:** `SearchText` matching InvoiceNumber or Notes.

## Data
- Entities / DTOs: `Purchase`, `PurchaseDetail`, `Stock`, `PurchaseViewModel`, `PurchaseInputModel`.

## Database Dependencies
- Tables: `Purchase` (PK `ID_Purchase`), `PurchaseDetail` (PK `ID_PurchaseDetail`), `Stock` (PK `ID_Stock`), `Supplier`, `ProductVariants`.
- Access: EF Core (`EcommerceDbContext`) & `PurchaseRepository`.

## API & Data Access
- Controller: `Controllers/Admin/PurchaseController.cs` (`[Route("Admin/Purchase")]`).
- Interface & Repository: `IPurchaseInterface.cs` / `PurchaseRepository.cs`.
- Helper: `PurchaseHelper.cs`.
- Permissions: `Purchase.View`, `Purchase.Create`, `Purchase.Update`, `Purchase.Delete`.

## UI Rules & Conventions
- Views: `views/Admin/Purchase/Index.cshtml`, `views/Admin/Purchase/Create.cshtml`, `views/Admin/Purchase/Details.cshtml`.
- Dynamic JS line item table for multi-SKU purchase entry.

## Business Rules
- Creating a Purchase automatically increases available SKU stock via `Stock` batch insertion.
- Soft delete (`Cancelled = 1`).

## Dependencies & Related Modules
- `ADMIN/modules/supplier.md` — Source of supplier selection.
- `ADMIN/modules/product-variant.md` — Source of SKU selection.
- `ADMIN/modules/inventory.md` — Stock levels created by purchases.
