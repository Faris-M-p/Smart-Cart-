# Patches

## Naming

`V###__ShortDescription.sql` — three-digit version, double underscore, then description.

| Version | File | Purpose |
|---------|------|---------|
| V001 | `V001__InitialSchema.sql` | Full initial schema |
| V002 | `V002__CompanyStoreRelationships.sql` | Reserved / no-op marker for future composite integrity |
| V003 | `V003__PlatformSuperAdminOtp.sql` | `users.is_platform_super_admin` + `auth_otp_challenges` |

## Rules

- Apply only via `ApplyPatches.bat`.
- Success is recorded in `patch_history` **after** the script exits 0.
- Never mark a failed patch successful by hand.
- Do not edit already-applied patches on shared databases; add a new version instead.
- `BuildDatabase.bat` records `V001` so the initial schema is not re-applied by the patch runner.
