# SmartCartPro — Architecture

Architecture **as intended / partially implemented** for SmartCartPro (not Ecommerce).

## Application shape

One web host: `SmartCartPro`. Conventional MVC routes plus attribute API routes (`MapControllers` for `FilterController`).

```text
Browser (Metis admin shell)
  → MVC Controller
      → IService / Service
          → IRepository / Repository
              → EF Core (SmartCartDbContext) → PostgreSQL
              → Dapper (optional reports only)
```

## Project folders

| Path | Role |
|------|------|
| `Controllers/` | MVC + Filter API |
| `Services/`, `Services/Interfaces/` | Business logic |
| `Repositories/`, `Repositories/Interfaces/` | EF data access |
| `Dapper/`, `Dapper/Interfaces/` | Justified report queries |
| `Entities/` | EF table maps |
| `Models/` | DTOs (pack related types in one file) |
| `Data/SmartCartDbContext.cs` | EF mappings |
| `Views/` | Razor (Metis layouts + SmartCart pages) |
| `wwwroot/metis/` | Built Metis CSS/JS/assets |
| `Database-Postgres/` | SQL schema, seed, patches, bats |
| `Exceptions/` | Typed domain/API exceptions (packed in `AppExceptions.cs`) |
| `Middleware/AppExceptionHandler.cs` | Global `IExceptionHandler` — all unhandled exceptions |
| `Configuration/` | `JwtSettings`, `EnvironmentSettings` |
| `Extensions/ServiceCollectionExtensions.cs` | DI |
| `context/` | Specs & rules (this tree) |

## Backend layers

- **Controllers** stay thin: validate, call service, return View or JSON.
- **Services** own Mode switches, criteria parsing/validation (e.g. `Criteria` → `CategoryId`), response composition (e.g. `FilterDataResponse`).
- **Repositories** own EF LINQ only; take prepared `FilterOptions` (no parse helpers / mode trees inside the repo).
- **DI** registered in `ServiceCollectionExtensions` (`AddSmartCartDataAccess`, `AddSmartCartAuth`, `AddSmartCartExceptionHandling`).
- **Errors:** unhandled exceptions → `AppExceptionHandler` → JSON error envelope (developer detail when not Prod).

## Frontend

- Shared `_Layout.cshtml` (admin shell + SmartCart sidebar).
- `_AuthLayout.cshtml` for login/register/password pages.
- `_MetisAssets.cshtml` loads hashed Metis bundles from `/metis/assets/`.

## Controllers by domain

| Controller | Domain |
|------------|--------|
| `AuthController` | Login / register / password |
| `HomeController` | Dashboard |
| `CatalogController` | Categories, brands, products, variants |
| `InventoryController` | Suppliers, purchases, stock |
| `SalesController` | POS, invoices, returns, online orders |
| `CustomersController` | Customers |
| `ReportsController` | Sales / product / customer reports |
| `AdminController` | Employees, roles, stores, settings |
| `FilterController` | Common dropdown fill API |

## Data access policy

| Concern | Access |
|---------|--------|
| CRUD, filter fills | EF Core + LINQ |
| Heavy reports / aggregates | Dapper (justify in code comments) |
| Schema | SQL scripts only — no EF migrations |

## Company / store

- Client DB model: typically one company per database; stores under `store.fk_company`.
- Store-scoped operational data uses `fk_store`.
- User access to stores via `user_store_access` (schema exists; app wiring ongoing).

## Platform Super Admin (planned)

Held in `ADMIN/modules/super-admin.md`. Same MVC host; reuse `users`; no separate Super Admin app/table. Multi-company licensing conflicts with “one company per DB” — decide platform registry vs expanded single DB before implementation. **Do not build until requested.**
