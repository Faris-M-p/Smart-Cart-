# SmartCart — Architecture

This is the architecture **as implemented**, not a target design.

## Application shape

One web host (`Ecommerce`). `Program.cs` maps a single conventional route:

`{controller=Home}/{action=Index}/{id?}`

Admin uses attribute routes on controllers (`[Route("Admin/Product")]`, `[Route("api/admin/auth")]`). There is no separate API project.

```
Browser (storefront CSS / Mantis admin)
    → MVC controller
        → Interface + Repository
            → Dapper + SP   or   EF Core
                → SQL Server (SmartCart)
```

Static files from `wwwroot/` (storefront assets, Admin template, `uploads/`).

## Project folders (relevant)

| Path | Role |
| --- | --- |
| `Controllers/`, `Controllers/Admin/` | MVC |
| `Repository/`, `Repository/Admin/` | Data access |
| `Interface/`, `Interface/Admin/` | Abstractions |
| `Models/`, `Models/Admin/`, `Models/Entities/` | DTOs and EF entities |
| `Helpers/` | List normalize, auth helpers, JWT, images |
| `DataAccess/` | `DataAccessDapper`, `EcommerceDbContext` |
| `Filters/RequirePermissionAttribute.cs` | Admin permission |
| `Middleware/` | Global exception, request log, admin session |
| `views/` | Razor (storefront + Admin + Shared layouts) |
| `Database/` | Tables, procedures, seed, sqlcmd entrypoints |
| `wwwroot/Admin/` | Mantis assets + compiled CSS |
| `wwwroot/uploads/` | Uploaded media |

Empty folder entries exist in the csproj (`Controllers/User`, `views/User`, …) but **storefront code does not live under those folders**.

## Backend layers

**Controllers** stay thin: validate model state, call repository, return `View` or JSON.

**Helpers** normalize paging/search/sort for admin lists (`CategoryHelper`, `ProductHelper`, …) and shop (`ShopHelper`).

### Database Trace Logging Layer

- **Target File:** `Logs/DatabaseTrace.txt`
- **Logger Helper:** `DatabaseTraceLogger.cs` (thread-safe, high-performance trace writer with password/token parameter masking).
- **EF Core Interceptor:** `EFDbCommandInterceptor.cs` registered via `options.AddInterceptors(new EFDbCommandInterceptor())` in `Program.cs`.
- **Dapper / ADO.NET Wrapper:** `TraceDbConnection.cs` wrapping `SqlConnection` in `DataAccessDapper.CreateConnection()`.
- **Captured Telemetry:** Execution timestamp `[yyyy-MM-dd HH:mm:ss.fff]`, inferred module name, query type (`STORED PROCEDURE` vs `QUERY`), procedure name, parameter key-values, status (`SUCCESS` / `FAILED`), execution duration in `ms`, full SQL/procedure execution call, and exception details on error.

**Repositories** own queries. Storefront repositories take `IDataAccessDapper`. Admin catalog repositories take `EcommerceDbContext`. `AdminOrderRepository` takes Dapper.

DI is registered in `ServiceCollectionExtensions.AddCustomServices` in `Program.cs`.

## Frontend

**Storefront:** `_Layout.cshtml` + pages under `views/Home`, `Shop`, `Cart`, `Wishlist`, `Checkout`, `Orders`, `Account`. JS under `wwwroot/js/` (e.g. shop listing).

**Admin:** `_AdminLayout.cshtml` (sidebar + permission attributes on links), `_AdminAuthLayout.cshtml` for login. Pages are mostly one `Index.cshtml` per module with modals and fetch APIs. Login page: `views/Admin/Login/Index.cshtml`.

### SCSS Compilation Pipeline

- **Compiler:** `AspNetCore.SassCompiler` NuGet package executes SCSS compilation during `.NET build`.
- **Source:** `wwwroot/Admin/assets/scss/style.scss` compiles to `wwwroot/Admin/assets/css/style.css`.
- **Dependencies:** Imports Bootstrap SCSS source files from `node_modules/bootstrap/scss/` (requires `npm install` after repository cloning).
- **Component Packaging:** Custom components (Toast, Empty State, presets) are imported into `style.scss` and compiled into a single `style.css` file.

### Admin Shared UI Components

- **Toast Notification System:**
  - Styles: Custom SCSS `wwwroot/Admin/assets/scss/themes/components/_toast.scss` compiled into `style.css`.
  - Script: `wwwroot/Admin/assets/js/common/toast.js` exposing global methods `showSuccess()`, `showError()`, `showWarning()`, `showInfo()`.
  - Layout: `<div id="toastContainer" class="toast-container"></div>` rendered in `_AdminLayout.cshtml`.

- **Empty State Handler:**
  - View Partial & Styles: `views/Shared/_EmptyState.cshtml` and `wwwroot/Admin/assets/scss/themes/components/_empty-state.scss`.
  - Client Handler: `wwwroot/Admin/assets/js/common/empty-state.js` exposing `emptyState` global instance (`showNoData()`, `showError()`, `showInTable()`).
  - Capabilities: Auto-detects network and 500 server errors, renders formatted empty tables with action buttons (Retry, Clear Search, Reset Filters).

## Request flow — storefront shop

1. `GET /Shop` → `ShopController.Index` → Razor.
2. `POST /Shop/GetProducts` with `InputProduct` → `ShopRepository.GetProductListAsync` → SP `GetProducts`.
3. `GET /Shop/GetFilters` → `GetShopFilters`.
4. `GET /Shop/Details/{slug}` page then `GET /Shop/GetProduct/{slug}` → `GetProductDetails`.

## Request flow — customer cart/order

1. JWT cookie read in `JwtBearer` `OnMessageReceived`.
2. Controller uses `UserAuthHelper.TryGetUserId`. If missing → 401 JSON for APIs.
3. Dapper SPs (`AddCartItem`, `PlaceOrder`, …) enforce stock, `SellOnline`, and login.

## Request flow — admin

1. Path starts with `/admin` or `/api/admin`.
2. `AdminAuthMiddleware` requires a valid employee session except login/access-denied.
3. Action `[RequirePermission]` loads current employee permissions from DB via `IAdminAuthInterface.GetCurrentAsync`.
4. Repository uses EF or Dapper as for that module.

## Authentication flow

**Admin:** `POST /api/admin/auth/login` → `AdminAuthRepository` (EF `AdminUsers` + hasher) → `AdminJwtTokenService` → cookie. `GET /api/admin/auth/me` is `[Authorize]`. Logout clears cookie.

**Customer:** `POST /Account/Login` → `GetUserByEmail` + hasher → `UserJwtTokenService` → cookie. Register → `RegisterUser` SP with hashed password.

JWT validation: issuer `JwtSettings.Issuer`, audiences **both** admin `Audience` and `UserAudience` (`SmartCartUser` default). Admin paths prefer the admin cookie; other paths prefer the user cookie, then admin fallback.

## Data flow — catalog vs stock vs orders

- Admin creates `Category` → `SubCategory` → `Brand` → `Products` → `ProductVariants` + `ProductVariantAttributes` + `ProductMedia` / `SkuMedia`.
- `Purchase` + `PurchaseDetail` insert `Stock` batches (`FK_ProductVariant`).
- Inventory adjust inserts another `Stock` row (can use `FK_PurchaseDetail = 0`).
- Storefront prices/stock from variants + summed `Stock.Quantity`.
- `PlaceOrder` writes `Orders`, `OrderItems`, `Payments`, `Shipping`, deducts stock; cancel SPs reverse stock.

## Important dependencies

- SQL Server (LocalDB in default `appsettings.json`)
- Node/`npm install` only for compiling admin SCSS (Bootstrap from `node_modules`)
- `CountryStateCity` HTTP API for supplier India location dropdowns (API key in config)

## Unused / leftover in the tree

- `HomeRepository` / `HomeInterface` are empty stubs; `HomeController` does not use them.
- `GetProductDetailsById` SP is not called from C#.
- Admin `Pro*` catalog SPs are not referenced by current repositories.
- `ProductImages` / `ProductVariantImages` tables still created; live media is `ProductMedia` / `SkuMedia` (SKU images may still go through EF `ProductVariantImages` **and** `SkuMedia` — see admin SKU module notes).
