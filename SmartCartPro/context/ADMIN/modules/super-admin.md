# Platform claim (not a separate module)

## Status
- **No** SuperAdmin controller, service, repository, or models.
- Super Admin is only the auth claim `is_platform_super_admin` (policy `PlatformSuperAdmin`).
- Full platform provisioning (multi-company wizard, packages, `/platform-access`) remains **ON HOLD**. Do not implement until explicitly requested.

## Rules
- Same login as everyone (OTP in Development may use a fixed code).
- Software-team owner of the installation: full permission and module access, never license-gated.
- After OTP, the session attaches the default (or first) active store when one exists. If there is no store yet, home is a setup page (company + first store). Creating a store switches the session into it.
- Header store switcher lists every active store. Super Admin does not need `user_store_access`.
- Creates the company via `CompanyController` and stores via `StoreController` (create is platform-only). Store create is not blocked by a missing license; MaxStores applies only when a license row exists.
- Owns `LicenseController` and `ModuleController` (class-level `PlatformSuperAdmin` policy). License is something they manage, not a gate on home or sign-in.
- Normal admin with `settings.manage` edits the company and existing stores.
