# Admin Module — Auth

## Module Overview
- **Area:** ADMIN
- **Module Name:** Auth
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Employee authentication against `users` (Identity `PasswordHasher`), JWT in HttpOnly cookie, store context via `fk_store` claim from `user_store_access`, first-company bootstrap register, change password, logout.

## Implemented Features
- **Login:** Username/email + password; preferred store from `user_store_access` (`isdefault` first); JWT cookie `smartcartpro.auth.token`
- **Register:** Blocked if any company exists; creates `company` + default `store` + `users` + `user_store_access` (+ `user_roles` SUPER_ADMIN when seeded) in one transaction; signs in with that `fk_store`
- **Change password:** Authenticated; re-issues JWT; clears `mustchangepassword`
- **Logout / LogoutGet:** `IAuthService.ClearAuthCookie()` then redirect to Login
- **Cookie API on service:** `SetAuthCookie` / `ClearAuthCookie` (not private helpers on the controller)
- **Global authorize:** MVC requires authenticated user; Auth actions use `[AllowAnonymous]` where needed
- **Store claims:** JWT includes `fk_store`, `fk_company`, `store_code`, `store_name`, `id_user`

## Filters / Search / Sorting
- N/A

## Data
- Entities: `User`, `UserStoreAccess`, `Role`, `UserRole`, `Company`, `Store`
- Models: `LoginInput`, `RegisterInput`, `ChangePasswordInput`, `AuthSessionResult`, `AuthUserStoreInfo`
- Helpers: `AuthClaims` (`GetFkStore`, `GetFkCompany`, `GetUserId`, `GetStoreName`)

## Database Dependencies
- Tables: `users`, `user_store_access`, `roles`, `user_roles`, `company`, `store`
- Access: EF Core via `AuthRepository` / `AuthService`

## API & Data Access
- Controller: `Controllers/AuthController.cs`
- Service: `Services/AuthService.cs`, `Services/AuthJwtTokenService.cs`
- Repository: `Repositories/AuthRepository.cs`
- Views: `Views/Auth/*` with `_AuthLayout`
- Config: `JwtSettings` in `appsettings.json`

## UI Rules & Conventions
- Brand: SmartCart; Metis auth chrome
- Layout header shows store name from `fk_store` claims (not a hardcoded label)
- Do not seed a known production password

## Business Rules
- Inactive / cancelled users cannot authenticate
- Passwords stored as ASP.NET Identity hashes only
- Login requires at least one active `user_store_access` row; active store sets `fk_store` in the token
- Register is one-time bootstrap (no second company via this path)
- Cookie `Secure` follows `Request.IsHttps` (works on local HTTP)

## Dependencies & Related Modules
- `employee.md`, `roles.md`, `stores.md`
