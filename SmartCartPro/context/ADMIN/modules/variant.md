# Admin Module — Variant / SKU

## Module Overview
- **Area:** ADMIN
- **Module Name:** Variant
- **Current Status:** `UI SHELL`

## Purpose & Responsibilities
Product variants / SKU codes and barcodes (`product_variants`).

## Implemented Features
- `Catalog/Variants` sample list

## Filters / Search / Sorting
- Planned

## Data
- Table: `product_variants`

## Database Dependencies
- `product_variants.fk_product` → `products`

## API & Data Access
- `CatalogController.Variants`
- View: `Views/Catalog/Variants.cshtml`

## UI Rules & Conventions
- Metis table shell

## Business Rules
- Soft cancel; unique variant name per product; unique sku/barcode when set

## Dependencies & Related Modules
- `product.md`, `inventory.md`