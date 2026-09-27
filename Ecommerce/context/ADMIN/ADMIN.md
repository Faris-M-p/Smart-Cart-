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

Specific Admin back-office modules documented under `modules/`:
- [`auth.md`](file:///d:/Faris/Work%20Area/Smart-Cart-/Ecommerce/context/ADMIN/modules/auth.md) — Admin authentication, JWT cookies & session
- [`dashboard.md`](file:///d:/Faris/Work%20Area/Smart-Cart-/Ecommerce/context/ADMIN/modules/dashboard.md) — Admin dashboard & key metrics
- [`category.md`](file:///d:/Faris/Work%20Area/Smart-Cart-/Ecommerce/context/ADMIN/modules/category.md) — Category catalog management
- [`subcategory.md`](file:///d:/Faris/Work%20Area/Smart-Cart-/Ecommerce/context/ADMIN/modules/subcategory.md) — SubCategory catalog management
- [`brand.md`](file:///d:/Faris/Work%20Area/Smart-Cart-/Ecommerce/context/ADMIN/modules/brand.md) — Brand catalog management
- [`variant.md`](file:///d:/Faris/Work%20Area/Smart-Cart-/Ecommerce/context/ADMIN/modules/variant.md) — Variant Axes management (Size, Color, etc.)
- [`variant-value.md`](file:///d:/Faris/Work%20Area/Smart-Cart-/Ecommerce/context/ADMIN/modules/variant-value.md) — Variant Values management
- [`product.md`](file:///d:/Faris/Work%20Area/Smart-Cart-/Ecommerce/context/ADMIN/modules/product.md) — Product catalog management & ProductMedia
- [`product-variant.md`](file:///d:/Faris/Work%20Area/Smart-Cart-/Ecommerce/context/ADMIN/modules/product-variant.md) — Product SKUs, attributes & SkuMedia
- [`supplier.md`](file:///d:/Faris/Work%20Area/Smart-Cart-/Ecommerce/context/ADMIN/modules/supplier.md) — Supplier management & India location dropdowns
- [`purchase.md`](file:///d:/Faris/Work%20Area/Smart-Cart-/Ecommerce/context/ADMIN/modules/purchase.md) — Purchase intake orders & SKU batch creation
- [`inventory.md`](file:///d:/Faris/Work%20Area/Smart-Cart-/Ecommerce/context/ADMIN/modules/inventory.md) — Stock levels & manual inventory adjustments
- [`order.md`](file:///d:/Faris/Work%20Area/Smart-Cart-/Ecommerce/context/ADMIN/modules/order.md) — Order fulfillment, confirm, ship, deliver & cancel
- [`employee.md`](file:///d:/Faris/Work%20Area/Smart-Cart-/Ecommerce/context/ADMIN/modules/employee.md) — Employee / AdminUser management
- [`user-role.md`](file:///d:/Faris/Work%20Area/Smart-Cart-/Ecommerce/context/ADMIN/modules/user-role.md) — User Roles & Permissions matrix tree

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
