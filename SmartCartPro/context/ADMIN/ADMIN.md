# Admin

## Purpose

Employee back office for SmartCartPro: catalog, purchasing, stock, POS/sales, customers, reports, company/stores, employees, and roles. Metis admin template.

## Architecture

Controllers under `Controllers/` (domain-grouped, not `Controllers/Admin/` prefix yet). Views under `Views/{Controller}/`. Layout: `_Layout.cshtml` + Metis assets.

**Data access:** EF Core via Services → Repositories. Common dropdowns: `FilterController` / `FilterService` / `FilterRepository`.

## Authentication

- Routes: `/Auth/Login`, ChangePassword, ForgotPassword, ResetPassword (**no Register**)
- Status: **IMPLEMENTED** for employee JWT cookie auth — see [`modules/auth.md`](modules/auth.md)
- Company onboarding is Super Admin only — see [`modules/super-admin.md`](modules/super-admin.md)
- **Platform Super Admin** (`/platform-access` + OTP, licenses, packages): **PLANNED / hold** — do not implement until requested.

## Authorization (target)

Roles/permissions seeded in SQL; enforce via claims + permission checks (`RequirePermission` still incomplete). Do not trust client-only permission flags. Platform Super Admin must use a **separate** policy from company role `SUPER_ADMIN`.

## UI structure

- Sidebar groups: Dashboard; Catalog; Purchase & stock; Sales; Reports; Administration
- List pages: Metis cards/tables; many still use sample rows until services bind real data
- Auth pages: `_AuthLayout`

## Current modules

Documented under `modules/`:

- [`auth.md`](modules/auth.md) — Sign-in / register / password
- [`super-admin.md`](modules/super-admin.md) — Platform Super Admin (**PLANNED / hold**)
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
- [`online-orders.md`](modules/online-orders.md) — Online orders
- [`customers.md`](modules/customers.md) — Customers
- [`reports.md`](modules/reports.md) — Reports
- [`employee.md`](modules/employee.md) — Employees
- [`roles.md`](modules/roles.md) — Roles & permissions
- [`stores.md`](modules/stores.md) — Company & stores
- [`settings.md`](modules/settings.md) — Settings

## Business rules (cross-cutting)

- Soft cancel on master/transaction tables
- Store-scoped ops use `fk_store`
- Company-wide master data shared across stores
- Filter fills use `FilterModels.cs` types only

## Conventions

- Thin controllers; logic in services
- Pack DTOs in shared model files
- Update this index when adding a module file
