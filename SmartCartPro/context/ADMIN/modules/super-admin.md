# Platform claim (not a separate module)

## Status
- **No** SuperAdmin controller, service, repository, or models.
- Super Admin is only the auth claim `is_platform_super_admin` (policy `PlatformSuperAdmin`).

## Rules
- Same login as everyone.
- Creates company + stores via normal `CompanyController` / `StoreController`.
- Normal admin edits company and stores.
