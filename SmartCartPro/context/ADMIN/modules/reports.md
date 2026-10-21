# Admin Module — Reports

## Module Overview
- **Area:** ADMIN
- **Module Name:** Reports
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Store-scoped operational reports: sales, products, stock value, tax, purchases and customers. Calendar days follow `locale.timezone` (IANA, default `Asia/Kolkata`) and are converted to UTC for queries. Amounts display `locale.currency` (default INR).

## Implemented Features
- `Reports/Index` card hub (Sales, Products, Stock valuation, Tax, Purchases, Customers, plus a link to Inventory/LowStock). The sidebar keeps only **All reports**; each report is opened from this page.
- Sales by day or month: invoices, gross, discounts, tax, net, average bill, returns, payment-method breakdown. Cancelled invoices are excluded from totals and counted separately.
- Products: top N (10/25/50/100) by quantity or revenue, optional category, bar chart.
- Stock valuation: quantity on hand × product cost price (document cost basis, footnoted), category subtotals, include zero/negative filter.
- Tax: real per tax name/rate via sale details → product → tax (no TOTAL placeholder).
- Purchases: by supplier, ordered vs received (received from purchase stock movements), cancelled excluded by default.
- Customers: top spenders, visits, average, last purchase; walk-in is one row.
- Print and UTF-8 BOM CSV (`FileContentResult`). Cells starting with `=+-@` are prefixed with a quote.
- Super Admin (store 0) gets an All-stores dropdown of active stores and never an error. Other users are locked to their claim store.
- Default range is month-to-date. From ≤ To; range capped at 366 days.

## Filters / Search / Sorting
- Query string: `DateFrom`, `DateTo`, `FkStore` (super admin), plus report-specific `GroupBy`, `Top`, `RankBy`, `FkCategory`, `IncludeZeroNegative`, `ExcludeCancelled`.
- Low stock is not duplicated here — link to `Inventory/LowStock`.

## Data
- Entities (repository only): `Sale`, `SaleDetail`, `Payment`, `SalesReturn`, `Product`, `Tax`, `Inventory`, `Purchase`, `PurchaseDetail`, `StockMovement`, `Customer`, `Store`, `Category`.
- Models: `Models/ReportModels.cs` — `InReport*View` / `InputReport*` / `OutputReport*` packed per operation.

## Database Dependencies
- `sales`, `sale_details`, `payments`, `sales_returns`, `products`, `taxes`, `inventory`, `purchases`, `purchase_details`, `stock_movements`, `customers`, `store`, `categories`, `settings` (`locale.timezone`, `locale.currency`).

## API & Data Access
- Controller: `ReportsController` → `IReportService` / `ReportService` → `IReportRepository` / `ReportRepository` (EF LINQ, no Dapper).
- Permissions: `reports.view`. Module `REPORTS`.
- Unused JSON endpoints (`SalesByDate`, `TaxSummary`, `TopSelling`, `LowStock`) were removed. CSV is GET `*Csv`.
- Request orchestrator register: `services.AddScoped<IReportRepository, ReportRepository>();` (keep `IReportService` if already registered). Drop unused `services.AddScoped<IProductReportRepository, ProductReportRepository>();`.

## UI Rules & Conventions
- Views: `Views/Reports/{Index,Sales,Products,StockValuation,Tax,Purchases,Customers}.cshtml`.
- JS: `wwwroot/js/reports/reports.js` + `wwwroot/js/sc-charts.js` (`window.SmartCharts`).
- CSS: `wwwroot/css/reports.css` (print hides filters/actions).
- Charts are SVG (no ApexCharts global, no CDN).

## Business Rules
- Store from claims. Never accept a store id the user cannot access. Super Admin invalid store id falls back to all stores.
- Daily buckets use `locale.timezone`. `TimeZoneInfo.FindSystemTimeZoneById` / `TryFind` with UTC fallback (does not throw).
- Completed, non-cancelled sales drive totals. Held sales are ignored.

## Dependencies & Related Modules
- `dashboard.md`, `inventory.md`, `pos.md`, `invoices.md`, `purchase.md`, `customers.md`, `tax.md`, `settings.md`
