# Admin Module — Tax

## Module Overview
- **Area:** ADMIN
- **Module Name:** Tax
- **Current Status:** `IMPLEMENTED` (server + AJAX UI, built on the Category pattern)

## Purpose & Responsibilities
Company-wide tax profiles (code, name, rate %, tax-inclusive flag) that can be assigned to products: list, create, edit, activate/deactivate, and a JSON lookup for product forms.

## Implemented Features
- `Catalog/Taxes` list: search (tax name or code, case-insensitive), status filter (All / Active / Inactive), sortable columns (code, tax, rate, pricing, status), page size + pagination, empty and "no match" states. Rate is shown as `5%`; pricing as "Tax inclusive" / "Tax exclusive".
- `Catalog/CreateTax` and `Catalog/EditTax` full-page forms: code, name, rate % (0–100, up to 4 decimals), "Inclusive" flag, active flag. AJAX save through `SmartCartAjax.submitForm`, field errors under inputs, redirect to the list (or `returnUrl`) with a flash toast.
- `Catalog/SetTaxActive` (POST, `products.manage`) — soft activate/deactivate from the list after a confirm dialog; **no hard delete**.
- `Catalog/TaxLookup` JSON (active taxes incl. rate and inclusive flag) for product forms — unchanged.
- Manage buttons (Add / Edit / Activate / Deactivate) are hidden without `products.manage`.

## Filters / Search / Sorting
- List query string (model `InputTaxList`): `Search`, `ActiveOnly` (`true` / `false`; empty = all), `SortBy` (`taxname` default, `taxcode`, `taxrate`, `isinclusive`, `isactive`), `SortDesc`, `Page`, `PageSize` (default 25, max 100).
- The service normalises the criteria (page/page-size clamp, sort default, search trim) and re-queries the last page when the requested page is past the end.

## Data
- Entity: `Tax` → `taxes` (repository only).
- Models: `Models/CatalogModels.cs` — `InTaxCreateView` / `InTaxUpdateView`, `InputTax{Create,Update,Select,List}`, `OutputTax{Create,Update,Select,List}` (`OutputTaxList.Filter` echoes the criteria for the view). `OutputTaxSelect.ToUpdateView()` drops trailing zeros of the `numeric(7,4)` rate for the edit form.

## Database Dependencies
- Table: `taxes`; `uq_taxes_taxcode` (case-sensitive, includes cancelled rows); `ck_taxes_taxrate` (`taxrate >= 0`); `taxrate NUMERIC(7,4)`.
- `products.fk_tax` references it.

## API & Data Access
- Controller: `CatalogController` (tax region) → `ICatalogService` → `ICatalogRepository` (EF).

| Route | Verb | Result | Permission |
|---|---|---|---|
| `Catalog/Taxes` | GET | `Views/Tax/Taxes.cshtml` | `products.view` |
| `Catalog/CreateTax?returnUrl=` | GET | `Views/Tax/CreateTax.cshtml` | `products.manage` |
| `Catalog/CreateTax` | POST | JSON (`redirectUrl`) | `products.manage` |
| `Catalog/EditTax/{id}?returnUrl=` | GET | `Views/Tax/EditTax.cshtml` (404 when missing) | `products.manage` |
| `Catalog/EditTax/{id}` | POST | JSON (`redirectUrl`) | `products.manage` |
| `Catalog/SetTaxActive` (`id`, `isActive`) | POST | JSON | `products.manage` |
| `Catalog/TaxLookup` | GET | JSON | signed in + module `PRODUCTS` |

- Module `PRODUCTS` (class-level `[ModuleAccess]`). Every POST is `[ValidateAntiForgeryToken]`; failures are thrown typed exceptions (404 `ResourceNotFoundException`, 409 `ConflictException`, validation errors via `CommonHelpers.ValidateModelState`).

## UI Rules & Conventions
- Views: `Views/Tax/Taxes.cshtml`, `CreateTax.cshtml`, `EditTax.cshtml` (explicit `View("~/Views/Tax/…")` paths).
- JS: `wwwroot/js/catalog/tax.js` (form submit + list busy state) on top of `wwwroot/js/sc-ajax.js`; activate/deactivate buttons use the declarative `data-sc-post` + `data-sc-confirm` hooks and reload with a toast.
- The rate input is a `type="number"` field (`min 0`, `max 100`, `step 0.0001`) inside a `%` input group; the create form starts empty so a rate is never saved by accident.

## Business Rules
- Soft cancel only; the UI never hard-deletes.
- Tax code unique: checked case-insensitively against **all** rows (including cancelled ones, as the DB constraint does) → 409.
- Tax name unique among non-cancelled taxes (application rule, not a DB constraint) → 409.
- Rate must be 0–100 (view validation); the repository rounds it to 4 decimals (away from zero) before saving.
- Company-wide (no `fk_store`), so the Platform Super Admin "no store" panel does not apply.

## Dependencies & Related Modules
- `product.md`

## Notes
- Query parameter is `ActiveOnly` (same as Category in code); `ADMIN.md` still says `status`.
- Changing a tax rate immediately affects every product that references the tax; deactivating a tax only hides it from the lookup used on product forms.
- Decimal parsing of the posted rate follows the server culture (no request-localization is configured in `Program.cs`); a decimal-comma server culture would need invariant request localization.
- There is no UI to restore a cancelled tax; its code stays reserved (409).
- Sidebar entry (Catalog → Taxes, `Catalog/Taxes`, `products.view`) must be added to `_Layout.cshtml` by the layout owner.
