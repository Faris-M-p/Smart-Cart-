# Seed data

## Scripts

| File | Purpose |
|------|---------|
| `001_DefaultRoles.sql` | System roles (ADMIN, STORE_MANAGER, …) |
| `002_DefaultPermissions.sql` | Modules, permissions, ADMIN grants |
| `003_DefaultSettings.sql` | Company-level default settings |
| `004_PlatformSuperAdmin.sql` | Platform Super Admin (`Muhdfarismp@gmail.com`) — OTP login; change password in non-dev |

Seeds are **idempotent** (`WHERE NOT EXISTS`) and contain **no** fake customers, sales, or payments.

## First company / store / administrator

Company store admins are provisioned by **platform Super Admin** (not public register).  
Do **not** treat the platform Super Admin seed password as a production secret forever — rotate after first deploy.

Recommended bootstrap (application or one-time ops script, in a single transaction):

1. `INSERT` into `company`.
2. `INSERT` into `store` with `isdefault = TRUE` for that company.
3. Create the first user with a **hashed** password supplied securely (prompt / secret store).
4. Assign `ADMIN` via `user_roles`.
5. Grant `user_store_access` for the default store.

`BuildDatabase.bat` applies seed scripts after the schema; it does not create a company or admin user.
