# Platform claim (not a separate module)

## Status
- **No** SuperAdmin controller, service, repository, or models.
- Super Admin is only the auth claim `is_platform_super_admin` (policy `PlatformSuperAdmin`).
- Full platform provisioning (multi-company wizard, packages, `/platform-access`) remains **ON HOLD**. Do not implement until explicitly requested.

## Rules
- Same login as everyone (OTP in Development may use a fixed code).
- Creates the company via `CompanyController` and stores via `StoreController` (create is platform-only).
- Owns `LicenseController` and `ModuleController` (class-level `PlatformSuperAdmin` policy).
- Normal admin with `settings.manage` edits the company and existing stores.
- Header store switcher does not render for this account (`fk_store` is 0).
