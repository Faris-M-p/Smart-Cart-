# SmartCartPro — Deferred / future

Document planned work here so AI does not invent it as “done”.

## Deferred features

- Full USER storefront MVC area (shop, cart checkout UX beyond schema tables)
- Platform multi-tenant registry DB (separate from client DB)
- Real auth JWT/cookie + `RequirePermission` end-to-end
- Live dashboard metrics (replace sample KPI cards)
- Stored procedures / PostgreSQL functions
- EF migrations (explicitly out of policy unless product decision changes)
- Advanced pricing, loyalty, gift cards, full GL accounting
- Composite cross-entity store FKs beyond current denormalized `fk_store` (see patch V002 notes)

## Schema already present but UI/app incomplete

Online orders, carts, wishlists, reviews, cash sessions, invoice sequences — tables exist; modules may still be UI shell or PLANNED.

## When promoting a future item

1. Implement code.
2. Move status in module context to `IMPLEMENTED` / `PARTIALLY IMPLEMENTED`.
3. Update `PROJECT.md` / `ARCHITECTURE.md` / `DATABASE.md` as needed.
4. Remove or shrink the bullet here.
