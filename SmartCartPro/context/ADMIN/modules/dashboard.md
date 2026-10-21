# Admin Module — Dashboard

## Module Overview
- **Area:** ADMIN
- **Module Name:** Dashboard
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Signed-in home (`Home/Index`) and live refresh (`Dashboard/Snapshot`). Every authenticated user can open Home; `dashboard.view` is not required on that action.

## Implemented Features
- **Store dashboard** (`dashboard.view` + store claim): KPIs for sales today (vs yesterday %), month-to-date, low stock (`qty <= reorder` when reorder &gt; 0, else `qty <= 0`), pending POs (`posted` + `partially_received`), pending online orders, open cash sessions. Charts: 7/30-day net trend, payment donut (MTD), top 5 products (MTD). Tables: recent invoices (link `Sales/InvoiceDetails`) and low stock (link `Inventory/LowStock`). Quick actions gated by permission + enabled module. First paint is server-rendered. Refresh button + optional 60s poll while the tab is visible (`Dashboard/Snapshot` JSON envelope).
- **Welcome**: users without `dashboard.view` (or without a store) see a welcome message and allowed quick actions only — never an error.
- **Super Admin setup** (no store yet): company / create store / license / modules. Home does not load or gate on license. After a store is selected, Super Admin gets the full store dashboard.

## Filters / Search / Sorting
- N/A. Business day bounds use `locale.timezone`.

## Data
- Models: `Models/DashboardModels.cs` — `InputDashboardSelect` / `OutputDashboardSelect`, `InputDashboardSnapshot` / `OutputDashboardSnapshot`.
- Entities stay in repositories: `Sale`, `Payment`, `Inventory`, `Purchase`, `Order`, `CashSession`, `Product`, `Customer`.

## Database Dependencies
- `sales`, `payments`, `inventory`, `purchases`, `online_orders`, `cash_sessions`, `sale_details`, `products`, `customers`, `license`, `store`, `settings`.

## API & Data Access
- `HomeController.Index` → `IDashboardService.GetHomeAsync` (no `dashboard.view`, no `DASHBOARD` module gate).
- `DashboardController.Snapshot` → `IDashboardService.GetSnapshotAsync` — `[RequirePermission("dashboard.view")]` `[ModuleAccess("DASHBOARD")]`, returns `CommonResponseModel`.
- `DashboardService` → `IDashboardRepository` / `DashboardRepository` (EF LINQ). Locale via `ISettingService`.
- `DashboardController` does not inject `SmartCartDbContext`.
- Request orchestrator register:
  - `services.AddScoped<IDashboardService, DashboardService>();`
  - `services.AddScoped<IDashboardRepository, DashboardRepository>();`

## UI Rules & Conventions
- View: `Views/Home/Index.cshtml`.
- JS: `wwwroot/js/dashboard/dashboard.js` + `wwwroot/js/sc-charts.js` (`window.SmartCharts`: bars / line / donut; SVG viewBox; title tooltips; `role=img`; visually-hidden table fallback; Bootstrap CSS variables). No CDN. No `window.bootstrap`.
- CSS: `wwwroot/css/dashboard.css`.
- Timestamps: `<time data-sc-utc="@TimeDisplay.Iso(...)">`.

## Business Rules
- Store from claims. Super Admin with no store never sees a store-required error.
- Today / yesterday / month are computed in `locale.timezone` then converted to UTC for queries. Unknown IANA ids fall back to UTC without throwing.
- Low stock is listed here and linked to Inventory; reports do not duplicate the full low-stock list.

## Dependencies & Related Modules
- `reports.md`, `pos.md`, `invoices.md`, `inventory.md`, `online-orders.md`, `purchase.md`, `license.md`, `modules.md`, `stores.md`, `settings.md`
