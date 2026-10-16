# Admin Module — Product

## Module Overview
- **Area:** ADMIN
- **Module Name:** Product
- **Current Status:** `UI SHELL` (entity + filter fill exist)

## Purpose & Responsibilities
Company-wide product master (shared across stores). Stock is store-scoped separately.

## Implemented Features
- `Catalog/Products` list UI (sample)
- Entity `Product` mapped in EF
- Filter mode 1 → `FillProduct`

## Filters / Search / Sorting
- Filter API search on product name
- UI filters planned (category/brand)

## Data
- Entity: `Product`
- Fill: `FillProduct` (`ID_value` = `id_product`, `Name` = `productname`)

## Database Dependencies
- `products` (+ optional `fk_category`, `fk_subcategory`, `fk_brand`, `fk_unit`, `fk_tax`)

## API & Data Access
- `CatalogController.Products`
- `IProductService` / `ProductRepository` (scaffold)
- `FilterController` mode=1

## UI Rules & Conventions
- View: `Views/Catalog/Products.cshtml`

## Business Rules
- Soft cancel; unique productcode / sku / barcode where set
- Company-wide master — do not duplicate per store

## Dependencies & Related Modules
- `category.md`, `subcategory.md`, `brand.md`, `variant.md`, `inventory.md`, `filter.md`