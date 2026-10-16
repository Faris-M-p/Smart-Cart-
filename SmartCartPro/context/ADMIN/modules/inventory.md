# Admin Module — Inventory / Stock

## Module Overview
- **Area:** ADMIN
- **Module Name:** Inventory
- **Current Status:** `UI SHELL` (entity mapped)

## Purpose & Responsibilities
On-hand stock per store / product / variant.

## Implemented Features
- `Inventory/Stock` sample list
- Entity `Inventory` with `FkStore`

## Filters / Search / Sorting
- Planned (store, low stock)

## Data
- Entity: `Inventory` → `inventory`
- Movements: `stock_movements`

## Database Dependencies
- Store-scoped balances; reorder levels

## API & Data Access
- `InventoryController.Stock`
- View: `Views/Inventory/Stock.cshtml`

## Business Rules
- Never silent cascade deletes on movements
- Quantities >= 0 (check constraints)

## Dependencies & Related Modules
- `product.md`, `variant.md`, `purchase.md`, `pos.md`, `stores.md`