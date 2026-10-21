# Admin Module — Employees

## Module Overview
- **Area:** ADMIN
- **Module Name:** Employees (users)
- **Current Status:** `IMPLEMENTED` (server + AJAX UI). Written without a build or a browser run; the first build and click-through still has to confirm it (see Notes).

## Purpose & Responsibilities
The company administrator manages the staff accounts that sign in to SmartCartPro: profile, exactly one role, store access, active / inactive state and administrator password resets. Platform Super Admin accounts are never listed, created or changed here.

## Implemented Features
- `Employee/Index` list: search, role / store / status filters, sorting, paging, empty state, per-row Edit and Activate / Deactivate (confirm → POST → toast).
- `Employee/Create`: username, full name, email, phone, employee code, role, store access with a default store, password mode:
  - **Generate** (default): a 14-character random temporary password is returned **once** in the success response and shown in a panel with a Copy button; it is never stored in plain text.
  - **Type one**: validated server-side with `[StrongPassword(8, 64)]`.
  - "Require a password change at next sign-in" (always applied when a password is generated).
- `Employee/Edit`: profile, role and store access; account card (status, last sign-in, password-change flag); **Reset password** card (generate or type, shown once, user must change it at next sign-in).
- `Employee/SetActive` (POST): activate / deactivate without opening the form. Inactive employees cannot sign in.
- Add / Edit forms submit through `SmartCartAjax.submitForm`: field errors appear under the inputs, success redirects to the list with a flash toast (except when a temporary password has to be shown first).

## Routes & Permissions
Every action requires module `USERS` and permission `users.manage` (class-level `[ModuleAccess("USERS")]` + `[RequirePermission("users.manage")]`; the platform Super Admin bypasses both, as everywhere).

| Method | Route | Action | Notes |
|---|---|---|---|
| GET | `/Employee` | `Index` | query: `Search`, `FkRole`, `FkStore`, `ActiveOnly`, `SortBy`, `SortDesc`, `Page`, `PageSize` |
| GET / POST | `/Employee/Create` | `Create` | POST returns `{ id, userName, temporaryPassword, redirectUrl }` (`Cache-Control: no-store`) |
| GET / POST | `/Employee/Edit/{id}` | `Edit` | POST returns `{ id, redirectUrl }` |
| POST | `/Employee/SetActive` | `SetActive` | `id`, `isActive` |
| POST | `/Employee/ResetPassword/{id}` | `ResetPassword` | returns `{ id, temporaryPassword }` (`Cache-Control: no-store`) |

Previously `EmployeeController` used the `PlatformSuperAdmin` policy on writes and had no check at all on `Index` / `Edit`. The policy was replaced by the module permission so the company admin can do the work.

## Filters / Search / Sorting
- Search (case-insensitive contains): full name, username, email, employee code.
- Filters: role, store (active access rows), status (all / active / inactive).
- Sort: `fullname` (default), `username`, `email`, `role`, `lastlogin`, `isactive`; page size 10 / 25 / 50 / 100. An out-of-range page falls back to the last page.

## Data
- Entities (repository only): `User`, `UserRole`, `UserStoreAccess`; read-only lookups of `Role` and `Store` for the pickers.
- Models: `Models/UserModels.cs` — regions User Options, List, Select, Create, Update, Status, Password Reset. `InUserCreateView`, `InUserUpdateView`, `InUserPasswordUpdateView` carry the validation; `Input…` / `Output…` stay plain.

## Database Dependencies
- Tables: `users`, `user_roles`, `user_store_access`, `roles`, `store`.
- `uq_user_store_access (fk_user, fk_store)` also covers cancelled rows, so removing and re-adding a store **revives** the old row.
- A partial unique index allows one default store per user among non-cancelled rows; the update writes in two passes inside one transaction to respect it.
- No schema change was needed.

## API & Data Access
- `EmployeeController` → `IUserService` / `UserService` → `IUserRepository` / `UserRepository` (EF only).
- The controller maps `InXxxView` → `InputXxx` with a named variable, then calls the service (no inline `new` in the call).
- `IUserService.GetUserPermissionsAsync` is used by the shared filters and the layout and was left untouched.

## UI Rules & Conventions
- Views: `Views/Employee/Index.cshtml`, `Create.cshtml`, `Edit.cshtml`, `_StorePicker.cshtml` (store checkboxes with a Default radio).
- JS: `wwwroot/js/admin/employee.js` on top of `sc-ajax.js` (password mode switch, show / hide password, copy, store-picker sync, local time for `time[data-sc-utc]`). No Bootstrap JS needed.
- Error keys match input names: `UserName`, `Email`, `FullName`, `Phone`, `EmployeeCode`, `FkRole`, `StoreIds`, `DefaultStoreId`, `InitialPassword`, `NewPassword`. Validation messages in the service use the generic summary "Please correct the highlighted fields." and the detail goes to the field.

## Business Rules
- Platform Super Admin accounts (`is_platform_super_admin`) and cancelled users are excluded from every query and write.
- Company scope: a company admin only reaches employees that have an active access row on a store of their company (claim `fk_company`). The platform Super Admin sees all companies. Role and store pickers follow the same scope.
- Username **and** email are unique across all accounts, ignoring case (login accepts either, and the database constraints are case-sensitive).
- One role per employee (active role; an inactive role is accepted only when the employee already holds it). At least one store is required; the default store must be one of the selected stores (first one when none chosen).
- Self-protection: you cannot deactivate yourself, change your own role or reset your own password here (use *Change password*).
- Passwords: hashed with `IPasswordHasher<object>`; the generated password comes from `RandomNumberGenerator`, is policy-compliant and has no look-alike characters; plain passwords and hashes are never logged or written to the audit trail. Reset sets `must_change_password = true`; the auth module redirects to *Change password* after the next sign-in and clears the flag when the password is changed.
- Audit (`IAuditService`, entity `User`): Create, Update, Activate, Deactivate, ResetPassword.
- No hard delete: deactivate instead.

## Dependencies & Related Modules
- `auth.md` (login, claims, change password), `roles.md` (role catalog), `stores.md` (store list), `audit.md`.

## Notes
- Deactivating an employee blocks **new** sign-ins; a JWT that is already issued stays valid until it expires (the `RequirePermission` filter does not re-check `users.is_active`).
- `must_change_password` is enforced right after login only; it is not a gate on other URLs.
- Roles are global in the current schema (no company column), so the role pickers list every role.
- An employee shared by two companies keeps the other company's access rows untouched when a company admin edits them.
- Nothing was built or run when this was written: compile and click-through checks are still open.
