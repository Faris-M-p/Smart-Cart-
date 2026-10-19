# Admin Module — Unit of measure

## Module Overview
- **Area:** ADMIN
- **Module Name:** Unit (unit of measure)
- **Current Status:** `IMPLEMENTED` (server + AJAX UI, built on the Category pattern)

## Purpose & Responsibilities
Company-wide units of measure (pcs, kg, box …): list, create, edit, activate/deactivate, and a JSON lookup for product forms.

## Implemented Features
- `Catalog/Units` list: search (unit name or code, case-insensitive), status filter (All / Active / Inactive), sortable columns (code, unit, status), page size + pagination, empty and "no match" states.
- `Catalog/CreateUnit` and `Catalog/EditUnit` full-page forms: code (short symbol), name, active flag. AJAX save through `SmartCartAjax.submitForm`, field errors under inputs, redirect to the list (or `returnUrl`) with a flash toast.
- `Catalog/SetUnitActive` (POST, `products.manage`) — soft activate/deactivate from the list after a confirm dialog; **no hard delete**.
- `Catalog/UnitLookup` JSON (active units) for product forms — unchanged.
- Manage buttons (Add / Edit / Activate / Deactivate) are hidden without `products.manage`.

## Filters / Search / Sorting
- List query string (model `InputUnitList`): `Search`, `ActiveOnly` (`true` / `false`; empty = all), `SortBy` (`unitname` default, `unitcode`, `isactive`), `SortDesc`, `Page`, `PageSize` (default 25, max 100).
- The service normalises the criteria (page/page-size clamp, sort default, search trim) and re-queries the last page when the requested page is past the end.

## Data
- Entity: `Unit` → `units` (repository only).
- Models: `Models/CatalogModels.cs` — `InUnitCreateView` / `InUnitUpdateView`, `InputUnit{Create,Update,Select,List}`, `OutputUnit{Create,Update,Select,List}` (`OutputUnitList.Filter` echoes the criteria for the view).

## Database Dependencies
- Table: `units`; `uq_units_unitcode` (case-sensitive, includes cancelled rows).
- `products.fk_unit` references it.

## API & Data Access
- Controller: `CatalogController` (unit region) → `ICatalogService` → `ICatalogRepository` (EF).

| Route | Verb | Result | Permission |
|---|---|---|---|
| `Catalog/Units` | GET | `Views/Unit/Units.cshtml` | `products.view` |
| `Catalog/CreateUnit?returnUrl=` | GET | `Views/Unit/CreateUnit.cshtml` | `products.manage` |
| `Catalog/CreateUnit` | POST | JSON (`redirectUrl`) | `products.manage` |
| `Catalog/EditUnit/{id}?returnUrl=` | GET | `Views/Unit/EditUnit.cshtml` (404 when missing) | `products.manage` |
| `Catalog/EditUnit/{id}` | POST | JSON (`redirectUrl`) | `products.manage` |
| `Catalog/SetUnitActive` (`id`, `isActive`) | POST | JSON | `products.manage` |
| `Catalog/UnitLookup` | GET | JSON | signed in + module `PRODUCTS` |

- Module `PRODUCTS` (class-level `[ModuleAccess]`). Every POST is `[ValidateAntiForgeryToken]`; failures are thrown typed exceptions (404 `ResourceNotFoundException`, 409 `ConflictException`, validation errors via `CommonHelpers.ValidateModelState`).

## UI Rules & Conventions
- Views: `Views/Unit/Units.cshtml`, `CreateUnit.cshtml`, `EditUnit.cshtml` (explicit `View("~/Views/Unit/…")` paths).
- JS: `wwwroot/js/catalog/unit.js` (form submit + list busy state) on top of `wwwroot/js/sc-ajax.js`; activate/deactivate buttons use the declarative `data-sc-post` + `data-sc-confirm` hooks and reload with a toast.

## Business Rules
- Soft cancel only; the UI never hard-deletes.
- Unit code unique: checked case-insensitively against **all** rows (including cancelled ones, as the DB constraint does) → 409.
- Unit name unique among non-cancelled units (application rule, not a DB constraint) → 409.
- Company-wide (no `fk_store`), so the Platform Super Admin "no store" panel does not apply.

## Dependencies & Related Modules
- `product.md`

## Notes
- Query parameter is `ActiveOnly` (same as Category in code); `ADMIN.md` still says `status`.
- Deactivating a unit does not change products that already use it; it only hides the unit from the lookup used on product forms.
- There is no UI to restore a cancelled unit; its code stays reserved (409).
- Sidebar entry (Catalog → Units, `Catalog/Units`, `products.view`) must be added to `_Layout.cshtml` by the layout owner.
