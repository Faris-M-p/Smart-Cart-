# Admin Module — Auth

## Module Overview
- **Area:** ADMIN (Back Office)
- **Module Name:** Auth
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Provides admin employee authentication, JWT cookie generation, session management, identity verification, permission matrix loading, and logout.

## Implemented Features
- **Admin Login:** Validates employee username & password via EF `AdminUsers`, generates JWT bearer token stored in HttpOnly cookie (`AdminAuthHelper.TokenCookieName`).
- **Session Middleware:** `AdminAuthMiddleware` intercepting `/admin` and `/api/admin` requests to enforce employee authentication.
- **Permission Enforcement:** `RequirePermissionAttribute` fetching current employee permissions from DB per request.
- **Current User API:** `GET /api/admin/auth/me` returning current logged-in employee profile and role information.
- **Logout:** Clears admin cookie and redirects to `/admin/login`.

## Filters / Search / Sorting
- N/A.

## Data
- Entities / DTOs: `AdminUser`, `AdminLoginModel`, `AdminUserViewModel`, `ApiResponse<T>`.

## Database Dependencies
- Tables: `AdminUsers`, `UserRoles`, `UserRolePermissions`, `Permissions`, `Modules`.
- Access: EF Core (`EcommerceDbContext`) & `AdminAuthRepository`.

## API & Data Access
- Controller: `Controllers/Admin/AdminAuthController.cs` (`[Route("api/admin/auth")]`).
- Interface & Repository: `IAdminAuthInterface.cs` / `AdminAuthRepository.cs`.

## UI Rules & Conventions
- Login View: `views/Admin/Login/Index.cshtml` using `_AdminAuthLayout.cshtml`.
- Error messages displayed via AJAX toast or inline alert.

## Business Rules
- Inactive employees (`IsActive = 0`) or soft-deleted employees (`Cancelled = 1`) cannot log in.
- Employee must have an assigned `UserRole`. Missing role results in HTTP 403 Forbidden.
- JWT audience is `JwtSettings.Audience` (`SmartCartAdmin`).

## Dependencies & Related Modules
- `ADMIN/modules/employee.md` — Manages employee credentials.
- `ADMIN/modules/user-role.md` — Defines role permission matrices.
