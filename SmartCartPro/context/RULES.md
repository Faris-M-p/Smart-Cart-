# SmartCartPro — Master development rules (COMMON)

These rules apply to **all** SmartCartPro work. Module details live under `ADMIN/modules/` (and later `USER/modules/`). Update this file when architecture or conventions change.

## Context maintenance (permanent)

Whenever a feature, architecture rule, or database behavior changes:

1. Update the relevant root context file (`PROJECT.md`, `ARCHITECTURE.md`, `DATABASE.md`, `RULES.md`, `FUTURE.md`) in the **same task**.
2. Create or update a module file under `context/ADMIN/modules/` (or `context/USER/modules/` when storefront exists).
3. Keep Admin and User documentation separate even when they share tables.
4. Keep `DATABASE.md` synchronized with `Database-Postgres/` SQL.
5. **Do not** create duplicate docs in `findings/`, `docs/`, `decisions/`, or temp logs. **`context/` is the single source of truth.**
6. Do not defer context updates to a later task.

### Module documentation standard sections

Every module file should include:

- Module Name & Area (`ADMIN` / `USER`)
- Purpose & Responsibilities
- Current Status (`IMPLEMENTED` / `PARTIALLY IMPLEMENTED` / `PLANNED` / `UI SHELL`)
- Implemented Features
- Filters / Search / Sorting (only what exists)
- Data (Entities / DTOs)
- Database dependencies
- API & Data Access (Controllers, Services, Repositories)
- UI Rules & Conventions
- Business Rules
- Dependencies & Related Modules
- Notes

---

## Solution layout

| Path | Role |
|------|------|
| `SmartCartPro/` | ASP.NET Core 9 MVC app |
| `SmartCartPro/Database-Postgres/` | PostgreSQL schema, seed, patches, bats |
| `SmartCartPro/context/` | **Single source of truth** for AI/humans |
| `Ecommerce/` | Legacy SmartCart — reference only; do not treat as this app |

---

## Layering (do not invert)

```text
Controller → IService / Service → IRepository / Repository → EF Core (DbContext)
                                      ↘ Dapper only for justified reports
```

| Layer | Does | Does not |
|-------|------|----------|
| Controller | HTTP bind, call service, return `IActionResult` (View/JSON/Redirect) | Query DB, business rules, cookies, hashing, JWT build, non-endpoint helpers |
| Service | Orchestration, mode switches, parse/validate criteria, compose responses, auth cookie set/clear | Raw SQL / DbContext |
| Repository | EF LINQ (preferred) data access only | HTTP, parsing, mode trees, helper/sub-functions for business logic |
| Interface | Contracts in `*/Interfaces/` | Implementations folder |

- **No** `Implementations/` folder.
- Controllers must **not** inject `DbContext`.
- **Controllers stay endpoint-thin:** only action methods plus small `IActionResult` helpers (e.g. safe local redirect). Put cookie write/delete, token issue, password hash/verify, and other non-endpoint logic in the service layer.
- Prefer **EF Core + LINQ**. Use **Dapper** only under `Dapper/` for justified aggregates/reports.
- **Repositories stay thin:** no private parse helpers, no mode switches, no criteria string parsing. Service prepares typed fields on the options model (e.g. `CategoryId` from `Criteria`); repository only reads those fields and queries.

---

## Naming

**PostgreSQL:** `id_*` PKs, `fk_*` FKs (`fk_store`, `fk_company`, `fk_product`). Soft cancel: `cancelled` / `cancelledon` / `cancelledreason`.

**Never** use `StoreId` in SQL — use `fk_store`. C#: `FkStore` mapped to `fk_store`.

**C# folders:** `Controllers/`, `Services/` + `Services/Interfaces/`, `Repositories/` + `Repositories/Interfaces/`, `Entities/`, `Models/`, `Data/`, `Views/`, `Dapper/`.

**DTO packing:** Prefer **one file with multiple related classes** (CRM-style). Example: all filter fill types in `Models/FilterModels.cs`. Do **not** create a new `.cs` file for every small DTO.

---

## Database rules

- SQL under `Database-Postgres/` is the **schema source of truth**.
- **No** EF migrations, `EnsureCreated`, `EnsureDeleted`, or `Database.Migrate`.
- **No** stored procedures / PostgreSQL functions in this phase (`04_Procedures/`, `05_Functions/` reserved empty).
- Build/patch via `BuildDatabase.bat` / `ApplyPatches.bat` with env vars (`SC_PGDATABASE`, `PGPASSWORD`, …). Do not hard-code passwords.
- Do not connect to / modify a live DB or run bats unless the user asks.
- After any schema change: update SQL scripts **and** `context/DATABASE.md` in the same task; align EF entities/DbContext.
- Soft-delete business documents; do not cascade-delete sales, payments, or stock history.
- Scopes: platform-wide / company-wide / store-scoped (`fk_store` required when store-scoped). See `DATABASE.md`.

---

## Filter / dropdown fill (common)

- Endpoint: `GET /Filter/GetFilterData`
- Types live in **`Models/FilterModels.cs` only** (`FilterOptions`, `FilterDataResponse`, `FillProduct`, `FillCategory`, `FillSubCategory`).
- Flow: `FilterController` → `IFilterService` → `IFilterRepository` (EF LINQ).
- Every repo fill method takes `FilterOptions` (extensible for extra criteria).
- **Service** switches on `Mode`, parses/validates `Criteria` (e.g. sets `FilterOptions.CategoryId`), then calls the repo.
- **Repository** must not parse `Criteria` or contain unwanted sub-functions — only use prepared fields (`CategoryId`, `Search`, `Fk_Store`, …) in LINQ.
- Modes: `1` products, `2` categories, `3` subcategories (`Criteria` = category id or empty = all).
- New fill types: add `FillX` **inside `FilterModels.cs`**, extend response + service/repo — no new model file per fill.

---

## UI

- Metis design assets: `wwwroot/metis/` via `_Layout` / `_AuthLayout` / `_MetisAssets`.
- Build SmartCart **business** pages — not Metis demo modules (Elements, Calendar, Messages, Go Pro, etc.).
- Group controllers by domain (`Auth`, `Catalog`, `Inventory`, `Sales`, `Admin`, …).

---

## Auth / security (target)

- Do not commit production secrets.
- Passwords hashed (Identity hasher / equivalent) — never store plain text.
- Future: claims for user, store access, permissions; tenant/store from claims only.

---

## Do-not-break

- Do not dump Metis showcase controllers/pages as product features.
- Do not switch an existing module from EF to Dapper (or reverse) without an explicit decision.
- Do not invent SPs because Ecommerce had them.
- Do not auto-run `dotnet build` / `dotnet run` / DB bats unless the user asks.
- Do not edit `Ecommerce/` when the task is SmartCartPro-only.

---

## Safe change checklist

1. Belongs under an existing ADMIN/USER module?
2. Controller → Service → Repository respected?
3. SQL + `DATABASE.md` updated if schema changed?
4. EF entities aligned (no migration)?
5. DTOs packed into existing model files when possible?
6. Module context file updated?
7. No secrets; no live DB unless asked?
