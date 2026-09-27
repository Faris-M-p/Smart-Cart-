# Admin Module — Product Variant (SKUs)

## Module Overview
- **Area:** ADMIN (Back Office)
- **Module Name:** Product Variant (SKUs)
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Manages SKU-level product variants (e.g., "500g Pack", "1kg Pack"), SKU pricing (MRP vs Selling Price), SKU attributes, barcode, UOM, SKU media, and individual SKU `SellOnline` toggles.

## Implemented Features
- **SKU Listing:** Paginated list of SKUs belonging to a product or global catalog.
- **Create SKU:** Generates unique SKU code, Barcode, MRP, SellingPrice, UnitOfMeasure (g, kg, ml, L, Pcs), UnitValue, `MaxOrderQty`, `IsDefault`, `SellOnline`.
- **SKU Attributes:** Maps SKU to variant axes/values (`ProductVariantAttributes`).
- **SKU Media:** Upload image specific to the SKU (`SkuMedia` / `ProductVariantImages`).
- **Toggle SellOnline:** SKU-level online visibility control.
- **Soft Delete:** Sets `Cancelled = 1`.

## Filters / Search / Sorting
- **Filters:** `ProductId`, `IsActive`, `SellOnline`.
- **Search:** `SearchText` matching SKU code, Barcode, or VariantLabel.

## Data
- Entities / DTOs: `ProductVariants`, `ProductVariantAttributes`, `SkuMedia`, `ProductVariantViewModel`, `ProductVariantInputModel`.

## Database Dependencies
- Tables: `ProductVariants` (PK `ID_ProductVariant`, FK `FK_Product`), `ProductVariantAttributes`, `SkuMedia`, `Variants`, `VariantValues`.
- Access: EF Core (`EcommerceDbContext`) & `ProductVariantRepository`.

## API & Data Access
- Controller: `Controllers/Admin/ProductVariantController.cs` (`[Route("Admin/ProductVariant")]`).
- Interface & Repository: `IProductVariantInterface.cs` / `ProductVariantRepository.cs`.
- Helper: `ProductVariantHelper.cs`.
- Permissions: `ProductVariant.View`, `ProductVariant.Create`, `ProductVariant.Update`, `ProductVariant.Delete`.

## UI Rules & Conventions
- Razor view: `views/Admin/ProductVariant/Index.cshtml`.
- Attribute matrix selection built dynamically via JS.

## Business Rules
- SKU code and Barcode must be unique across the entire database.
- SellingPrice must be $\le$ MRP.
- `SellOnline` defaults to `0`. Must be explicitly set to `1` for the SKU to be purchasable on storefront.
- Stock is linked strictly to `ID_ProductVariant`.

## Dependencies & Related Modules
- `ADMIN/modules/product.md` — Parent Product reference.
- `ADMIN/modules/variant-value.md` — Variant values mapped in attributes.
- `ADMIN/modules/purchase.md` & `ADMIN/modules/inventory.md` — Stock intake & adjustment per SKU.
- `USER/modules/cart.md` — Shopping cart item reference.
