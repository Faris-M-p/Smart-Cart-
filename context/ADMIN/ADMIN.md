# Admin

## Purpose

Employee back office for catalog, purchasing, stock, customer-order fulfillment, employees, and role permissions. Mantis admin template.

## Architecture

`Controllers/Admin/*`, `views/Admin/*`, layout `_AdminLayout.cshtml`. Sidebar groups: Dashboard; Catalog (Category, SubCategory, Brand, Products, SKUs, Variants, Variant values); Purchase (Supplier, Purchase, Inventory); Orders; Administration (Employees, User Roles).

**Catalog / purchase / stock / employees / roles / admin login: EF Core.**  
**Orders: Dapper + SPs.**

## Authentication

- `/admin/login` (`AdminAuthController`)
- JWT cookie; middleware on `/admin` and `/api/admin`
- Inactive employee or missing role → forbidden
- Seeded dev user is documented in `Database/05_Seed/AdminUsers.sql` (username `admin`)

## Authorization

`[RequirePermission("Module.Action")]` on actions. Codes seeded in `Permissions.sql`. Sidebar uses `data-permission`. JS hides buttons. Server re-loads permissions from the role on each protected action.

`Orders.Create` is seeded; **there is no admin create-order endpoint.**

Billing permissions are seeded; **no Billing UI.**

## UI structure

List pages: search + pagination + modal create/edit + soft delete reason. Images via `CommonImageService`. Empty states and toasts. Permission tree on User Role → Permissions page.

Dashboard view is still Mantis sample metrics (not live SmartCart stats).

## Current modules

See `modules/`: AUTH, DASHBOARD, CATEGORIES, SUBCATEGORIES, BRANDS, PRODUCTS, PRODUCT_VARIANTS, VARIANTS, VARIANT_VALUES, SUPPLIERS, PURCHASES, STOCK, ORDERS, EMPLOYEES, USER_ROLES.

## Business rules (cross-cutting)

- Soft delete (`Cancelled`)
- `SellOnline` default off on product and SKU
- Stock is SKU-level; purchases create batches
- Order workflow: Placed → Confirmed → (optional Shipped) → Delivered; cancel before ship
- Do not add a Ship **module**; Shipped is a status API

## Conventions

- Route prefix `Admin/{Controller}`
- POST `Get{Entity}List` + GET `GetById/{id}` + POST Create/Update/Delete
- `ApiResponse<T>` wrapper on many admin JSON results
- Helpers `*Helper.NormalizeInput` for paging
