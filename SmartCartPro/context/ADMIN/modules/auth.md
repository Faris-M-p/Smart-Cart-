# Admin Module — Auth

## Module Overview
- **Area:** ADMIN
- **Module Name:** Auth
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Employee authentication against `users` (Identity `PasswordHasher`), JWT in HttpOnly cookie, store context via `fk_store` claim from `user_store_access`, change password, logout.

There is **no** Auth Register endpoint or view. Company onboarding belongs to Super Admin (`super-admin.md`).

## Implemented Features
- **Login:** Username/email + password; preferred store from `user_store_access`; JWT cookie `smartcartpro.auth.token`
- **Change password:** Authenticated; re-issues JWT; clears `mustchangepassword`
- **Logout / LogoutGet:** `ClearAuthCookie()` then redirect to Login
- **Cookie API on service:** `SetAuthCookie` / `ClearAuthCookie`
- **Global authorize:** MVC requires authenticated user; Auth login/forgot use `[AllowAnonymous]`
- **Store claims:** JWT includes `fk_store`, `fk_company`, `store_code`, `store_name`, `id_user`

## Filters / Search / Sorting
- N/A

## Data
- Entities: `User`, `UserStoreAccess`, `Company`, `Store` (read via joins)
- Models: `LoginInput`, `ChangePasswordInput`, `AuthSessionResult`, `AuthUserStoreInfo`
- Helpers: `AuthClaims`

## Database Dependencies
- Tables: `users`, `user_store_access`, `store`, `company`
- Access: EF Core via `AuthRepository` / `AuthService`

## API & Data Access
- Controller: `Controllers/AuthController.cs` (Login, ChangePassword, Forgot/Reset, Logout — **no Register**)
- Views: `Views/Auth/Login`, `ChangePassword`, `ForgotPassword`, `ResetPassword`

## UI Rules & Conventions
- Brand: SmartCart; Metis auth chrome
- Layout header shows store name from `fk_store` claims

## Business Rules
- Inactive / cancelled users cannot authenticate
- Passwords stored as ASP.NET Identity hashes only
- Login requires active `user_store_access` for `fk_store` claim

## Dependencies & Related Modules
- `super-admin.md` — future company registration
- `employee.md`, `roles.md`, `stores.md`
