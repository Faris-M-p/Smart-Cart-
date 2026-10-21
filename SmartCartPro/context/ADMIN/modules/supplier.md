# Admin Module — Supplier

## Module Overview
- **Area:** ADMIN
- **Module Name:** Supplier
- **Current Status:** `IMPLEMENTED` (server + AJAX UI; follows the Category reference pattern)

## Purpose & Responsibilities
Company-wide vendor master used when raising purchase orders and receiving goods: list, create, edit, activate/deactivate and a lookup for pickers.
Suppliers are **not** store-scoped, so they work for every store user and for platform administrators (who have no store).

## Implemented Features
- `Supplier/Index` list: search, status filter (all / active / inactive), sortable headers (code, name, city, status), page-size selector, pager, empty states (nothing yet vs. no match), activate/deactivate with confirm → POST → reload + toast.
- `Supplier/Create` and `Supplier/Edit` full-page forms (AJAX save through `SmartCartAjax.submitForm`, field errors under the inputs, summary alert inside the form, redirect to the list or to a local `returnUrl`).
- `Supplier/SetActive` (POST, `purchases.manage`) — soft activate/deactivate from the list; **no hard delete**.
- `Supplier/Lookup` (GET) — JSON array of active suppliers for vendor pickers.
- Manage buttons/links are hidden without `purchases.manage`.

## Filters / Search / Sorting
- Query string: `search`, `activeOnly` (`true` = active, `false` = inactive, empty = all), `sortBy` (`suppliercode|suppliername|city|isactive`), `sortDesc`, `page`, `pageSize` (10 / 25 / 50 / 100).
- Search is case-insensitive "contains" on name, code, contact person, phone, email and city (max 100 characters).
- `SupplierService.ListSuppliersAsync` normalises every value (unknown sort → name, unknown page size → 25) and falls back to the last page when a stale link points past the end; the normalised filter is echoed back as `OutputSupplierList.Filter`.

## Data
- Entity: `Supplier` → `suppliers` (repository only).
- Models: `Models/SupplierModels.cs` — `InSupplierCreateView` / `InSupplierUpdateView`, `InputSupplier{Create|Update|Select|List|SetActive}`, `OutputSupplier{Create|Update|Select|List|SetActive}`. Validation attributes live only on the `In…View` classes.

## Database Dependencies
- Table: `suppliers` (`UNIQUE(suppliername)` across **all** rows, soft-cancelled ones included).
- Referenced by `purchases.fk_supplier`.

## API & Data Access
- Controller: `SupplierController` → `ISupplierService` → `ISupplierRepository` (module `PURCHASES`).
- Routes and permissions:
  - `GET Supplier/Index` — `purchases.view`
  - `GET Supplier/Lookup` — `purchases.view` (JSON array, not the envelope)
  - `GET|POST Supplier/Create`, `GET|POST Supplier/Edit/{id}`, `POST Supplier/SetActive` — `purchases.manage`
- POST contract: `[ValidateAntiForgeryToken]` → `CommonHelpers.ValidateModelState` → explicit `InputSupplier…` variable → service → `CommonHelpers.SuccessResponse(message, { id, redirectUrl })`. Failures are typed exceptions (`ConflictException` 409, `ResourceNotFoundException` 404, `ValidationException` 422) rendered as JSON by `AppExceptionHandler`.

## UI Rules & Conventions
- Views: `Views/Supplier/Index.cshtml`, `Create.cshtml`, `Edit.cshtml`. The old placeholder `Views/Inventory/Suppliers.cshtml` was removed.
- JS: `wwwroot/js/purchasing/supplier.js` on top of `wwwroot/js/sc-ajax.js` (form submit, filter busy state). Activate/deactivate is declarative (`data-sc-post` + `data-sc-confirm`, needs `_ConfirmModal`).
- Breadcrumb group label is "Purchase & stock" (same as the sidebar section). Follow `ADMIN.md` → *Front-end conventions*.

## Business Rules
- Soft cancel only; company-wide (no `fk_store`).
- Supplier name must be unique, compared case-insensitively and including cancelled rows (the database constraint is case-sensitive and covers cancelled rows) → 409 `A supplier with this name already exists.`; a database unique violation caused by a race is mapped to the same 409.
- Inactive suppliers stay on existing purchase documents and in list filters, but cannot be chosen on a new purchase order (create/edit re-checks on the server).
- Text inputs are trimmed; empty optional values are stored as `NULL`.

## Dependencies & Related Modules
- `purchase.md` (purchase orders pick suppliers from `ISupplierService.GetSupplierLookupAsync`), `filter.md`

## Notes
- The sidebar entry should point to `Supplier/Index` (label "Suppliers", icon `bi-truck`, permission `purchases.view`, module `PURCHASES`). `_Layout.cshtml` still links to the removed `Inventory/Suppliers` action until the orchestrator updates it.
- `Supplier/Lookup` now requires `purchases.view` (it used the non-seeded `purchasing.view` code before).
