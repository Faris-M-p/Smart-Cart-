# Admin Module — Roles & permissions

## Module Overview
- **Area:** ADMIN
- **Module Name:** Roles
- **Current Status:** `IMPLEMENTED` (server + AJAX UI). Written without a build or a browser run; the first build and click-through still has to confirm it (see Notes).

## Purpose & Responsibilities
Maintain roles and the permission set each role grants. The company admin defines what each kind of employee can see and do; the permission catalog itself (modules and permissions) comes from the seed and is read-only here.

## Implemented Features
- `Role/Index` list: search, status filter, sorting, paging, employee and permission counts, System badge, empty state.
- `Role/Create`: code, name, description, active flag and the **permission matrix** (one card per module, a checkbox per permission, module toggle, counter, Select all / Clear all). Ticking an action permission also ticks the module's `*.view` permission (UI convenience only).
- `Role/Edit`: name, description and permissions. The code never changes. A **locked** role (the built-in `SUPER_ADMIN` seen by a non-platform user) opens read-only.
- `Role/SetActive` (POST): activate / deactivate with confirm.
- `Role/Delete` (POST): soft delete with confirm.
- In-use guard: a role that employees still hold can be neither deactivated nor deleted; the buttons are disabled with the reason and the server refuses with a 409.

## Routes & Permissions
Every action requires module `USERS` and permission `users.manage` (class-level `[ModuleAccess("USERS")]` + `[RequirePermission("users.manage")]`). The platform Super Admin bypasses both.

| Method | Route | Action | Notes |
|---|---|---|---|
| GET | `/Role` | `Index` | query: `Search`, `ActiveOnly`, `SortBy`, `SortDesc`, `Page`, `PageSize` |
| GET / POST | `/Role/Create` | `Create` | POST returns `{ id, redirectUrl }` |
| GET / POST | `/Role/Edit/{id}` | `Edit` | POST returns `{ id, redirectUrl }` |
| POST | `/Role/SetActive` | `SetActive` | `id`, `isActive` |
| POST | `/Role/Delete` | `Delete` | `id` |

Previously the write actions required the `PlatformSuperAdmin` policy. The module permission replaces it so the company admin can maintain roles; the SUPER_ADMIN definition stays platform-only (see Business Rules).

## Filters / Search / Sorting
- Search (case-insensitive contains): code, name, description.
- Filter: status (all / active / inactive).
- Sort: `rolename` (default), `rolecode`, `usercount`, `isactive`; page size 10 / 25 / 50 / 100.

## Data
- Entities (repository only): `Role`, `RolePermission`; read-only `Permission`, `Module`, `UserRole`, `User`.
- Models: `Models/RoleModels.cs` — regions Permission Select, Role List, Select, Create, Update, Status, Delete. `InRoleCreateView` / `InRoleUpdateView` carry the validation.
- Permission catalog in the seed (`02_Seed/002_DefaultPermissions.sql`): `dashboard.view`; `products.view`, `products.manage`; `inventory.view`, `inventory.adjust`; `purchases.view`, `purchases.manage`; `pos.sell`, `pos.return`, `pos.cancel`; `customers.view`, `customers.manage`; `reports.view`; `settings.manage`; `users.manage` (modules `DASHBOARD`, `PRODUCTS`, `INVENTORY`, `PURCHASES`, `POS`, `CUSTOMERS`, `REPORTS`, `SETTINGS`, `USERS`). The matrix is built from the database, so new seeded permissions appear without code changes.

## Database Dependencies
- Tables: `roles`, `modules`, `permissions`, `role_permissions`, `user_roles`.
- `uq_roles_rolecode` also covers cancelled rows, so the code of a deleted role stays reserved.
- Seeded system roles (`SUPER_ADMIN`, `CASHIER`, …) have `is_system = true`.

## API & Data Access
- `RoleController` → `IRoleService` / `RoleService` → `IRoleRepository` / `RoleRepository` (EF only).
- Create writes the role and its `role_permissions` in one `SaveChanges`; update applies only the difference.

## UI Rules & Conventions
- Views: `Views/Role/Index.cshtml`, `Create.cshtml`, `Edit.cshtml`, `_PermissionMatrix.cshtml`.
- JS: `wwwroot/js/admin/role.js` on top of `sc-ajax.js` (matrix counters and toggles, role code upper-casing, list filter busy state).
- Error keys match input names: `RoleCode`, `RoleName`, `Description`, `PermissionIds`.

## Business Rules
- Permissions can only come from the active seeded catalog; any other id is refused with a `PermissionIds` field error. This holds for every actor, platform admin included.
- Role code: letters, digits, underscore, starts with a letter, stored upper-case, unique including deleted roles. Role name is unique among non-deleted roles.
- System roles cannot be deactivated or deleted. Their name, description and permissions can be edited, except `SUPER_ADMIN`, which only the platform Super Admin may edit (everyone else gets a read-only view and a 403 on POST).
- In-use guard counts employees that are not cancelled (inactive employees count). Deleting is a soft cancel and keeps `role_permissions`.
- Audit (`IAuditService`, entity `Role`): Create, Update, Activate, Deactivate, Delete.

## Dependencies & Related Modules
- `employee.md` (role picker, in-use counts), `auth.md` (permissions are read per request by `RequirePermission`), `audit.md`.

## Notes
- Roles and permissions are global in the current schema (no company column). In a multi-company database an edit of a shared role applies to every company.
- Nothing stops an administrator from removing `users.manage` from the role they hold (self-lockout); the platform Super Admin can repair it.
- Not built or run when this was written: compile and click-through checks are still open.
