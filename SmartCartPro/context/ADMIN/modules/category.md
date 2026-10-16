# Admin Module — Category

## Module Overview
- **Area:** ADMIN
- **Module Name:** Category
- **Current Status:** `UI SHELL`

## Purpose & Responsibilities
Company-wide product categories.

## Implemented Features
- `Catalog/Categories` list view with sample rows
- Filter fill: mode 2 via Filter API

## Filters / Search / Sorting
- Planned: search by name; Filter API `Search`

## Data
- Entity: `Category` → `categories`
- Fill: `FillCategory` in `FilterModels.cs`

## Database Dependencies
- Table: `categories`
- Access: EF (`Category` entity / future Catalog service)

## API & Data Access
- Controller: `CatalogController.Categories`
- Filter: `FilterController` mode=2

## UI Rules & Conventions
- View: `Views/Catalog/Categories.cshtml`

## Business Rules
- Soft cancel; unique category name
- Company-wide (no `fk_store`)

## Dependencies & Related Modules
- `subcategory.md`, `product.md`, `filter.md`