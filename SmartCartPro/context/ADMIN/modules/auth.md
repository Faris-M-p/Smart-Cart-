# Admin Module — Auth

## Module Overview
- **Area:** ADMIN
- **Module Name:** Auth
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Employee authentication against `users` (Identity `PasswordHasher`), JWT in HttpOnly cookie, store context via `fk_store` claim from `user_store_access`, change password, logout.

**Company registration is not public.** Per Super Admin rules (`super-admin.md`), companies are provisioned by platform Super Admin — not via `/Auth/Register`.

## Implemented Features
- **Login:** Username/email + password; preferred store from `user_store_access` (`isdefault` first); JWT cookie `smartcartpro.auth.token`
- **Register (public):** **Disabled** — GET/POST `/Auth/Register` redirect to Login; Login page has no “Register company” link
- **Change password:** Authenticated; re-issues JWT; clears `mustchangepassword`
- **Logout / LogoutGet:** `IAuthService.ClearAuthCookie()` then redirect to Login
- **Cookie API on service:** `SetAuthCookie` / `ClearAuthCookie` (not private helpers on the controller)
- **Global authorize:** MVC requires authenticated user; Auth actions use `[AllowAnonymous]` where needed
- **Store claims:** JWT includes `fk_store`, `fk_company`, `store_code`, `store_name`, `id_user`

## Filters / Search / Sorting
- N/A

## Data
- Entities: `User`, `UserStoreAccess`, `Role`, `UserRole`, `Company`, `Store`
- Models: `LoginInput`, `RegisterInput` (kept for future Super Admin wizard reuse), `ChangePasswordInput`, `AuthSessionResult`, `AuthUserStoreInfo`
- Helpers: `AuthClaims` (`GetFkStore`, `GetFkCompany`, `GetUserId`, `GetStoreName`)

## Database Dependencies
- Tables: `users`, `user_store_access`, `roles`, `user_roles`, `company`, `store`
- Access: EF Core via `AuthRepository` / `AuthService`

## API & Data Access
- Controller: `Controllers/AuthController.cs`
- Service: `Services/AuthService.cs` (`RegisterAsync` retained for Super Admin when implemented — not exposed publicly)
- Repository: `Repositories/AuthRepository.cs`
- Views: `Views/Auth/Login`, ChangePassword, Forgot/Reset; Register view is a dead-end message only
- Config: `JwtSettings` in `appsettings.json`

## UI Rules & Conventions
- Brand: SmartCart; Metis auth chrome
- Layout header shows store name from `fk_store` claims (not a hardcoded label)
- Do not seed a known production password
- Do not offer public “First setup? Register company”

## Business Rules
- Inactive / cancelled users cannot authenticate
- Passwords stored as ASP.NET Identity hashes only
- Login requires at least one active `user_store_access` row; active store sets `fk_store` in the token
- **No public company signup** — Super Admin company wizard (PLANNED) owns onboarding
- Cookie `Secure` follows `Request.IsHttps` (works on local HTTP)

## Dependencies & Related Modules
- `super-admin.md` — platform company registration
- `employee.md`, `roles.md`, `stores.md`
