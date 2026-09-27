# Admin Module — Dashboard

## Module Overview
- **Area:** ADMIN (Back Office)
- **Module Name:** Dashboard
- **Current Status:** `PARTIALLY IMPLEMENTED`

## Purpose & Responsibilities
Serves as the main administrative home page displaying overall business metrics, sales overview, order statistics, catalog counts, and quick shortcuts.

## Implemented Features
- **Dashboard View Rendering:** `DashboardController.Index` returning Mantis admin template home page.
- **Static Overview Widgets:** Sample metric cards (Sales, Orders, Visitors, Revenue).

## Filters / Search / Sorting
- N/A on current template page.

## Data
- DTOs: Dashboard view metrics.

## Database Dependencies
- Tables: `Orders`, `Products`, `Users`, `Stock` (planned for live metrics).
- Access: EF Core / Dapper.

## API & Data Access
- Controller: `Controllers/Admin/DashboardController.cs` (`[Route("Admin/Dashboard")]`).
- Permission: Protected by `[RequirePermission("Dashboard.View")]`.

## UI Rules & Conventions
- Rendered in `views/Admin/Dashboard/Index.cshtml` using `_AdminLayout.cshtml`.

## Business Rules
- Accessible only to admin employees with `Dashboard.View` permission code.
- **Current Status Note:** Dashboard view uses template sample numbers; live aggregation queries for real-time sales/orders are planned.

## Dependencies & Related Modules
- `ADMIN/modules/auth.md` — Authentication gate.
- `ADMIN/modules/order.md` — Order statistics source.
- `ADMIN/modules/product.md` — Catalog statistics source.
