# SmartCartPro — PostgreSQL database scripts

SQL scripts are the **source of truth** for schema. EF Core maps to these tables; do **not** use EF migrations, `EnsureCreated`, `EnsureDeleted`, or `Database.Migrate` for this project.

Reference conventions were audited from `Ecommerce/Database-Postgres/` (identity PKs `id_*`, FKs `fk_*`, soft-cancel columns, `TIMESTAMP DEFAULT NOW()`, `NUMERIC` money, `CREATE TABLE IF NOT EXISTS`). Company/Store and `fk_store` are new for SmartCartPro.

## Directory structure

```text
Database-Postgres/
├── 01_Tables/          Numbered table DDL (source fragments + create.sql)
├── 02_Seed/            Idempotent reference data only
├── 03_Patches/         Versioned patches V###__Name.sql
├── 04_Procedures/      Reserved — no executable routines yet
├── 05_Functions/       Reserved — no executable routines yet
├── BuildDatabase.bat   Initial tables + seed (existing DB)
├── ApplyPatches.bat    Pending patches + patch_history
├── DatabaseVersion.sql patch_history helper / version comment
└── README.md
```

File naming: `NNN_EntityName.sql` under `01_Tables/`; patches `V001__InitialSchema.sql` (double underscore).

## Table scopes

| Scope | Meaning | `fk_store` |
|-------|---------|------------|
| **Platform-wide** | Shared reference / meta inside the client DB | No (or N/A) |
| **Company-wide** | Shared across all stores of the company | No |
| **Store-scoped** | Belongs to one store | **Required** `fk_store` → `store(id_store)` |

### Platform-wide

`modules`, `permissions`, `patch_history`

### Company-wide

`company`, `store` (store master), `users`, `roles`, `role_permissions`, `user_roles`, `units`, `taxes`, `categories`, `subcategories`, `brands`, `products`, `product_variants`, `product_images`, `suppliers`, `customers`, `customer_addresses`, `audit_logs` (optional `fk_store`), `product_reviews` (optional `fk_store`)

### Store-scoped

`user_store_access`, `inventory`, `stock_movements`, `purchases`, `purchase_details`, `cash_sessions`, `sales`, `sale_details`, `payments`, `sales_returns`, `sales_return_details`, `expenses`, `online_orders`, `online_order_items`, `carts`, `cart_items`, `wishlists`, `wishlist_items`, `invoice_sequences`

### Mixed

`settings` — `setting_scope` in (`platform`,`company`,`store`); when `store`, `fk_store` is NOT NULL (CHECK enforced).

## `fk_company` and `fk_store` conventions

- PostgreSQL columns are unquoted lowercase: `fk_company`, `fk_store` (same family as legacy `fk_product`).
- C# / EF map these as `FkCompany` / `FkStore` (or `FK_Store` documentation alias); **never** use `StoreId` in SQL.
- `store.fk_company` → `company(id_company)`; store codes unique per company: `UNIQUE (fk_company, storecode)`.
- At most one default store: partial unique index `uq_store_one_default` on `(fk_company) WHERE isdefault AND NOT cancelled`.
- Check: default store must be active and not cancelled.
- **Application transaction required:** create company + first default store together; promote another default before deactivating/cancelling the current default (SQL cannot express “exactly one after setup” alone without blocking the first insert).

## Cross-company integrity

Architecture assumes **one company per client database**. Cross-tenant isolation is primarily by separate databases. Within a client DB:

- All stores share one `company` row (or a small set under the same DB).
- Store-scoped rows always carry `fk_store`; company-wide masters are not duplicated per store.
- Line tables also carry `fk_store` denormalized for indexing and same-store validation; application should ensure parent sale/purchase store matches line `fk_store`. Composite FKs may be added in a later patch (`V002` reserved) if needed.

Do **not** silently `ON DELETE CASCADE` historical `sales`, `payments`, or `stock_movements`. Soft-cancel (`cancelled` / `cancelledon`) is preferred for business documents.

## Build and patch execution

### Prerequisites

- PostgreSQL client `psql` on PATH, or set `SC_PSQL` to full path of `psql.exe`.
- Target database **already created** (this toolset does not `DROP`/`CREATE DATABASE` by default). Creating a DB needs `CREATEDB` / superuser — do that manually.

### Environment variables

| Variable | Purpose |
|----------|---------|
| `SC_PGHOST` | Host (default `localhost`) |
| `SC_PGPORT` | Port (default `5432`) |
| `SC_PGUSER` | User (default `postgres`) |
| `SC_PGDATABASE` | **Required** target database name |
| `PGPASSWORD` | **Required** password (never commit; not echoed by design beyond env) |
| `SC_PSQL` | Optional full path to `psql.exe` |

Example (PowerShell):

```powershell
$env:SC_PGDATABASE = "smartcartpro"
$env:PGPASSWORD = "<secret>"
# optional: $env:SC_PSQL = "C:\Program Files\PostgreSQL\18\bin\psql.exe"
.\BuildDatabase.bat
.\ApplyPatches.bat
```

### `BuildDatabase.bat`

1. Verifies `psql` and connection.
2. Runs `01_Tables/create.sql` (dependency order, `\ir`).
3. Runs seed scripts.
4. Runs `DatabaseVersion.sql`.
5. Inserts `patch_history` row for `V001` if missing (so later `ApplyPatches` skips re-applying initial schema).
6. Exits non-zero on any failure; does not report success otherwise.

### `ApplyPatches.bat`

1. Ensures `patch_history`.
2. Takes `pg_advisory_lock(87245001)`.
3. For each `03_Patches/V*.sql` in name order: skip if version already successful; else apply with `ON_ERROR_STOP`; **only then** insert success row.
4. Unlocks; exits non-zero on failure.

**Failed patch recovery:** fix the SQL; re-run `ApplyPatches.bat`. Do **not** manually insert a success row for a failed patch. If DDL partially applied outside a transaction, repair objects manually to a consistent state before re-running.

Do not re-apply `V001` on an initialized DB — history skip prevents it after Build or a successful Apply.

## Seed data

See `02_Seed/README.md`. Roles, permissions, settings only. No fake sales/customers. First company, default store, and admin user are provisioned by a secure application/ops bootstrap (hashed password), not by seed scripts.

## Adding a new table or patch

1. Add `01_Tables/0NN_Name.sql` and include it in `create.sql` in dependency order.
2. Add the same DDL to a new patch `03_Patches/V00N__ShortName.sql` (do not edit applied V001 on shared environments).
3. Update this README scope classification.
4. Add/update EF entity + `SmartCartDbContext` fluent mapping to match column names (`fk_store`, etc.).
5. Apply with `ApplyPatches.bat` on each environment.

## EF Core synchronization

- Map tables/columns explicitly (`ToTable`, `HasColumnName`).
- Prefer SQL nullability and types; money → `numeric(12,2)` / `HasPrecision(12,2)`.
- Keep schema changes in SQL first, then align C# entities.
- No migrations in this workflow.

## Deferred features (not in initial schema)

- Multi-warehouse transfers beyond movement types
- Full accounting GL / chart of accounts
- Loyalty / gift cards
- Advanced pricing engines
- Platform (multi-tenant registry) database objects — separate from client DB scripts here
- Stored procedures and PostgreSQL functions (`04_Procedures/`, `05_Functions/` reserved empty)

## Restrictions

Scripts are for controlled developer/ops use. They do not run when the .NET app starts. Do not auto-modify production databases.
