# Admin

## Purpose

Employee back office for SmartCartPro: catalog, purchasing, stock, POS/sales, customers, reports, company/stores, employees, and roles. Metis admin template.

## Architecture

Controllers under `Controllers/` (domain-grouped, not `Controllers/Admin/` prefix yet). Views under `Views/{Controller}/`. Layout: `_Layout.cshtml` + Metis assets.

**Data access:** EF Core via Services → Repositories. Common dropdowns: `FilterController` / `FilterService` / `FilterRepository`.

## Authentication

- Routes: `/Auth/Login`, ChangePassword, ForgotPassword, ResetPassword (**no Register**)
- Status: **IMPLEMENTED** for employee JWT cookie auth — see [`modules/auth.md`](modules/auth.md)
- Company/Store: normal MVC `CompanyController` / `StoreController` — Super Admin creates stores; admin edits — see [`modules/company.md`](modules/company.md)
- **No** SuperAdmin controller and **no** `/api/admin` company APIs

## Authorization (target)

Roles/permissions seeded in SQL; enforce via claims + permission checks (`RequirePermission` still incomplete). Do not trust client-only permission flags. Platform Super Admin must use a **separate** policy from company role `ADMIN`.

## UI structure

- Sidebar groups: Dashboard; Catalog; Purchase & stock; Sales; Reports; Administration
- Sidebar chrome (`_Layout` + `wwwroot/css/admin-shell.css` + `wwwroot/js/admin-sidebar.js`): desktop expand/collapse (icon rail, persisted), overlay drawer below 992px, independent sidebar scroll
- Menu items are gated with `Access.CanAsync` / `ModuleEnabledAsync` and point at real module routes (Employee/Role/Setting, Product/Index, Supplier, Purchase, Inventory/Stock, OnlineOrder, Audit, License, Module). The header host a `StoreSwitcher` view component (no-op until the Company/Stores work fills it). There is no header search.
- Auth pages: `_AuthLayout`

## Front-end conventions (every admin screen)

Reference implementation: **Category** (`Views/Category/*`, `wwwroot/js/catalog/category.js`, `CatalogController` Category actions). Copy its structure; do not invent new patterns.

### Files and folders
| What | Where |
|------|-------|
| Views | `Views/{Module}/…` — one **top-level folder per master/module** (`Category/`, `Brand/`, `Supplier/`, …). No shared `Views/Catalog/` folder and **no view-location expander**. When one controller serves several modules (`CatalogController`), return `View("~/Views/{Module}/{Page}.cshtml", model)`. |
| Page JS | `wwwroot/js/{area}/{module}.js`, loaded from `@section Scripts` with `asp-append-version="true"`. No business logic in inline `<script>`. |
| Page CSS | Only when Bootstrap/Metis utilities are not enough: `wwwroot/css/{module}.css` via `@section Styles`. |
| Shared (do not fork) | `_Layout`, `_AuthLayout`, `wwwroot/js/sc-ajax.js` (`window.SmartCartAjax`), partials `_PageHeader`, `_StatusBadge`, `_ConfirmModal`, `_Pager`. `_Layout` already renders a page-wide antiforgery token and a `Styles` / `Scripts` section. |

### Page patterns
- **List**: breadcrumb → `_PageHeader` (title + primary action, hidden without the manage permission) → GET filter form (search, status, other filters; `data-sc-auto-submit` on selects) → table with sortable headers → empty state → `_Pager`. State lives in the query string (`search`, `status`, `sortBy`, `sortDesc`, `page`, `pageSize`). Row actions: Edit link, and activate/deactivate (or cancel) through `data-sc-post` + confirm. **No hard delete.**
- **Form (create/edit)**: full page, fields exactly as the `In…View`, antiforgery token, `<form novalidate>`, `[data-sc-form-summary]` alert inside the form, `asp-validation-for` spans (the AJAX helper fills them from `validationErrors`), Save disabled while posting, Cancel back to the list (honour a safe `returnUrl`).
- **Detail / document view**: read-only header card + lines table + action buttons gated by status and permission (receive, cancel, print…).
- **Document entry (purchase, sale, transfer…)**: header form + dynamic line table; line product picker uses `SmartCartAjax.typeahead`; the whole document is sent with `SmartCartAjax.postJson` (server validates again).
- Every table is `table-responsive`; every page works at tablet width; loading and empty states are visible; all output is Razor-encoded (never `Html.Raw` on user data; build DOM with `.text()` or `SmartCartAjax.escapeHtml`).

### AJAX contract (server ⇄ `sc-ajax.js`)
- Mutating POST actions are `[ValidateAntiForgeryToken]`, call `CommonHelpers.ValidateModelState(ModelState)` first, then the service, then `CommonHelpers.SuccessResponse(message, data)`; **no TempData/Redirect from a POST**. `data.redirectUrl` (built with `Url.Action`) makes `submitForm` navigate after showing a flash toast.
- Failures are thrown as typed `AppException`s from the service/controller (`ValidationException` with `FieldErrors`, `ResourceNotFoundException` 404, `ConflictException` 409 …). The global handler returns the JSON envelope for any request with `X-Requested-With: XMLHttpRequest` (all `SmartCartAjax` calls send it). 401 → login redirect, 403 → toast.
- Map `In…View` → `Input…` with an explicit named variable in the controller (see `model-naming.mdc`); services return `Output…`.
- Read endpoints used by pickers return `CommonResponseModel.Success` with `data` = array (or a small typed list) so `typeahead`/`fillSelect` can use `res.data`.

### `SmartCartAjax` quick reference
`submitForm($form, {stayOnSuccess,onSuccess,onError})` · `post(url, fields, {silent,$form,onSuccess})` · `postJson(url, obj, {…})` · `getJson(url, params)` · `confirm({title,body,confirmText,confirmClass,onConfirm})` · `typeahead(input,{source,label,sublabel,onSelect,minChars})` · `fillSelect($sel, items, {valueField,textField,placeholder,selected})` · `toast/flash/reloadWithToast` · `setBusy` · `formToObject` · `fmt.money/number/date/dateTime` · `debounce` · `escapeHtml`.
Declarative: `<button data-sc-post="@Url.Action(…)" data-sc-param-id="@x.Id" data-sc-param-isactive="false" data-sc-confirm="…" data-sc-confirm-ok="Deactivate" data-sc-after="reload|none|redirect|remove-row">` and `<select data-sc-auto-submit>`.

### Permissions and modules in views
`_ViewImports` injects `Access` (`ICurrentUserAccess`): `@if (await Access.CanAsync("products.manage")) { … }`, `await Access.ModuleEnabledAsync("POS")`. This only shapes the UI; enforcement stays in `[RequirePermission]` / `[ModuleAccess]` on the action. Platform Super Admin passes every check but has **no store** (`fk_store = 0`), so store-scoped screens must show a friendly "select a store / sign in as a store user" state instead of failing.

### Permission code catalog (SQL seed is the source of truth)
Seeded in `Database-Postgres/02_Seed/002_DefaultPermissions.sql` (+ `V006` for `pos.cancel`): `dashboard.view`, `products.view`, `products.manage`, `inventory.view`, `inventory.adjust`, `purchases.view`, `purchases.manage`, `pos.sell`, `pos.return`, `pos.cancel`, `customers.view`, `customers.manage`, `reports.view`, `settings.manage`, `users.manage`. Modules: `DASHBOARD`, `PRODUCTS`, `INVENTORY`, `PURCHASES`, `POS`, `CUSTOMERS`, `REPORTS`, `SETTINGS`, `USERS`.
Controllers must only reference codes from this catalog. Previously unseeded codes were replaced: `purchasing.view/manage` → `purchases.view/manage`, `inventory.manage` → `inventory.adjust`, `pos.view`/`pos.manage` → `pos.sell` (POS, invoices, sessions) and `customers.view`/`customers.manage` (customers).

### Cross-cutting helpers (use them, do not re-implement)
- **Any-of authorization:** `[RequirePermission("customers.view", "pos.sell")]` passes when the user holds *any* listed code; `[ModuleAccess("CUSTOMERS", "POS")]` passes when *any* listed module is enabled. Two separate `[RequirePermission]` attributes on one action mean *all of*. Platform Super Admin bypasses both.
- **Timestamps are stored in UTC.** Render a timestamp that has a time of day as `<time data-sc-utc="@TimeDisplay.Iso(x.CreatedAt)" data-sc-format="datetime">@x.CreatedAt.ToString("dd MMM yyyy HH:mm")</time>` (`format` = `datetime` | `date` | `time`); `sc-ajax.js` converts it to the browser's time zone on load (call `SmartCartAjax.localizeTimes(container)` after injecting rows with JS). `SmartCartAjax.fmt.date/dateTime/time` already treat server strings as UTC. Pure calendar dates (purchase date, filters) are shown as-is. Server-side "today" boundaries use the company setting `locale.timezone`.
- **Transactions:** `IStockRepository.ApplyMovementAsync` and `IDocumentNumberRepository.NextAsync` join the caller's open transaction (they only start their own when none is open), so a repository that owns a transaction can call them. Never call `BeginTransactionAsync` inside a method that may itself run inside a transaction.
- **Icons:** the complete Bootstrap Icons 1.13.1 glyph set is available (`wwwroot/css/bootstrap-icons-glyphs.css`, linked from `_MetisAssets`), so any `bi-*` name from <https://icons.getbootstrap.com> renders.
- **Charts:** the Metis chart bundle is an ES module without a global; use plain HTML/SVG/CSS charts (see `wwwroot/js/sc-charts.js` once the dashboard module ships it).

### Razor pitfalls (build breaks otherwise)
- Never put C# expressions inside the attribute area of tag-helper elements (`<option … @(c ? "selected" : null)>` → RZ1031). Use `selected="@flag"` boolean attributes or if/else blocks.
- `@model` type must match what the action passes; use `@using` for enums/helpers.
- Use `Url.Action`/`asp-action`, never hard-coded URLs. Use `data-*` attributes to pass URLs/ids to JS.

## Current modules

Documented under `modules/`:

- [`auth.md`](modules/auth.md) — Sign-in / register / password
- [`super-admin.md`](modules/super-admin.md) — Platform Super Admin role/claim (no separate controller)
- [`company.md`](modules/company.md) — Company master + store/setup APIs
- [`dashboard.md`](modules/dashboard.md) — Home dashboard
- [`filter.md`](modules/filter.md) — Common dropdown fill API
- [`category.md`](modules/category.md) — Categories
- [`subcategory.md`](modules/subcategory.md) — Subcategories (via filter + catalog)
- [`brand.md`](modules/brand.md) — Brands
- [`unit.md`](modules/unit.md) — Units of measure
- [`tax.md`](modules/tax.md) — Tax profiles
- [`product.md`](modules/product.md) — Products
- [`variant.md`](modules/variant.md) — SKU / variants
- [`supplier.md`](modules/supplier.md) — Suppliers
- [`purchase.md`](modules/purchase.md) — Purchases
- [`inventory.md`](modules/inventory.md) — Stock
- [`pos.md`](modules/pos.md) — POS billing
- [`invoices.md`](modules/invoices.md) — Sales invoices
- [`returns.md`](modules/returns.md) — Sales returns (list, create against invoice, details)
- [`online-orders.md`](modules/online-orders.md) — Online orders (Phase 7: full status flow + stock)
- [`customers.md`](modules/customers.md) — Customers
- [`reviews.md`](modules/reviews.md) — Product reviews (Phase 7: admin moderation)
- [`cart-wishlist.md`](modules/cart-wishlist.md) — Cart & wishlist (Phase 7: repo+service+JSON controller)
- [`reports.md`](modules/reports.md) — Reports (sales, products, tax, purchases, customers, stock valuation + CSV)
- [`employee.md`](modules/employee.md) — Employees
- [`roles.md`](modules/roles.md) — Roles & permissions
- [`stores.md`](modules/stores.md) — Company & stores
- [`license.md`](modules/license.md) — Installation license (platform Super Admin)
- [`modules.md`](modules/modules.md) — Enabled modules / presets (platform Super Admin)
- [`settings.md`](modules/settings.md) — Settings (Phase 7: known keys via SettingController)
- [`audit.md`](modules/audit.md) — Audit log viewer (Phase 7)

## Business rules (cross-cutting)

- Soft cancel on master/transaction tables
- Store-scoped ops use `fk_store`
- Company-wide master data shared across stores
- Filter fills use `FilterModels.cs` types only

## Conventions

- Thin controllers; logic in services
- Pack DTOs in shared model files
- Update this index when adding a module file
