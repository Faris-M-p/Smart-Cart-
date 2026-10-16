# Admin Module — Sales returns

## Module Overview
- **Area:** ADMIN
- **Module Name:** Returns
- **Current Status:** `UI SHELL`

## Purpose & Responsibilities
Returns against original store invoices.

## Implemented Features
- `Sales/Returns` sample list

## Filters / Search / Sorting
- Planned

## Data
- `sales_returns`, `sales_return_details`

## Database Dependencies
- Store-scoped; `fk_sale` → `sales`

## API & Data Access
- `SalesController.Returns`
- View: `Views/Sales/Returns.cshtml`

## Business Rules
- Restock via stock movements when implemented

## Dependencies & Related Modules
- `invoices.md`, `inventory.md`