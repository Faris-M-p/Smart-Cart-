# Admin Module — Order

## Module Overview
- **Area:** ADMIN (Back Office)
- **Module Name:** Order
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Provides administrative order fulfillment workflow, status updates, order inspection, and administrative order cancellation with automatic stock restock.

## Implemented Features
- **Admin Order Listing:** Paginated list of all customer orders with status filters and keyword search (`GetAdminOrders`).
- **Order Details Inspection:** Displays order header, customer info, shipping address, payment status, and snapshot line items (`GetAdminOrder`).
- **Confirm Order:** Transitions order status from `Placed`/`Pending` to `Confirmed` (`AdminConfirmOrder`).
- **Update Status / Ship:** Transitions order status to `Shipped` (`AdminUpdateOrderStatus`).
- **Deliver Order:** Transitions order status to `Delivered` and marks COD payment as `Paid` (`AdminDeliverOrder`).
- **Admin Cancel Order:** Cancels order if not shipped/delivered, restocks SKU quantities (`AdminCancelOrder`).

## Filters / Search / Sorting
- **Filters:** `OrderStatus` (`Placed`, `Confirmed`, `Shipped`, `Delivered`, `Cancelled`), Date range.
- **Search:** `SearchText` matching OrderNumber, Customer Name, Phone, or Pincode.

## Data
- DTOs: `AdminOrderSummaryView`, `AdminOrderDetailView`, `AdminOrderStatusUpdateInput`, `CommonResponse`.

## Database Dependencies
- Tables: `Orders`, `OrderItems`, `Payments`, `Shipping`, `Stock`.
- Stored Procedures: `GetAdminOrders`, `GetAdminOrder`, `AdminConfirmOrder`, `AdminUpdateOrderStatus`, `AdminDeliverOrder`, `AdminCancelOrder`.
- Access: **Dapper + stored procedures** (`AdminOrderRepository`).

## API & Data Access
- Controller: `Controllers/Admin/OrderController.cs` (`[Route("Admin/Order")]`).
- Interface & Repository: `IAdminOrderInterface.cs` / `AdminOrderRepository.cs`.
- Permissions: `Order.View`, `Order.Confirm`, `Order.Ship`, `Order.Deliver`, `Order.Cancel`.

## UI Rules & Conventions
- Views: `views/Admin/Order/Index.cshtml`, `views/Admin/Order/Details.cshtml`.
- Status badges and action buttons conditioned on current order status.

## Business Rules
- **Order State Machine:** `Placed` / `Pending` $\rightarrow$ `Confirmed` $\rightarrow$ `Shipped` $\rightarrow$ `Delivered`.
- **Shipped is a Status Update:** Shipped is a status API call, not a separate "Ship" module.
- **Delivery Payment Collection:** Marking an order `Delivered` automatically sets payment status to `Paid` for COD.
- **Cancellation Restock:** Cancelling an order restores line item SKU quantities to `Stock`. Orders cannot be cancelled after status reaches `Shipped` or `Delivered`.
- **No Admin Create Order:** `Orders.Create` permission is seeded in DB, but there is **no admin endpoint or UI to create customer orders**.

## Dependencies & Related Modules
- `USER/modules/checkout.md` — Creates customer orders.
- `USER/modules/orders.md` — Customer-facing order view.
- `ADMIN/modules/inventory.md` — Restocked upon cancellation.
