# SmartCartPro PostgreSQL database

PostgreSQL scripts are the schema source of truth. EF Core maps the SQL schema;
do not use EF migrations, `EnsureCreated`, `EnsureDeleted`, or `Database.Migrate`.
No SQL script is executed automatically by application startup.

## Layout

```text
Database-Postgres/
  01_Tables/       Final authoritative table definitions and ordered create.sql
  02_Seed/         Required system/reference data and explicit create.sql
  03_Patches/      V001__Baseline.sql, then future V002__Description.sql onward
  04_Procedures/   Reserved; no executable procedures
  05_Functions/    Reserved; no executable functions
  06_DummyData/    Optional fictional data; manual execution only
  DatabaseRunner.ps1 Shared configuration, psql, authentication, and errors
  BuildDatabase.bat
  ApplyPatches.bat
  RunDummyData.bat
  DatabaseVersion.sql
```

`BuildDatabase.bat` invokes only `03_Patches/V001__Baseline.sql`. The baseline
includes `01_Tables/create.sql`, `02_Seed/create.sql`, and version metadata by
explicit `\ir` paths. It writes the successful V001 marker in the same
transaction, after schema and required seeds complete. `ApplyPatches.bat`
accepts only a database with that exact marker and applies pending V002+ files.
Neither normal runner reads or calls `06_DummyData`.

## Required system data and optional dummy data

`02_Seed/` holds the module and permission catalog, role definitions and their
default grants, company settings, module presets, and menu feature catalog/core
module selections. These records are used by application authorization,
settings, navigation, and installation-module flows. See
[`02_Seed/README.md`](02_Seed/README.md).

`06_DummyData/` holds the optional development Platform Super Admin account,
development trial license, and GreenBasket sample company/store, users,
catalog, suppliers, stock, purchases, sales, and other fictional transactions.
They contain no production credentials and are never included in V001 or future
patches. Run them only by explicitly invoking `RunDummyData.bat` against a
disposable development/test database. The scripts do not truncate or delete
rows to make room for samples and insert by stable natural keys. See
[`06_DummyData/README.md`](06_DummyData/README.md).

A production installation gets its license and first administrator through a
secure provisioning process; no hard-coded production account or license key
is seeded.

## Connection configuration

The local connection defaults are hard-coded near the top of
`DatabaseRunner.ps1`. Edit them when switching servers. Existing libpq
environment variables still override those values:

| Variable | Purpose |
|---|---|
| `SC_PGHOST` | Host; defaults to `localhost` |
| `SC_PGPORT` | Port; defaults to `5432` |
| `SC_PGUSER` | User; defaults to `postgres` |
| `SC_PGDATABASE` | Optional target database override; defaults to `smartcart_pro` |
| `SC_PGMAINTENANCEDATABASE` | Maintenance database for creation; defaults to `postgres` |
| `PGPASSWORD` | Optional inherited password; otherwise one hidden password prompt is shown |
| `SC_NO_PAUSE` | Set to `1` only for command-line use when the final pause is not wanted |
| `SC_PSQL` | Optional path or command for `psql.exe`; otherwise use the hard-coded PostgreSQL 18 path, PATH, or another standard PostgreSQL install folder |

The built-in values are `localhost`, `5432`, `postgres`, `smartcart_pro`, and
the `postgres` maintenance database. The target name is validated before use.
`BuildDatabase.bat` needs a PostgreSQL account with `CREATEDB` permission when
creating a new database. Passwords are never stored in these files; typed
passwords are entered invisibly and passed to `psql.exe` through its process
environment. Alternatively, use a PostgreSQL `pgpass` file or another
configured libpq authentication method. If `psql.exe` cannot be found, set
`SC_PSQL` or edit `DefaultSettings.PsqlPath` in `DatabaseRunner.ps1`.

## Fresh database workflow

1. In File Explorer, open `Database-Postgres` and double-click
   `BuildDatabase.bat`.
2. The local connection settings are used without a settings form. Enter the
   PostgreSQL password at the single hidden prompt unless it is already
   supplied through `PGPASSWORD` or another libpq authentication method.
3. For a new `smartcart_pro` target, the runner connects to the maintenance
   database, creates the target, and executes `03_Patches/V001__Baseline.sql`
   once. If it already exists, no changes are made and the runner directs you
   to `ApplyPatches.bat`.

The baseline creates the final tables, required system/reference rows, patch
history, and successful V001 marker in one transaction. It does not create a
company, store, employee, license, or dummy business records. Provision
installation data through the approved setup workflow. The baseline already
includes all schema changes; do not run it separately or apply V001 again.

To update an existing SmartCartPro database, double-click `ApplyPatches.bat`.
It verifies the V001 marker and applies only successful-history-missing V002+
patches. There are currently no V002+ patch files.

## Future schema changes

The next patch is `03_Patches/V002__Description.sql`. For each future change:

1. Update the final table definition in `01_Tables/` and its include in
   `01_Tables/create.sql` when the schema changes.
2. Add an additive, reviewed V002+ patch for databases already at the baseline.
   If required system seed rows change, keep them idempotent in `02_Seed/` and
   include that seed file explicitly from the patch when existing installations
   need the rows.
3. Run `ApplyPatches.bat` against each already-baselined database. It applies
   only pending V002+ files and records success only after SQL succeeds.
4. Update documentation and EF entities/mappings to match the final schema.

With the same connection variables set, the future update command is:

```powershell
.\ApplyPatches.bat
```

Never edit a patch that may already have run on a shared database. Never put
dummy/demo statements in a patch. A failed patch is not marked successful;
review its partial effects and repair them before retrying.

## Manual dummy data workflow

After the fresh baseline succeeds on a database intentionally chosen for local
development/testing, manually double-click `RunDummyData.bat`.

The runner uses the same hard-coded defaults, optional `SC_PG*` overrides,
`PGPASSWORD`, and `SC_PSQL` settings and checks for the reviewed V001 marker. It
explicitly lists its scripts; it does not recursively discover SQL. Dummy data is only run when
you manually launch this separate file; `BuildDatabase.bat`,
`ApplyPatches.bat`, the baseline, and application startup never invoke it. Demo
prerequisites are documented in `06_DummyData/README.md`.

## Existing development databases with old V001–V013 history

The old V001 meant `InitialSchema`; it is not the new `Baseline`. The patch
runner checks the V001 description and rejects old history rather than silently
skipping the new baseline. Do not run `BuildDatabase.bat` on an existing DB.

An explicit transition is available only for a development database whose old
patch history and schema have been reviewed:

1. Take and verify a database backup. Inspect `patch_history`; do not transition
   a database with failed, unknown, or partially applied old patches.
2. Confirm the schema already includes the cumulative old V002–V013 structure
   needed by the current `01_Tables/` final definitions, including
   `users.session_version`, licensing/installation/feature tables,
   `product_reviews.fk_onlineorderitem`, purchase status constraint, and report
   indexes. Resolve missing or custom schema changes manually before proceeding.
3. Preserve the old history rows under legacy labels, then insert the new
   baseline marker. This does not delete/reset rows or apply DDL. Run only after
   the schema review and backup:

   ```sql
   BEGIN;
   DO $$
   BEGIN
       IF EXISTS (SELECT 1 FROM patch_history WHERE version LIKE 'LEGACY-%') THEN
           RAISE EXCEPTION 'Legacy history labels already exist; inspect before transition';
       END IF;
       IF EXISTS (SELECT 1 FROM patch_history WHERE version !~ '^V[0-9]{3}$') THEN
           RAISE EXCEPTION 'Unexpected patch-history versions; inspect before transition';
       END IF;
   END $$;
   UPDATE patch_history
      SET version = 'LEGACY-' || version,
          description = 'Preserved pre-baseline history: ' || COALESCE(description, '')
    WHERE version ~ '^V[0-9]{3}$';
   INSERT INTO patch_history (version, description, appliedby, success)
   VALUES ('V001', 'SmartCartPro baseline V001', current_user, TRUE);
   COMMENT ON TABLE patch_history IS
       'SmartCartPro baseline V001; successful follow-up patches are recorded in patch_history.';
   COMMIT;
   ```

4. Run `ApplyPatches.bat` only after that manual transition. Existing data is
   retained. Old V010 may already have inserted GreenBasket demo rows; this
   transition does not identify or delete them. Review fictional records
   separately and never delete rows without confirming they are safe to remove.

This transition only re-labels known version-history rows and marks the
already-present cumulative schema as the new baseline; it does not make an old
database match the new schema. If the old patch history is incomplete, the
schema differs from the final definitions, or real and demo data cannot be
distinguished, keep the database untouched and plan a reviewed manual migration
or a clean development database.
