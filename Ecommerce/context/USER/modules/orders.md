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
- **Per-item Ratings & Reviews (Order Details):** every item of a **Delivered** order gets a review slot (quick stars + **Add Review**, then **Edit Review** / **Delete Review**, then "Review period ended" once 7 days after delivery have passed). One review per order item, regardless of quantity. The page loads the item states from `GET /Reviews/Order?orderId=` after `GetOrder` renders (`wwwroot/js/shop-order-reviews.js`; `shop-orders.js` fires the `smartcart:order-loaded` event). Full rules: `USER/modules/reviews.md`.

## Filters / Search / Sorting
- **Sorting:** Default order date descending.
- **Filter:** `UserId` scoping inside stored procedures.

## Data
- DTOs: `OrderSummaryView`, `OrderDetailView`, `OrderItemSnapshotView`, `CommonResponse`. `OrderPage.Items` is `List<OrderLine>` (`OrderLine : CartLine` + `OrderItemId`, selected by `get_order`) so the UI can attach a review slot to each order item.

## Database Dependencies
- Tables: `Orders`, `OrderItems`, `Payments`, `Shipping`, `Stock`.
- Stored Procedures: `GetOrders`, `GetOrder`, `CancelOrder`.

## API & Data Access
- Controller: `Controllers/OrdersController.cs` (`Index`, `GetOrders`, `Details`, `GetOrderDetails`, `Cancel`).
- Repository: `OrderRepository.cs` via `IDataAccessDapper`.

## UI Rules & Conventions
- History page: `views/Orders/Index.cshtml`.
- Details page: `views/Orders/Details.cshtml` with status timeline badges and line item table, plus the review modal (`#reviewModal`) and delete confirmation (`#reviewDeleteModal`); styles in `wwwroot/css/shop-reviews.css`.

## Business Rules
- **Requires Customer Authentication:** `UserId` extracted from JWT cookie. Stored procedures enforce `UserId = @UserId` ownership check.
- **Customer Cancellation Restriction:** Customers can ONLY cancel orders in `Placed` or `Pending` status. Once an order is updated to `Confirmed`, `Shipped`, or `Delivered` by Admin, customer cancellation is blocked.
- Cancellation automatically restocks SKU quantities in `Stock`.

## Dependencies & Related Modules
- `USER/modules/checkout.md` — Creates orders.
- `ADMIN/modules/order.md` — Admin order processing and status updates.
