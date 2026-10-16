# Admin Module — Reports

## Module Overview
- **Area:** ADMIN
- **Module Name:** Reports
- **Current Status:** `UI SHELL`

## Purpose & Responsibilities
Sales, product, and customer reporting screens.

## Implemented Features
- `Reports/Sales`, `Reports/Products`, `Reports/Customers` sample tables
- Dapper scaffold: `IProductReportRepository` (example aggregate)

## Filters / Search / Sorting
- Date range UI on sales report (sample)

## Data
- Report DTOs as needed; prefer Dapper for heavy aggregates

## Database Dependencies
- Reads from sales / products / customers (no schema ownership)

## API & Data Access
- `ReportsController`
- Views: `Views/Reports/*`

## Business Rules
- Scope reports by allowed stores when auth exists

## Dependencies & Related Modules
- `invoices.md`, `product.md`, `customers.md`