# Admin Module — SubCategory

## Module Overview
- **Area:** ADMIN
- **Module Name:** SubCategory
- **Current Status:** `IMPLEMENTED` (server + AJAX UI, built on the Category pattern)

## Purpose & Responsibilities
Company-wide sub-categories, each belonging to exactly one category (`fk_category`): list with a category filter, create, edit (including moving to another category), activate/deactivate, and a JSON lookup for cascading product pickers.

## Implemented Features
- `Catalog/SubCategories` list: category filter, search (sub-category or category name, case-insensitive), status filter (All / Active / Inactive), sortable columns (category, sub-category, order, status), page size + pagination, empty and "no match" states.
- `Catalog/CreateSubCategory` and `Catalog/EditSubCategory` full-page forms: category dropdown, name, description, display order, active flag. AJAX save through `SmartCartAjax.submitForm`, field errors under inputs, redirect to the list (or `returnUrl`) with a flash toast.
- `Catalog/SetSubCategoryActive` (POST, `products.manage`) — soft activate/deactivate from the list after a confirm dialog; **no hard delete**.
- `Catalog/SubCategoryLookup?fkCategory=` JSON (active rows of one category) for product forms — unchanged.
- Manage buttons (Add / Edit / Activate / Deactivate) are hidden without `products.manage`.
- Filter API mode 3 returns `FillSubCategory` (shared dropdown fill, unchanged).

## Filters / Search / Sorting
- List query string (model `InputSubCategoryList`): `FkCategory` (empty = all categories), `Search`, `ActiveOnly` (`true` / `false`; empty = all), `SortBy` (`categoryname` default, `subcategoryname`, `displayorder`, `isactive`), `SortDesc`, `Page`, `PageSize` (default 25, max 100).
- The service normalises the criteria (page/page-size clamp, sort default, search trim, `FkCategory <= 0` → none) and re-queries the last page when the requested page is past the end.
- Dropdown fill: criteria empty → all; criteria = category id → filtered.

## Data
- Entity: `SubCategory` → `subcategories` (repository only).
- Models: `Models/CatalogModels.cs` — `InSubCategoryCreateView` / `InSubCategoryUpdateView`, `InputSubCategory{Create,Update,Select,List}`, `OutputSubCategory{Create,Update,Select,List}` (`OutputSubCategoryList.Filter` echoes the criteria for the view).
- Category dropdown source: `GetCategoryOptionsAsync` → `OutputCategorySelect` (non-cancelled categories incl. `IsActive`); the lookup endpoint keeps returning active categories only.
- Fill: `FillSubCategory` in `FilterModels.cs`.

## Database Dependencies
- Table: `subcategories` (`fk_category` → `categories`); `uq_subcategories_category_name` on `(fk_category, subcategoryname)` (case-sensitive, includes cancelled rows).
- `products.fk_subcategory` references it.

## API & Data Access
- Controller: `CatalogController` (sub-category region) → `ICatalogService` → `ICatalogRepository` (EF).

| Route | Verb | Result | Permission |
|---|---|---|---|
| `Catalog/SubCategories` | GET | `Views/SubCategory/SubCategories.cshtml` | `products.view` |
| `Catalog/CreateSubCategory?fkCategory=&returnUrl=` | GET | `Views/SubCategory/CreateSubCategory.cshtml` | `products.manage` |
| `Catalog/CreateSubCategory` | POST | JSON (`redirectUrl`) | `products.manage` |
| `Catalog/EditSubCategory/{id}?returnUrl=` | GET | `Views/SubCategory/EditSubCategory.cshtml` (404 when missing) | `products.manage` |
| `Catalog/EditSubCategory/{id}` | POST | JSON (`redirectUrl`) | `products.manage` |
| `Catalog/SetSubCategoryActive` (`id`, `isActive`) | POST | JSON | `products.manage` |
| `Catalog/SubCategoryLookup?fkCategory=` | GET | JSON | signed in + module `PRODUCTS` |

- Module `PRODUCTS` (class-level `[ModuleAccess]`). Every POST is `[ValidateAntiForgeryToken]`; failures are thrown typed exceptions (400 `ValidationException` with `FkCategory` field error, 404 `ResourceNotFoundException`, 409 `ConflictException`).

## UI Rules & Conventions
- Views: `Views/SubCategory/SubCategories.cshtml`, `CreateSubCategory.cshtml`, `EditSubCategory.cshtml` (explicit `View("~/Views/SubCategory/…")` paths).
- JS: `wwwroot/js/catalog/subcategory.js` (form submit + list busy state) on top of `wwwroot/js/sc-ajax.js`; activate/deactivate buttons use the declarative `data-sc-post` + `data-sc-confirm` hooks and reload with a toast.
- The edit form lists active categories plus the current parent even when that parent is inactive (labelled "(inactive)"), so an unchanged record can always be saved.
- Create form shows a warning with a link to Categories when no active category exists.

## Business Rules
- Soft cancel only; the UI never hard-deletes.
- Name unique per category: checked case-insensitively against **all** rows of that category (including cancelled ones, as the DB constraint does) → 409.
- The parent category must exist, not be cancelled and be active on create or when moving to another category (400, shown under the Category field). An unchanged inactive parent never blocks edits or status toggles.
- Display order: lower numbers first (default 0); both forms post it, so editing never resets it.
- Company-wide (no `fk_store`), so the Platform Super Admin "no store" panel does not apply.

## Dependencies & Related Modules
- `category.md`, `product.md`, `filter.md`

## Notes
- Query parameter is `ActiveOnly` (same as Category in code); `ADMIN.md` still says `status`.
- Deactivating a category does not change its sub-categories.
- There is no UI to restore a cancelled sub-category; its name stays reserved within its category (409).
- Sidebar entry (Catalog → Sub-categories, `Catalog/SubCategories`, `products.view`) must be added to `_Layout.cshtml` by the layout owner.
