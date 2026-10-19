# Admin Module — Brand

## Module Overview
- **Area:** ADMIN
- **Module Name:** Brand
- **Current Status:** `IMPLEMENTED` (server + AJAX UI, built on the Category pattern)

## Purpose & Responsibilities
Company-wide brand master: list, create, edit, activate/deactivate, and a JSON lookup for product forms.

## Implemented Features
- `Catalog/Brands` list: search (brand name, case-insensitive), status filter (All / Active / Inactive), sortable columns (brand, status), page size + pagination, empty and "no match" states.
- `Catalog/CreateBrand` and `Catalog/EditBrand` full-page forms: name, description, active flag. AJAX save through `SmartCartAjax.submitForm`, field errors under inputs, redirect to the list (or `returnUrl`) with a flash toast.
- `Catalog/SetBrandActive` (POST, `products.manage`) — soft activate/deactivate from the list after a confirm dialog; **no hard delete**.
- `Catalog/BrandLookup` JSON (active brands) for product forms — unchanged.
- Manage buttons (Add / Edit / Activate / Deactivate) are hidden without `products.manage`.

## Filters / Search / Sorting
- List query string (model `InputBrandList`): `Search`, `ActiveOnly` (`true` / `false`; empty = all), `SortBy` (`brandname` default, `isactive`), `SortDesc`, `Page`, `PageSize` (default 25, max 100).
- The service normalises the criteria (page/page-size clamp, sort default, search trim) and re-queries the last page when the requested page is past the end.

## Data
- Entity: `Brand` → `brands` (repository only).
- Models: `Models/CatalogModels.cs` — `InBrandCreateView` / `InBrandUpdateView`, `InputBrand{Create,Update,Select,List}`, `OutputBrand{Create,Update,Select,List}` (`OutputBrandList.Filter` echoes the criteria for the view).

## Database Dependencies
- Table: `brands`; `uq_brands_brandname` (case-sensitive, includes cancelled rows).
- `products.fk_brand` references it.

## API & Data Access
- Controller: `CatalogController` (brand region) → `ICatalogService` → `ICatalogRepository` (EF).

| Route | Verb | Result | Permission |
|---|---|---|---|
| `Catalog/Brands` | GET | `Views/Brand/Brands.cshtml` | `products.view` |
| `Catalog/CreateBrand?returnUrl=` | GET | `Views/Brand/CreateBrand.cshtml` | `products.manage` |
| `Catalog/CreateBrand` | POST | JSON (`redirectUrl`) | `products.manage` |
| `Catalog/EditBrand/{id}?returnUrl=` | GET | `Views/Brand/EditBrand.cshtml` (404 when missing) | `products.manage` |
| `Catalog/EditBrand/{id}` | POST | JSON (`redirectUrl`) | `products.manage` |
| `Catalog/SetBrandActive` (`id`, `isActive`) | POST | JSON | `products.manage` |
| `Catalog/BrandLookup` | GET | JSON | signed in + module `PRODUCTS` |

- Module `PRODUCTS` (class-level `[ModuleAccess]`). Every POST is `[ValidateAntiForgeryToken]`; failures are thrown typed exceptions (404 `ResourceNotFoundException`, 409 `ConflictException`, validation errors via `CommonHelpers.ValidateModelState`).

## UI Rules & Conventions
- Views: `Views/Brand/Brands.cshtml`, `CreateBrand.cshtml`, `EditBrand.cshtml` (explicit `View("~/Views/Brand/…")` paths). The old static placeholder list was replaced.
- JS: `wwwroot/js/catalog/brand.js` (form submit + list busy state) on top of `wwwroot/js/sc-ajax.js`; activate/deactivate buttons use the declarative `data-sc-post` + `data-sc-confirm` hooks and reload with a toast.

## Business Rules
- Soft cancel only; the UI never hard-deletes.
- Brand name unique: checked case-insensitively against **all** rows (including cancelled ones, as the DB constraint does) → 409.
- Company-wide (no `fk_store`), so the Platform Super Admin "no store" panel does not apply.

## Dependencies & Related Modules
- `product.md`

## Notes
- Query parameter is `ActiveOnly` (same as Category in code); `ADMIN.md` still says `status`.
- Deactivating a brand does not change products that already use it; it only hides the brand from the lookup used on product forms.
- There is no UI to restore a cancelled brand; its name stays reserved (409).
