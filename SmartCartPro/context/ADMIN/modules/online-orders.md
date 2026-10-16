# Admin Module — Online orders

## Module Overview
- **Area:** ADMIN
- **Module Name:** Online orders
- **Current Status:** `UI SHELL`

## Purpose & Responsibilities
Fulfill storefront orders assigned to a store.

## Implemented Features
- `Sales/OnlineOrders` sample list
- Entity `Order` mapped to `online_orders` (with `FkStore`)

## Filters / Search / Sorting
- Planned (status, date)

## Data
- `online_orders`, `online_order_items`

## Database Dependencies
- Store-scoped fulfillment

## API & Data Access
- `SalesController.OnlineOrders`
- `IOrderService` / `OrderRepository` (scaffold)
- View: `Views/Sales/OnlineOrders.cshtml`

## Business Rules
- Soft cancel; status workflow TBD (align with Ecommerce when porting)

## Dependencies & Related Modules
- `customers.md`, `product.md`, `inventory.md`
- Future: `USER` storefront modules