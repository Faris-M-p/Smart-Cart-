# Seed data

## Scripts

| File | Purpose |
|------|---------|
| `001_DefaultRoles.sql` | System roles (SUPER_ADMIN, STORE_MANAGER, …) |
| `002_DefaultPermissions.sql` | Modules, permissions, SUPER_ADMIN grants |
| `003_DefaultSettings.sql` | Company-level default settings |

Seeds are **idempotent** (`WHERE NOT EXISTS`) and contain **no** fake customers, sales, or payments.

## First company / store / administrator

Do **not** seed a production admin with a known password.

Recommended bootstrap (application or one-time ops script, in a single transaction):

1. `INSERT` into `company`.
2. `INSERT` into `store` with `isdefault = TRUE` for that company.
3. Create the first user with a **hashed** password supplied securely (prompt / secret store).
4. Assign `SUPER_ADMIN` via `user_roles`.
5. Grant `user_store_access` for the default store.

`BuildDatabase.bat` applies seed scripts after the schema; it does not create a company or admin user.
