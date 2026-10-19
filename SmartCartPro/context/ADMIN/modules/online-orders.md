# Online Orders — Phase 7

## Status
IMPLEMENTED (Phase 7)

## Tables
- `online_orders` — order header (store-scoped via `fk_store`)
- `online_order_items` — line items (FK → online_orders, products, product_variants)

## Status Flow
```
pending → confirmed → packed → shipped → delivered
                                       → cancelled  (from any non-terminal state)
```

## Stock Integration
- **Confirm** (`pending → confirmed`): deducts stock via `IStockRepository.ApplyMovementAsync("sale")`
  - Reference type: `online_order`, reference ID: order ID
  - Rolls back entire transition if any item fails stock check
- **Cancel** (from `confirmed`, `packed`, `shipped`, or `delivered`): restores stock via movement type `return_in`
  - Rolls back were stock was previously deducted

## Module Access Decision
Module code: **`POS`**
Rationale: No dedicated `ORDERS` module exists in seed data. Online order fulfillment is the same business domain as POS (order → stock → delivery). Update to a dedicated `ORDERS` module when added to seed.

## Files
| Layer | File |
|---|---|
| Entity | `Entities/Order.cs` + `Entities/OnlineOrderItem.cs` |
| Models | `Models/OnlineOrderModels.cs` |
| Repository interface | `Repositories/Interfaces/IOrderRepository.cs` → `IOnlineOrderRepository` |
| Repository impl | `Repositories/OrderRepository.cs` → `OnlineOrderRepository` |
| Service interface | `Services/Interfaces/IOrderService.cs` → `IOnlineOrderService` |
| Service impl | `Services/OrderService.cs` → `OnlineOrderService` |
| Controller | `Controllers/OnlineOrderController.cs` |

## Endpoints
| Method | Route | Permission | Description |
|---|---|---|---|
| GET | `/OnlineOrder` | pos.sell | Paginated list (filter: status, store, date, search) |
| GET | `/OnlineOrder/ListJson` | pos.sell | JSON list |
| GET | `/OnlineOrder/Details/{id}` | pos.sell | Single order with items |
| GET | `/OnlineOrder/DetailsJson/{id}` | pos.sell | JSON detail |
| GET | `/OnlineOrder/UpdateStatus/{id}` | pos.sell | Status change form |
| POST | `/OnlineOrder/UpdateStatus/{id}` | pos.sell | Submit status change |
| POST | `/OnlineOrder/UpdateStatusJson` | pos.sell | JSON status update |

## Dead Scaffold Removal
Old entity-leaking `IOrderRepository` / `OrderRepository` / `IOrderService` / `OrderService`
files have been fully rewritten — entities no longer cross the repository boundary.
