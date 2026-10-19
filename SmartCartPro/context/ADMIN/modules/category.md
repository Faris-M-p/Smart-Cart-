# Admin Module — Category

## Module Overview
- **Area:** ADMIN
- **Module Name:** Category
- **Current Status:** `IMPLEMENTED` (server + AJAX UI; reference implementation for all master screens)

## Purpose & Responsibilities
Company-wide product categories: list, create, edit, activate/deactivate, lookup for pickers.

## Implemented Features
- `Catalog/Categories` list: search, status filter, sorting, pagination, empty state, confirm → POST → toast.
- `Catalog/CreateCategory` and `Catalog/EditCategory` full-page forms (AJAX save, field errors under inputs, redirect to the list).
- `Catalog/SetCategoryActive` (POST, `products.manage`) — soft activate/deactivate; **no hard delete**.
- `Catalog/CategoryLookup` JSON for dropdowns; Filter API mode 2 for shared fills.
- Manage buttons are hidden without `products.manage`.

## Filters / Search / Sorting
- Query string: `search`, `status` (`all|active|inactive`), `sortBy`, `sortDesc`, `page`, `pageSize`.

## Data
- Entity: `Category` → `categories` (repository only).
- Models: `Models/CatalogModels.cs` — `InCategoryCreateView` / `InCategoryUpdateView`, `InputCategory*`, `OutputCategory*`.
- Fill: `FillCategory` in `FilterModels.cs`.

## Database Dependencies
- Table: `categories`

## API & Data Access
- Controller: `CatalogController` (category actions) → `ICatalogService` → `ICatalogRepository`.
- Permissions: `products.view` (list/lookup), `products.manage` (create/edit/activate). Module `PRODUCTS`.

## UI Rules & Conventions
- Views: `Views/Category/Categories.cshtml`, `CreateCategory.cshtml`, `EditCategory.cshtml` (explicit `View("~/Views/Category/…")` paths from `CatalogController`).
- JS: `wwwroot/js/catalog/category.js` on top of `wwwroot/js/sc-ajax.js`.
- Follow `ADMIN.md` → *Front-end conventions*; this module is the template for Sub-category, Brand, Unit and Tax.

## Business Rules
- Soft cancel; unique category name (409 `ConflictException` on duplicates).
- Company-wide (no `fk_store`).

## Dependencies & Related Modules
- `subcategory.md`, `product.md`, `filter.md`
