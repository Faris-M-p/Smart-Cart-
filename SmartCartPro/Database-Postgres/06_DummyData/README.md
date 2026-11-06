# Optional dummy data

This folder contains fictional development/test records only. It is not
included by `01_Tables/create.sql`, `02_Seed/create.sql`,
`V001__Baseline.sql`, `BuildDatabase.bat`, `ApplyPatches.bat`, or application
startup. The only normal entry point is the explicit manual command
`RunDummyData.bat`.

## Scripts

| File | Contents |
|---|---|
| `001_DevelopmentPlatformSuperAdmin.sql` | Optional local Platform Super Admin login with a documented demo credential; inserts only if the email is unused |
| `002_DevelopmentTrialLicense.sql` | Optional trial license; inserts only when no license exists |
| `003_GreenBasketSupermarket.sql` | Fictional GreenBasket company/store, sample employee roles/users, catalog, stock, purchasing, POS, online orders, and related records |

`RunDummyData.bat` explicitly runs those files in dependency order and stops on
the first SQL error. It verifies the reviewed V001 baseline first. It does not
use recursive SQL discovery and is not called by a normal runner.

## Prerequisites and limits

- Create the database with `BuildDatabase.bat` first. Required roles,
  permissions, settings, module presets/features, and table structures must be
  present.
- Run only against a disposable development/test database selected explicitly
  with `SC_PGDATABASE`. The folder includes a known development password and
  fictional business records; never use it on production.
- GreenBasket uses stable company/store/product/customer/document identifiers
  and looks up generated IDs by those keys. It does not truncate tables or
  delete records. Existing records with the same demo identifiers are retained
  and may be referenced by the sample script; inspect collisions before use.
- GreenBasket inserts a marker setting at the end. A rerun after successful
  completion inserts nothing. A failed transaction rolls back the dataset.
- Existing development databases may already contain GreenBasket rows because
  the old V010 patch was discoverable by `ApplyPatches.bat`. The baseline
  history transition does not remove or clean those records.

The optional platform account is `Muhdfarismp@gmail.com` with the development
password documented in its SQL file. It requires the app's configured OTP
delivery to complete sign-in. GreenBasket employee accounts use the same
development hash and require password change at first sign-in.
