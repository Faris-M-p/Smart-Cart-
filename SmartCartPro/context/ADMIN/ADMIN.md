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

Roles/permissions seeded in SQL; enforce via claims + permission checks (`RequirePermission` still incomplete). Do not trust client-only permission flags. Platform Super Admin must use a **separate** policy from company role `SUPER_ADMIN`.

## UI structure

- Sidebar groups: Dashboard; Catalog; Purchase & stock; Sales; Reports; Administration
- Sidebar chrome (`_Layout` + `wwwroot/css/admin-shell.css` + `wwwroot/js/admin-sidebar.js`): desktop expand/collapse (icon rail, persisted), overlay drawer below 992px, independent sidebar scroll
- List pages: Metis cards/tables; many still use sample rows until services bind real data
- Auth pages: `_AuthLayout`

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
- [`product.md`](modules/product.md) — Products
- [`variant.md`](modules/variant.md) — SKU / variants
- [`supplier.md`](modules/supplier.md) — Suppliers
- [`purchase.md`](modules/purchase.md) — Purchases
- [`inventory.md`](modules/inventory.md) — Stock
- [`pos.md`](modules/pos.md) — POS billing
- [`invoices.md`](modules/invoices.md) — Sales invoices
- [`returns.md`](modules/returns.md) — Sales returns
- [`online-orders.md`](modules/online-orders.md) — Online orders (Phase 7: full status flow + stock)
- [`customers.md`](modules/customers.md) — Customers
- [`reviews.md`](modules/reviews.md) — Product reviews (Phase 7: admin moderation)
- [`cart-wishlist.md`](modules/cart-wishlist.md) — Cart & wishlist (Phase 7: repo+service+JSON controller)
- [`reports.md`](modules/reports.md) — Reports (Phase 7: SalesByDate, StockValuation, LowStock, TaxSummary, TopSelling)
- [`employee.md`](modules/employee.md) — Employees
- [`roles.md`](modules/roles.md) — Roles & permissions
- [`stores.md`](modules/stores.md) — Company & stores
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
