# Step 6 — Verification checklist

## Counts
| Item | SQL Server (Database) | Postgres (Database-Postgres) |
|------|----------------------|------------------------------|
| Procedures | 76 | 76 |
| Module Patch.sql | 18 | 18 |
| Physical tables (CREATE) | 33 | 33 |
| CREATE FUNCTION | n/a | 0 (none — procedures only) |
| Master scripts | Database.sql, Database-Patch.sql | Database.sql, Database-Patch.sql |

## Name mapping
Every source procedure has a matching snake_case target file (e.g. GetProducts → get_products.sql). No gaps.

## Not fully converted (manual review)
1. **05_Seed/dev_sample_catalog.sql** — stub only (source DevSampleCatalog.sql still SQL Server). Remains commented in patch.sql (same as original).
2. **05_Seed/super_market_catalog.sql** — stub only (source SuperMarketCatalog.sql). Remains commented.
3. **ApplyCartWishlist.sql** — one-off SQL Server root script; not mirrored (cart/wishlist session columns already folded into table DDL).
4. **RunDatabase.bat / RunPatch.bat / SmartCart.dbml** — SQL Server tooling; not converted (use psql + Database.sql).

## Schema adaptation notes (behavior preserved where possible)
- Admin **01_Products** procs adapted from legacy Product columns (price/gender/status on product) onto current products + product_variants + product_media/sku_media. See comments in those procedure files.
- Purchase detail JSON/TVP → `jsonb_array_elements`.
- Multi-result-set procs use `INOUT p_result` / `p_result2` / … refcursors.

## Live apply
`psql` was not available in the conversion environment. Apply locally with:

```bash
cd Ecommerce/Database-Postgres
psql -v ON_ERROR_STOP=1 -v dbname=smartcart -f Database.sql
```
