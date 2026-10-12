# Phase 2 Foundation — delivery notes

## Changed / added paths

- `platform/SmartCart.sln`
- `platform/src/SmartCart.Domain/**`
- `platform/src/SmartCart.Application/**`
- `platform/src/SmartCart.Infrastructure/**`
- `platform/src/SmartCart.API/**`
- `platform/src/SmartCart.Admin/**`
- `platform/src/SmartCart.Customer/**`
- `platform/README.md`

## Database changes

- **None applied.** Contexts and models are defined only.
- Planned databases: `smartcart_platform` + per-company client DBs.

## API changes (scaffolded)

- `GET /health`
- `GET /api/platform/license/me` (authorized; tenant from claims)
- `GET /api/storefront/config/{companyCode}` (anonymous; no secrets)

## Tests

- Not added in this foundation slice.

## Remaining issues / next approvals

1. Apply EF migrations (Platform first) after you approve DB work.
2. Copy Metis assets into Admin `wwwroot` (path on your machine: Downloads/metis-v1.0.0).
3. Implement Super Admin login + company provisioning transaction.
4. Wire Admin company screens to API (API-centric ops).
5. Decide GitHub remote split timing for the three application repos.
6. Discard or archive empty `SmartCartPro/` (.NET 9 scaffold) when you confirm.

## Safety

- Existing `Ecommerce/` monolith was not modified.
- No apps were started; no live DB migrations were executed.
