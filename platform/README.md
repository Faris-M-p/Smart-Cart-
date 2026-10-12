# SmartCart Platform (Phase 2 Foundation)

Hybrid greenfield scaffold for the multi-company / multi-store SmartCart platform.

The existing MCA monolith at [`../Ecommerce`](../Ecommerce) is **unchanged** and remains the reference implementation to port from.

## Solution layout

```
platform/
  SmartCart.sln
  src/
    SmartCart.Domain          # Platform + client entities
    SmartCart.Application     # Tenant / license / permission / store contracts
    SmartCart.Infrastructure  # EF Core, tenant resolver, license service
    SmartCart.API             # ASP.NET Core Web API
    SmartCart.Admin           # ASP.NET Core MVC (SuperAdmin + Company areas)
    SmartCart.Customer        # React + Vite storefront (API-only data access)
```

Future GitHub split (when ready):

1. `SmartCart.Admin` (+ shared packages or submodule)
2. `SmartCart.API` (+ shared packages)
3. `SmartCart.Customer`

## Architecture decisions (from Phase 1)

- Database-per-company + separate **Platform** database
- Tenant id only from authenticated claims (`tenant_id`) — never from client body
- Central `ILicenseService` for writes / modules / store limits
- Customer React never connects to PostgreSQL
- Admin UI foundation: Metis (Bootstrap 5) — integrate assets under `SmartCart.Admin/wwwroot` next
- .NET 8 + PostgreSQL + EF Core

## What was implemented in this foundation

- Domain entities for Platform and Client DBs
- `PlatformDbContext` / `ClientDbContext`
- Encrypted client connection mapping via data protection (`TenantConnectionResolver`)
- `LicenseService`, `PermissionService`, `StoreAccessService`
- API JWT auth shell + `TenantResolutionMiddleware`
- `GET /api/platform/license/me` and `GET /api/storefront/config/{companyCode}`
- Admin SuperAdmin / Company menu shells
- Customer app with **ten** theme token sets and storefront API client stub

## Explicitly not done yet (needs approval / later phases)

- Database creation / EF migrations applied to live PostgreSQL
- Company provisioning workflow
- Full Super Admin CRUD screens
- Metis template asset copy into Admin
- Running, watching, or deploying any app
- Splitting into three remote GitHub repositories

## Configuration

Set locally (do not commit secrets):

- `ConnectionStrings:PlatformDatabase`
- `Jwt:SecretKey` (32+ characters)

Example placeholders are in each `appsettings.json` with `CHANGE_ME`.

## Commands (run only when you approve)

```bash
# API / Admin — restore & build only when ready
dotnet build SmartCart.sln

# Customer — install & dev only when ready
cd src/SmartCart.Customer
npm install
npm run dev
```

Migrations must not be applied without explicit approval.
