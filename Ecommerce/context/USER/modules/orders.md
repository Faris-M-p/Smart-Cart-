# User Module — Orders

## Module Overview
- **Area:** USER (Storefront)
- **Module Name:** Orders
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Provides customer order history, order details view, current order status tracking, and early order cancellation by the customer.

## Implemented Features
- **Order History:** Lists all orders placed by the logged-in customer sorted by date descending (`GetOrders`).
- **Order Details:** Displays line items, snapshot prices, shipping address, payment status, and delivery tracking status (`GetOrder`).
- **Customer Order Cancellation:** Allows customer to cancel an order if it is still in `Placed` or `Pending` status. Restocks SKU inventory automatically (`CancelOrder`).

## Filters / Search / Sorting
- **Sorting:** Default order date descending.
- **Filter:** `UserId` scoping inside stored procedures.

## Data
- DTOs: `OrderSummaryView`, `OrderDetailView`, `OrderItemSnapshotView`, `CommonResponse`.

## Database Dependencies
- Tables: `Orders`, `OrderItems`, `Payments`, `Shipping`, `Stock`.
- Stored Procedures: `GetOrders`, `GetOrder`, `CancelOrder`.

## API & Data Access
- Controller: `Controllers/OrdersController.cs` (`Index`, `GetOrders`, `Details`, `GetOrderDetails`, `Cancel`).
- Repository: `OrderRepository.cs` via `IDataAccessDapper`.

## UI Rules & Conventions
- History page: `views/Orders/Index.cshtml`.
- Details page: `views/Orders/Details.cshtml` with status timeline badges and line item table.

## Business Rules
- **Requires Customer Authentication:** `UserId` extracted from JWT cookie. Stored procedures enforce `UserId = @UserId` ownership check.
- **Customer Cancellation Restriction:** Customers can ONLY cancel orders in `Placed` or `Pending` status. Once an order is updated to `Confirmed`, `Shipped`, or `Delivered` by Admin, customer cancellation is blocked.
- Cancellation automatically restocks SKU quantities in `Stock`.

## Dependencies & Related Modules
- `USER/modules/checkout.md` — Creates orders.
- `ADMIN/modules/order.md` — Admin order processing and status updates.
