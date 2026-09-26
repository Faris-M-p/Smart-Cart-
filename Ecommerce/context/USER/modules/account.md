# User Module — Account

## Module Overview
- **Area:** USER (Storefront)
- **Module Name:** Account
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Handles customer authentication including registration, login, JWT cookie generation, session validation, and logout.

## Implemented Features
- **Customer Registration:** Validates customer inputs, hashes password using ASP.NET Identity `PasswordHasher`, calls `RegisterUser` stored procedure.
- **Customer Login:** Validates credentials via `GetUserByEmail`, verifies password hash, generates customer JWT token set in HttpOnly cookie (`UserAuthHelper.TokenCookieName`).
- **Logout:** Clears the customer JWT authentication cookie and redirects to Home.

## Filters / Search / Sorting
- N/A.

## Data
- DTOs: `RegisterUserView`, `LoginView`, `User` entity snapshot.

## Database Dependencies
- Tables: `Users`.
- Stored Procedures: `RegisterUser`, `GetUserByEmail`, `GetUserById`.

## API & Data Access
- Controller: `Controllers/AccountController.cs` (`Login`, `Register`, `Logout`).
- Repository: `UserAuthRepository.cs` via `IDataAccessDapper`.
- Token Service: `UserJwtTokenService.cs`.

## UI Rules & Conventions
- Pages rendered in `views/Account/Login.cshtml` and `views/Account/Register.cshtml`.
- Error messages displayed via ModelState validation summaries.

## Business Rules
- Customer identity is separate from Admin (`AdminUsers`). Customer table `Users.IsAdmin` column is not used for admin access.
- Passwords are securely hashed using ASP.NET Identity `PasswordHasher<T>` (PBKDF2 with HMAC-SHA256).
- JWT token audience is `JwtSettings.UserAudience` (`SmartCartUser`).

## Dependencies & Related Modules
- `USER/modules/cart.md` — Requires authenticated customer `UserId`.
- `USER/modules/checkout.md` — Requires authenticated customer `UserId`.
- `USER/modules/orders.md` — Requires authenticated customer `UserId`.
