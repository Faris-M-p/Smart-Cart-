# Admin Module — Dashboard

## Module Overview
- **Area:** ADMIN (Back Office)
- **Module Name:** Dashboard
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Administrative home page showing live store metrics: online orders/revenue, catalogue counts, customers, POS revenue, order trends, status mix, top products, recent orders, and low-stock alerts.

## Implemented Features
- **Dashboard view:** `DashboardController.Index` with Mantis layout cards and ApexCharts.
- **Live summary API:** `GET /Admin/Dashboard/GetSummary` returns KPIs and chart series from EF Core aggregations.
- **Charts:** 14-day order/revenue trend, order status donut, online vs counter sales bars, top products by quantity.
- **Tables/lists:** recent orders, low-stock SKUs (threshold 10, same as inventory reorder default).
- **Refresh:** client-side refresh button reloads summary JSON.

## Filters / Search / Sorting
- Fixed 14-day trend window; top 8 products; top 8 low-stock rows; latest 8 orders.

## Data
- DTOs in `Models/Admin/DashboardModel.cs` (`DashboardSummary`, KPIs, trend points, recent orders, low stock).

## Database Dependencies
- Tables (EF): `orders`, `orderitems`, `products`, `productvariants`, `stock`, `users`, `sales`.
- Access: EF Core via `EcommerceDbContext` (`DashboardRepository`).

## API & Data Access
- Controller: `Controllers/Admin/DashboardController.cs`
- Interface: `IDashboardInterface`
- Repository: `Repository/Admin/DashboardRepository.cs`
- Permission: `[RequirePermission("Dashboard.View")]`
- Endpoints:
  - `GET /Admin/Dashboard` / `Index` — page
  - `GET /Admin/Dashboard/GetSummary` — JSON summary

## UI Rules & Conventions
- View: `views/Admin/Dashboard/Index.cshtml`
- Script: `wwwroot/Admin/assets/js/pages/dashboard-smartcart.js` (page Scripts section only)
- ApexCharts loaded from admin layout; fake `dashboard-default.js` sample metrics removed from global layout load.

## Business Rules
- Online KPIs exclude cancelled orders (`cancelled != true`).
- Pending = statuses `Placed`, `Pending`, or `Confirmed`.
- Low stock = summed non-cancelled `stock.quantity` per SKU ≤ 10.
- Currency display uses INR formatting on the client.

## Dependencies & Related Modules
- Orders, Inventory, Products, Sales, Homepage (quick links).
- Auth / `Dashboard.View` permission.

## Notes
- Replaces previous Mantis template placeholder numbers with live SmartCart data.
