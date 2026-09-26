# SmartCart — Project

SmartCart is a single ASP.NET Core 8 MVC web application (`Ecommerce/`) for a supermarket-style store: a public shop plus an employee admin panel.

## What it is now

One store, one SQL Server database (`SmartCart`). Customers browse products, add SKUs to a cart, wish-list products, and place **Cash on Delivery** orders. Employees manage catalog, suppliers, purchases, stock, and customer orders.

There is **no** multi-store, zone, or tenant model in the running application.

## Main users

| Audience | Who | Entry |
| --- | --- | --- |
| Storefront customer | Rows in `Users` | `/Account/Login`, `/Account/Register` |
| Admin employee | Rows in `AdminUsers` | `/admin/login` |

Customers and employees are **separate** identity stores and JWTs (different cookie names and JWT audiences).

## Current scope (implemented)

**Storefront:** shop list/detail, category/brand/price filters, cart, wishlist, checkout (COD), order list/detail, customer cancel for early statuses, login/register.

**Admin:** catalog (category, subcategory, brand, product, SKU, variant axes/values), suppliers, purchases (creates stock batches), inventory adjust, order list/detail/confirm/ship/deliver/cancel, employees, user roles and permissions.

**Online sell** is per **product** and **SKU** (`SellOnline`, default off). There is no store-wide purchase setting.

## Architecture summary

- ASP.NET Core 8 MVC + Razor views
- SQL Server
- **Storefront + admin orders:** Dapper + stored procedures
- **Admin catalog, purchases, stock, employees, roles, admin login:** Entity Framework Core
- JWT in cookies for both admin and customer
- Admin APIs gated by `RequirePermission`
- Images/videos stored under `wwwroot/uploads/`

## Technology stack (from `Ecommerce.csproj` / `Program.cs`)

- `net8.0` web SDK
- Dapper 2.1.35, Microsoft.Data.SqlClient, EF Core SQL Server 8.0.11
- JWT Bearer 8.0.11, ASP.NET Identity password hasher
- AspNetCore.SassCompiler + npm Bootstrap 5.3 for admin SCSS
- Admin UI: Mantis Bootstrap template
- Storefront: custom CSS (`storefront.css`, `shop-listing.css`, `shop-bag.css`)
- Optional HTTP client `CountryStateCity` for India states/cities on supplier forms

Connection string name in config is `Ecommerse` (spelling as in code).

## Implementation status

| Area | Status |
| --- | --- |
| Shop, cart, wishlist, COD checkout, customer orders | IMPLEMENTED (login required for cart/wishlist/checkout) |
| Admin catalog + inventory + purchases | IMPLEMENTED |
| Admin orders | IMPLEMENTED (no invoice/UPI/admin-created orders) |
| Admin dashboard numbers | PARTIALLY IMPLEMENTED (template/static page) |
| Billing module | PLANNED (seeded permissions; no controller/UI) |
| Guest cart/session checkout | PARTIALLY IMPLEMENTED (`SessionKey` columns exist; C# uses logged-in `UserId` only) |
| UPI / online payment | PLANNED (`PlaceOrder` rejects UPI) |
| Ratings / reviews | NOT a live feature (`Ratings` table exists; shop input fields `Ratings`/`Gender` are not passed to `GetProducts`) |
| Store-wide Settings | Removed; do not reintroduce |

## Relationship between User and Admin

Admin writes catalog and stock. Storefront reads only products/SKUs that are not cancelled, **active**, and **SellOnline**. Orders written by customers are processed by admin (confirm → optional shipped → deliver, or cancel with restock).

## Principles already visible in the code

- Soft delete via `Cancelled` / `CancelledOn` / `CancelledReason` on most admin entities
- Stock is **SKU-level** (`Stock.FK_ProductVariant`), often as purchase batches
- Do not mix storefront Dapper/SP paths with admin EF catalog paths unless the existing module already does (admin orders are the Dapper exception)
- `README.md` / `SETUP.md` still mention old `xPROCEDURExTABLES` paths and omit EF/JWT; **`Ecommerce/Database/` plus this `context/` folder are the source of truth**
