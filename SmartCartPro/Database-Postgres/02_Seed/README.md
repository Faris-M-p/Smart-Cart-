# Required system seed data

`create.sql` is the explicit dependency-ordered entry point. It runs only from
`V001__Baseline.sql`; it does not discover files recursively.

| File | Required data |
|---|---|
| `001_DefaultRoles.sql` | Company roles used by the employee and permission flows |
| `002_DefaultPermissions.sql` | Module catalog, permission catalog (including `pos.cancel` and `reports.profit`), Admin grants, and default grants for built-in employee roles |
| `003_DefaultSettings.sql` | Company-level settings defaults used by POS, inventory, and locale behavior |
| `004_PlatformSuperAdmin.sql` | Platform Super Admin credential (`Muhdfarismp@gmail.com`) for system administration and onboarding |
| `005_ModulePresets.sql` | Basic/Premium presets and their module membership |
| `007_ModuleFeatures.sql` | Module descriptions/core flags, menu feature catalog, enabled core modules/features, and preset feature membership |

The seed scripts are idempotent for normal reruns and use stable role, module,
permission, setting, preset, and feature codes. They do not create a company,
store, employee account, license, customer, product, supplier, or transaction.

## First installation provisioning

After the baseline, provision a real license and the first platform/company
administrator through the authorized secure operations workflow. Do not add a
production username, password, or license key to these seed files. Create the
company, default store, and company Admin user with a securely supplied hashed
password, then assign `ADMIN` and default-store access in one transaction.

Development accounts, trial licenses, and fictional business records are in
`06_DummyData/` and run only through the explicit `RunDummyData.bat` command.
