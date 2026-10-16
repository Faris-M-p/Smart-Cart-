# Admin Module — Auth

## Module Overview
- **Area:** ADMIN
- **Module Name:** Auth
- **Current Status:** `UI SHELL`

## Purpose & Responsibilities
Employee sign-in, registration preview, forgot/reset password screens (Metis auth layout).

## Implemented Features
- Razor pages: Login, Register, ForgotPassword, ResetPassword
- Logout action redirects to Login
- Forms are UI-only until JWT/session is wired

## Filters / Search / Sorting
- N/A

## Data
- Target entities: `users`, `roles`, `user_roles`, `user_store_access`
- DTOs: none required yet (form posts pending)

## Database Dependencies
- Tables: `users`, `roles`, `user_roles`, `permissions`, `role_permissions`, `modules`
- Access: planned EF via Auth service/repository

## API & Data Access
- Controller: `Controllers/AuthController.cs`
- Views: `Views/Auth/*` with `_AuthLayout`

## UI Rules & Conventions
- Brand: SmartCart; Metis auth chrome
- Do not seed a known production password

## Business Rules
- Inactive / cancelled users must not authenticate (when implemented)
- Passwords must be hashed

## Dependencies & Related Modules
- `employee.md`, `roles.md`, `stores.md`