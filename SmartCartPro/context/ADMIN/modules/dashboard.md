# Admin Module — Dashboard

## Module Overview
- **Area:** ADMIN
- **Module Name:** Dashboard
- **Current Status:** `UI SHELL`

## Purpose & Responsibilities
Home overview: KPIs, recent sales sample, quick actions.

## Implemented Features
- `Home/Index` Metis-styled cards with sample numbers
- Links to POS, products, purchases, online orders, stores

## Filters / Search / Sorting
- N/A

## Data
- Sample static markup (not live queries yet)

## Database Dependencies
- Future: sales, online_orders, inventory aggregates

## API & Data Access
- Controller: `HomeController.Index`
- View: `Views/Home/Index.cshtml`

## UI Rules & Conventions
- Replace sample metrics with real service calls when ready

## Business Rules
- Respect active store context when live metrics exist

## Dependencies & Related Modules
- `pos.md`, `invoices.md`, `inventory.md`, `online-orders.md`