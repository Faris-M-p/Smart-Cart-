# Admin Module — User Role & Permissions

## Module Overview
- **Area:** ADMIN (Back Office)
- **Module Name:** User Role & Permissions
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Manages back-office administrative roles (e.g., Admin, Inventory Manager, Cashier) and assigns granular permission matrices (Module.Action permissions) to each role.

## Implemented Features
- **Role Listing:** Table of defined user roles with descriptions and assigned employee counts.
- **Create Role:** Defines new role (`RoleName`, Description).
- **Update Role:** Edits role name and description.
- **Permission Matrix Tree:** Interactive checkbox tree grouped by functional modules to assign or revoke permissions for a role (`UserRolePermissions`).
- **Soft Delete:** Sets `Cancelled = 1`.

## Filters / Search / Sorting
- **Search:** `SearchText` matching RoleName or Description.

## Data
- Entities / DTOs: `UserRole`, `Permission`, `Module`, `UserRolePermission`, `UserRoleViewModel`, `PermissionTreeViewModel`.

## Database Dependencies
- Tables: `UserRoles` (PK `ID_UserRole`), `Permissions` (PK `ID_Permission`), `Modules` (PK `ID_Module`), `UserRolePermissions`.
- Access: EF Core (`EcommerceDbContext`) & `UserRoleRepository`.

## API & Data Access
- Controller: `Controllers/Admin/UserRoleController.cs` (`[Route("Admin/UserRole")]`).
- Interface & Repository: `IUserRoleInterface.cs` / `UserRoleRepository.cs`.
- Helper: `UserRoleHelper.cs`.
- Permissions: `UserRole.View`, `UserRole.Create`, `UserRole.Update`, `UserRole.Delete`, `UserRole.AssignPermissions`.

## UI Rules & Conventions
- Views: `views/Admin/UserRole/Index.cshtml`, `views/Admin/UserRole/Permissions.cshtml`.
- Permission tree grouped by module (Catalog, Purchasing, Orders, Administration) with select-all category toggles.

## Business Rules
- Standard seed creates `Admin` role with all permissions assigned (`05_Seed/UserRolePermissions.sql`).
- Permissions assigned to a role take effect immediately on next HTTP request (re-loaded per request by `AdminAuthMiddleware`).
- Soft delete (`Cancelled = 1`).

## Dependencies & Related Modules
- `ADMIN/modules/auth.md` — Enforces role permissions via `[RequirePermission]`.
- `ADMIN/modules/employee.md` — Assigns roles to employees.
