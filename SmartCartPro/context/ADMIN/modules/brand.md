# Admin Module — Brand

## Module Overview
- **Area:** ADMIN
- **Module Name:** Brand
- **Current Status:** `UI SHELL`

## Purpose & Responsibilities
Company-wide brand master.

## Implemented Features
- `Catalog/Brands` sample list view

## Filters / Search / Sorting
- Planned

## Data
- Table: `brands` (EF entity may be added when CRUD is wired)

## Database Dependencies
- `brands`; products reference `fk_brand`

## API & Data Access
- Controller: `CatalogController.Brands`
- View: `Views/Catalog/Brands.cshtml`

## UI Rules & Conventions
- Metis table shell

## Business Rules
- Soft cancel; unique brand name
- Company-wide

## Dependencies & Related Modules
- `product.md`