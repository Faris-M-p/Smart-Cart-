# Admin Module — Sales invoices

## Module Overview
- **Area:** ADMIN
- **Module Name:** Invoices
- **Current Status:** `UI SHELL`

## Purpose & Responsibilities
List / view POS and manual sales invoices for a store.

## Implemented Features
- `Sales/Invoices` sample table

## Filters / Search / Sorting
- Planned (date, status, customer)

## Data
- `sales`, `sale_details`, `payments`

## Database Dependencies
- Store-scoped (`fk_store`)

## API & Data Access
- `SalesController.Invoices`
- View: `Views/Sales/Invoices.cshtml`

## Business Rules
- Soft cancel; no cascade delete of payment history

## Dependencies & Related Modules
- `pos.md`, `returns.md`, `customers.md`