# Admin Module — Inventory (Stock Adjustments)

## Module Overview
- **Area:** ADMIN (Back Office)
- **Module Name:** Inventory (Stock Adjustments)
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Provides SKU-level inventory monitoring and manual stock adjustments (spoilage, breakage, inventory audit count corrections).

## Implemented Features
- **Stock Levels Overview:** Displays total available stock quantity per SKU (`SUM(Quantity)` from non-cancelled `Stock` rows).
- **Manual Stock Adjustment:** Allows admin to insert an inventory adjustment entry (`+` or `-` quantity) for a target SKU.
- **Stock Batch History:** Displays history of purchase batches and adjustment entries for a given SKU.

## Filters / Search / Sorting
- **Filters:** `ProductId`, Low stock filter.
- **Search:** `SearchText` matching SKU code, Barcode, or Product Name.

## Data
- Entities / DTOs: `Stock`, `StockAdjustmentInputModel`, `StockViewModel`.

## Database Dependencies
- Tables: `Stock` (PK `ID_Stock`, `FK_ProductVariant`, `Quantity`, `FK_PurchaseDetail`).
- Access: EF Core (`EcommerceDbContext`) & `InventoryRepository`.

## API & Data Access
- Controller: `Controllers/Admin/InventoryController.cs` (`[Route("Admin/Inventory")]`).
- Interface & Repository: `IInventoryInterface.cs` / `InventoryRepository.cs`.
- Helper: `InventoryHelper.cs`.
- Permissions: `Inventory.View`, `Inventory.Adjust`.

## UI Rules & Conventions
- Razor view: `views/Admin/Inventory/Index.cshtml`.
- Quick modal for stock adjustment entry.

## Business Rules
- Stock is calculated at the **SKU level** (`FK_ProductVariant`), not product header level.
- Adjustments create a new `Stock` row. Runtime adjust entries pass `FK_PurchaseDetail = 0` (or matching purchase detail).
- Total available stock quantity is the sum of non-cancelled `Quantity` values.

## Dependencies & Related Modules
- `ADMIN/modules/product-variant.md` — Target SKU reference.
- `ADMIN/modules/purchase.md` — Source of purchase stock batches.
- `USER/modules/cart.md` & `USER/modules/checkout.md` — Verified and deducted during customer purchase.
