# SmartCartPro — Database

PostgreSQL. Scripts: `SmartCartPro/Database-Postgres/`.

## Source of truth

| Path | Role |
|------|------|
| `01_Tables/` | Numbered table DDL + `create.sql` |
| `02_Seed/` | Roles, permissions, settings (idempotent; no fake sales) |
| `03_Patches/` | `V001__InitialSchema.sql`, `V002__…` |
| `BuildDatabase.bat` / `ApplyPatches.bat` | Windows ops (env-based) |
| `DatabaseVersion.sql` / `patch_history` | Patch tracking |

**Do not** use EF migrations. Align entities after SQL changes.

## Naming

- PK: `id_<entity>` `INT GENERATED ALWAYS AS IDENTITY`
- FK: `fk_<ref>` (e.g. `fk_store`, `fk_category`)
- Soft cancel columns on business tables
- Money: `NUMERIC(12,2)`

## Scopes

| Scope | Examples | `fk_store` |
|-------|----------|------------|
| Platform-wide | `modules`, `permissions`, `patch_history` | No |
| Company-wide | `company`, `store`, users/roles, categories, products, customers, suppliers | No (store master has `fk_company`) |
| Store-scoped | inventory, stock_movements, purchases, sales, payments, cash_sessions, online_orders, carts | **Required** |

## Company / store invariants

- Store codes unique per company.
- At most one default store per company (partial unique index).
- Create company + first default store in **one application transaction**.
- Promote another default before deactivating/cancelling the current default.

## Filter-related tables

- `products` → mode 1 fills (`id_product`, `productname`)
- `categories` → mode 2 (`id_category`, `categoryname`)
- `subcategories` → mode 3 (`id_subcategory`, `subcategoryname`, `fk_category`)

## Seed

Roles (`SUPER_ADMIN`, `STORE_MANAGER`, `CASHIER`, …), permissions, company settings.  
No production admin password in seed — bootstrap hashed admin separately.

**Naming:** `SUPER_ADMIN` = company-wide full access. Platform Super Admin (licenses/packages) is **PLANNED** and must use a distinct protected identity — see `ADMIN/modules/super-admin.md`.

## Not in schema yet (Super Admin hold)

License, package/menu-group, company feature entitlement, OTP challenge, and storefront-theme tables are **not** created. Add only via SQL scripts when Super Admin implementation is authorized. Prefer resolving platform-registry DB vs multi-company single DB first (`FUTURE.md`).

## Change workflow

```text
Change SQL in Database-Postgres/
  → Update DATABASE.md (this file)
  → Align Entities + SmartCartDbContext
  → Update affected ADMIN/modules/*.md
```

Full operational detail: `Database-Postgres/README.md`.
