# Admin Module — Online Orders

## Module Name & Area
- **Area:** ADMIN
- **Module Name:** Online orders (POS)
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Fulfill storefront orders for the signed-in store: list, inspect, and advance status. Confirm deducts stock; cancel restores it. Both run in one transaction with `IStockRepository.ApplyMovementAsync`.

## Implemented Features
- Index filters (status, dates, search order number / customer) + sort + pager
- StoreRequired when `fk_store` is 0 (platform Super Admin)
- Details document view with **allowed next statuses from the output model** (not hard-coded in the view)
- Confirm copy explains stock deduction; cancel copy explains restore when stock was already deducted
- JSON list/detail/status endpoints return `CommonResponseModel`

## Status Flow
```
pending → confirmed → packed → shipped → delivered
                                       → cancelled  (from any non-terminal)
```
`OutputOnlineOrderSelect.AllowedNextStatuses` is the source of truth for buttons. `StockWasDeducted` drives the cancel stock message.

## Stock Integration
- **Confirm** (`pending → confirmed`): `ApplyMovementAsync("sale")`, reference `online_order` / order id. Any line failure rolls back the whole transition.
- **Cancel** from confirmed/packed/shipped/delivered: `ApplyMovementAsync("return_in")` in the same transaction.

## Filters / Search / Sorting
- `Status`, `From`, `To`, `Search`, `SortBy`, `SortDesc`, `Page`, `PageSize`
- Sort: `orderdate` (default desc), `ordernumber`, `status`, `totalamount`, `customername`

## Data
- `InputOnlineOrderList` / `OutputOnlineOrderList` / `OutputOnlineOrderListItem`
- `InputOnlineOrderSelect` / `OutputOnlineOrderSelect` / `OutputOnlineOrderItem`
- `InOnlineOrderStatusView` / `InputOnlineOrderStatusUpdate` / `OutputOnlineOrderStatusUpdate`
- Entities `Order`, `OnlineOrderItem` stay in the repository

## Database Dependencies
- `online_orders` (store-scoped), `online_order_items`, `customers`, `products`, `product_variants`, stock movements

## API & Data Access
| Method | Route | Permission | Description |
|---|---|---|---|
| GET | `/OnlineOrder` | pos.sell | Paged list (store-scoped) |
| GET | `/OnlineOrder/ListJson` | pos.sell | AJAX envelope list |
| GET | `/OnlineOrder/Details/{id}` | pos.sell | Document view |
| GET | `/OnlineOrder/DetailsJson/{id}` | pos.sell | AJAX envelope detail |
| GET | `/OnlineOrder/UpdateStatus/{id}` | pos.sell | Redirects to Details |
| POST | `/OnlineOrder/UpdateStatus/{id}` | pos.sell (+ **pos.cancel** when NewStatus=cancelled) | Status change |
| POST | `/OnlineOrder/UpdateStatusJson` | pos.sell (+ pos.cancel for cancel) | JSON status change |

Controller: `[ModuleAccess("POS")]`. Cancel is not a separate route; the action checks `pos.cancel` before applying `cancelled`.

## UI Rules & Conventions
- Purchase-style list + document details
- Status buttons: `data-sc-post` with confirm text that explains stock
- Partials: `_OnlineOrderStatusBadge`, `StoreRequired`
- JS: `wwwroot/js/orders/online-order.js`
- Do **not** link to `Views/Sales/OnlineOrders.cshtml` (removed by Agent A)

## Business Rules
- Store-scoped; Super Admin sees StoreRequired
- Allowed next statuses come from the output model
- Soft cancel (`cancelled` + reason); no hard delete
- Stock confirm/cancel in one DB transaction

## Dependencies & Related Modules
- `pos.md`, `customers.md`, `inventory.md`

## Notes
- File names remain `OrderService` / `OrderRepository` / `IOrderService.cs` / `IOrderRepository.cs` while types are `IOnlineOrderService` / `IOnlineOrderRepository`.
- No print view in this pass.
