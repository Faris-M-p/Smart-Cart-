# Admin Module — SubCategory

## Module Overview
- **Area:** ADMIN
- **Module Name:** SubCategory
- **Current Status:** `PARTIALLY IMPLEMENTED` (filter fill ready; dedicated CRUD UI may use Catalog)

## Purpose & Responsibilities
Subcategories under a category (`fk_category`).

## Implemented Features
- Filter API mode 3 returns `FillSubCategory`
- Criteria empty → all; criteria = category id → filtered

## Filters / Search / Sorting
- Criteria / Search on `FilterOptions`

## Data
- Entity: `SubCategory` → `subcategories`
- Fill: `FillSubCategory` in `FilterModels.cs`

## Database Dependencies
- Table: `subcategories` (`fk_category` → `categories`)

## API & Data Access
- `FilterRepository.GetSubCategoriesAsync(FilterOptions)`
- Future Catalog CRUD actions as needed

## UI Rules & Conventions
- Dependent dropdown: load categories (mode 2), then subcategories (mode 3 + criteria)

## Business Rules
- Unique `(fk_category, subcategoryname)` among active rows
- Soft cancel

## Dependencies & Related Modules
- `category.md`, `product.md`, `filter.md`