# Admin Module — POS Billing

## Module Overview
- **Area:** ADMIN
- **Module Name:** POS
- **Current Status:** `UI SHELL`

## Purpose & Responsibilities
Point-of-sale billing for the active store.

## Implemented Features
- `Sales/Pos` UI shell (sample cart / pay buttons)

## Filters / Search / Sorting
- Product search planned via Filter API / barcode

## Data
- Tables: `sales`, `sale_details`, `payments`, `cash_sessions`

## Database Dependencies
- Store-scoped; invoice number unique per store

## API & Data Access
- `SalesController.Pos`
- View: `Views/Sales/Pos.cshtml`

## Business Rules
- Require open cash session when that feature is enforced
- Stock deduction via movements (when implemented)

## Dependencies & Related Modules
- `invoices.md`, `inventory.md`, `customers.md`, `filter.md`