# Database

PostgreSQL scripts: `Database-Postgres/`. SQL is authoritative; do not use EF migrations.

## Layout

| Path | Role |
|---|---|
| `01_Tables/` | Final table definitions and dependency-ordered `create.sql` |
| `02_Seed/` | Required roles, permissions, settings, presets, and module features |
| `03_Patches/` | `V001__Baseline.sql`; future patches begin with V002 |
| `06_DummyData/` | Optional fictional records; manual-only via `RunDummyData.bat` |
| `BuildDatabase.bat` / `ApplyPatches.bat` | Fresh baseline / future patch runners |
| `DatabaseVersion.sql` / `patch_history` | Baseline and patch tracking |

The baseline includes final table DDL and required system seeds. It creates no
company, store, employee account, production credential, or license. Dummy data
is never run by baseline, patch, or app startup paths.

License, installation, module preset/feature, invitation, and OTP challenge
tables are included in the baseline. `01_Tables/` is authoritative for their
final definitions.

Full workflows and the explicit legacy development history transition are in
`Database-Postgres/README.md`.
