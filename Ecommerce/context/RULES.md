# SmartCart — Master development rules

These rules describe how the **current** codebase is structured. Follow them for new work. Do not “fix” naming or data-access style just to look consistent.

## Context maintenance (permanent)

Whenever a new feature or module is added, or an existing architecture, rule, or database behavior changes:

1. **Update the relevant context file** (`PROJECT.md`, `ARCHITECTURE.md`, `DATABASE.md`, `RULES.md`, `FUTURE.md`) immediately.
2. **Create a new module context file** under `context/USER/modules/` or `context/ADMIN/modules/` when a new module is created.
3. **Keep User and Admin documentation strictly separate**, even when they operate on shared tables.
4. **Keep database documentation synchronized** with actual SQL scripts and database schema.
5. **Do not create duplicate project documentation elsewhere** (such as `findings/`, `docs/`, `decisions/`, or temporary logs). `context/` is the single source of truth.
6. **Context documentation must be updated as part of the same development task** — do not defer documentation updates.

### Module Documentation Standard Sections

When creating or updating a module context file, include:
- Module Name & Area (USER or ADMIN)
- Purpose & Responsibilities
- Current Status (`IMPLEMENTED` / `PARTIALLY IMPLEMENTED` / `PLANNED`)
- Implemented Features
- Filters / Search / Sorting (only what is actually implemented)
- Data (Entities / DTOs)
- Database dependencies (Tables & SPs / EF mappings)
- API & Data Access (Controllers, Repositories, Endpoints)
- UI Rules & Conventions
- Business Rules
- Dependencies & Related Modules
- Notes

## Solution layout

- App project: `Ecommerce/`
- Database scripts: `Ecommerce/Database/` (`RunDatabase.bat` full create, `RunPatch.bat` incremental)
- Knowledge & Specs: `context/` folder (single source of truth)

## Data access (do not invert)

| Area | Access |
| --- | --- |
| Storefront shop, cart, wishlist, customer auth, customer orders/checkout | Dapper + stored procedures via `IDataAccessDapper` |
| Admin catalog, suppliers, purchases, inventory, employees, user roles, admin login | EF Core (`EcommerceDbContext`) |
| Admin orders | Dapper + SPs (`GetAdminOrders`, `AdminConfirmOrder`, …) |

Many `Pro*` admin catalog stored procedures still exist under `Database/02_Procedures/01_Admin/` but **current C# does not call those `Pro*` names**. Do not assume a new admin list must use those SPs; match the existing module (EF vs Dapper).

## Naming

**Newer / admin-style tables:** `ID_TableName` PK, `FK_Other` FKs (`ID_Product`, `FK_SubCategory`).

**Legacy storefront/order tables:** `Orders.OrderId`, `Users.UserId`, `Cart.CartId`, `Payments.PaymentId`, `Shipping.ShippingId`. Do not rename them in app code.

**SQL objects:** procedures named by action (`GetProducts`, `PlaceOrder`, `AdminDeliverOrder`). Older admin scripts use `Pro{Entity}{Action}` (`ProCategoryListSelect`).

**C#:** controllers in `Controllers/` (storefront) and `Controllers/Admin/`; interfaces `ShopInterface` (storefront, no `I` prefix) vs `ICategoryInterface` (admin); repositories match.

**Config:** connection string key is `Ecommerse`.

## Database scripts

- Incremental table patches must be **safe to re-run** (`IF OBJECT_ID` / `IF COL_LENGTH`).
- Put new columns on the **existing table script** used by Patch (same pattern as `Category.ImageUrl` in `Categories.sql`). Do **not** add one-off `*_AddColumn.sql` files for a single `IF COL_LENGTH` unless that is already how that table is patched.
- Full `CREATE TABLE` without `IF NOT EXISTS` belongs in create-only flows, not blindly in `01_Tables/Patch.sql`.
- Apply schema with `RunPatch.bat` / `RunDatabase.bat`; do not invent a second schema source.

## Soft delete

Admin entities typically set `Cancelled = 1` instead of hard delete. Storefront queries filter `Cancelled = 0`. Order cancel also sets order status and restocks.

## Online sell

- Product: shop-visible only if `IsActive = 1` AND `SellOnline = 1` AND not cancelled.
- SKU: same on `ProductVariants`.
- Default `SellOnline` is **0**. Do not add a global `StoreSettings.AllowOnlinePurchase` gate.

## Stock and orders

- Quantity lives on `Stock` rows keyed by `FK_ProductVariant` (batch rows from purchases; inventory adjust inserts additional rows).
- Order lines store both `FK_Product` and `FK_ProductVariant`.
- Deduct/restock in order SPs by SKU, not by product header.
- Admin must not create customer orders or edit line items (not implemented; do not add unless requested).
- Shipped is a **status update**, not a Ship module.

## Authentication and security

- Admin: cookie `AdminAuthHelper.TokenCookieName`, JWT audience admin; `AdminAuthMiddleware` on `/admin` and `/api/admin`.
- Customer: cookie `UserAuthHelper.TokenCookieName`, JWT `UserAudience`.
- Admin actions: `[RequirePermission("Module.Action")]`; permissions come from role mappings, re-read on the request (not trusted from the client alone).
- Passwords: Identity `PasswordHasher` (not reversible).
- Do not commit real production secrets. `appsettings.json` currently holds a **development** JWT key and optional location API key.

## API / UI conventions

**Admin lists:** Razor Index + AJAX POST `Get{Entity}List` returning `ApiResponse<TableOutput<T>>` with `PageIndex` / `PageSize` / `SearchText`. Shared pagination, empty-state, toast, searchable selects, `admin-permissions.js` for buttons.

**Storefront:** Razor pages + JSON from MVC actions (`Shop/GetProducts`, `Cart/Add`, …). Login required for cart/wishlist/checkout/orders.

**Images:** `CommonImageService` — jpg/jpeg/png/webp, max 2 MB; product also allows one video (mp4/webm, 20 MB); max 5 product media items. List thumbs use `list-entity-cell`.

**Validation:** DataAnnotations on `*VIEW` inputs; repositories/SPs return `CommonResponse` (`StatusCode`, `ResponseCode`, `ResponseMsg`).

## Error handling

`GlobalExceptionMiddleware` and `RequestLoggingMiddleware` wrap the pipeline. Admin list actions often `throw` after catch so the global handler runs.

## Do-not-break

- Do not switch an existing module from EF to Dapper or the reverse without an explicit product decision.
- Do not use `Orders.Create` as “admin placed this order” — permission exists in seed; **no admin create-order UI**.
- Do not treat Billing, Ratings, AuditLogs, ProductStatus, or leftover `ProductImages` as live features unless you implement and document them.
- Do not auto-run `dotnet build` / `dotnet run` unless the user asks; they often apply SQL with `RunPatch.bat` themselves.

## Folder conventions for new code

- Storefront: `Controllers/`, `Repository/`, `Interface/`, `views/{Controller}/`
- Admin: `Controllers/Admin/`, `Repository/Admin/`, `Interface/Admin/`, `views/Admin/{Module}/`, `Models/Admin/`
- EF entities: `Models/Entities/`
- SQL: `Database/01_Tables/`, `Database/02_Procedures/01_Admin|02_Users|03_Common/`, `Database/05_Seed/`
