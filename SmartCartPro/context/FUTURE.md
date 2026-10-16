# SmartCartPro — Deferred / future

Document planned work here so AI does not invent it as “done”.

## Deferred features

- **Super Admin (platform) module** — full functional spec + gap analysis held in [`ADMIN/modules/super-admin.md`](ADMIN/modules/super-admin.md). **Do not implement until explicitly requested.** Includes `/platform-access` + OTP, packages, licenses, company wizard, themes, store limits, platform nav. Resolve platform-registry DB vs multi-company single DB before DDL.
- Full USER storefront MVC / React area (shop, cart checkout UX beyond schema tables)
- Platform multi-tenant registry DB (separate from client DB) — likely required by Super Admin SaaS model
- `RequirePermission` / feature-entitlement enforcement end-to-end (permissions seeded; middleware/attributes incomplete)
- Live dashboard metrics (replace sample KPI cards) — Super Admin dashboard must use real DB stats when built
- Stored procedures / PostgreSQL functions
- EF migrations (explicitly out of policy unless product decision changes)
- Advanced pricing, loyalty, gift cards, full GL accounting
- Composite cross-entity store FKs beyond current denormalized `fk_store` (see patch V002 notes)
- License / package / OTP / storefront-theme tables (not in schema yet — see super-admin.md)

## Schema already present but UI/app incomplete

Online orders, carts, wishlists, reviews, cash sessions, invoice sequences — tables exist; modules may still be UI shell or PLANNED.

## Naming watch

Seeded role `SUPER_ADMIN` = **company** full admin, **not** platform Super Admin. Future platform identity must use a distinct flag/role (see `super-admin.md`).

## When promoting a future item

1. Implement code.
2. Move status in module context to `IMPLEMENTED` / `PARTIALLY IMPLEMENTED`.
3. Update `PROJECT.md` / `ARCHITECTURE.md` / `DATABASE.md` as needed.
4. Remove or shrink the bullet here.
