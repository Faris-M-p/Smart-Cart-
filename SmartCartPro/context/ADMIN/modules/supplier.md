# Admin Module — Supplier

## Module Overview
- **Area:** ADMIN
- **Module Name:** Supplier
- **Current Status:** `UI SHELL`

## Purpose & Responsibilities
Vendor master for purchasing.

## Implemented Features
- `Inventory/Suppliers` sample list

## Filters / Search / Sorting
- Planned

## Data
- Table: `suppliers`

## Database Dependencies
- `suppliers`; purchases use `fk_supplier`

## API & Data Access
- `InventoryController.Suppliers`
- View: `Views/Inventory/Suppliers.cshtml`

## Business Rules
- Soft cancel; company-wide

## Dependencies & Related Modules
- `purchase.md`