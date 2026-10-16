# Admin Module — Purchase

## Module Overview
- **Area:** ADMIN
- **Module Name:** Purchase
- **Current Status:** `UI SHELL`

## Purpose & Responsibilities
Store-scoped purchase receipts (`fk_store`).

## Implemented Features
- `Inventory/Purchases` sample list

## Filters / Search / Sorting
- Planned (date, supplier, store)

## Data
- Tables: `purchases`, `purchase_details`

## Database Dependencies
- Store-scoped; supplier + product lines

## API & Data Access
- `InventoryController.Purchases`
- View: `Views/Inventory/Purchases.cshtml`

## Business Rules
- Soft cancel; do not cascade-delete history
- Receiving store must match `fk_store`

## Dependencies & Related Modules
- `supplier.md`, `product.md`, `inventory.md`, `stores.md`