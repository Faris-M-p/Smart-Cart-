# SmartCartPro — Project

SmartCartPro is an ASP.NET Core 9 MVC application for multi-company / multi-store retail operations: catalog, purchasing, inventory, POS billing, online orders, customers, employees, roles, and settings.

## What it is now

- **Admin-first** UI using the **Metis** template (assets in `wwwroot/metis/`).
- PostgreSQL schema under `Database-Postgres/` (Company, Store, `fk_store`, catalog, stock, sales, etc.).
- Layered C#: Controllers → Services → Repositories (EF Core). Common filter API for dropdowns.
- Many screens are **UI shells** with sample data (`return View()`); business wiring is in progress.
- Customer storefront (USER area) is **planned / deferred** relative to admin.

There is a legacy monolith at `Ecommerce/` (SQL Server + mixed EF/Dapper/SPs). Treat it as a **reference**, not this codebase.

## Prerequisites

- .NET SDK 9
- PostgreSQL + `psql` (for DB bats when needed)
- Node.js only if rebuilding Metis from `metis-v1.0.0/` (built assets already under `wwwroot/metis/`)

## Getting started (high level)

1. Open `SmartCartPro/` solution / project.
2. Set `ConnectionStrings:DefaultConnection` in `appsettings.json` (dev only; do not commit real secrets).
3. Create target DB manually if required; run `Database-Postgres/BuildDatabase.bat` with env vars when ready (do not run unless asked in an AI session).
4. `dotnet run` when the user wants the app started.

## Main users (target)

| Audience | Who | Entry |
|----------|-----|--------|
| Employees / admins | `users` + roles + store access | `/Auth/Login` |
| Customers (future USER) | `customers` | Storefront (not primary yet) |

## Current scope

**ADMIN (this app):** Dashboard shell, Catalog, Inventory, Sales (POS/invoices/returns/online orders), Customers, Reports, Administration (employees, roles, stores, settings), Filter API.

**USER:** Not a separate MVC area yet — document under `USER/` when storefront work starts.

## Technology stack

- `net9.0`, MVC + Razor
- EF Core + Npgsql; Dapper optional for reports
- PostgreSQL
- Metis admin UI (Bootstrap 5 / Alpine assets)

## Context map

| File / folder | Purpose |
|---------------|---------|
| `RULES.md` | **Common rules for every AI/developer** |
| `PROJECT.md` | This overview |
| `ARCHITECTURE.md` | Layers, folders, DI |
| `DATABASE.md` | Schema, scopes, patches |
| `FUTURE.md` | Deferred work |
| `ADMIN/` | Admin area + `modules/` |
| `USER/` | Future storefront context |

## Related docs

- `Database-Postgres/README.md` — SQL build/patch details
- `.cursor/rules/smartcartpro.mdc` — Cursor pointer into this `context/`
