# Admin Module — Inventory / Stock

## Module Overview
- **Area:** ADMIN
- **Module Name:** Inventory
- **Current Status:** `IMPLEMENTED` (server + AJAX UI). Open items are listed under *Notes*.

## Purpose & Responsibilities
Store-scoped stock: on-hand balance per store / product / variant, the append-only movement ledger, opening stock, adjustments, store-to-store transfers, movement history and low-stock replenishment hints.
The signed-in user's store (claim `fk_store`) is always the store being worked on; there is no store switcher.
Purchasing, POS, online orders and returns change stock only through `IStockRepository.ApplyMovementAsync`.

## Implemented Features
- `Inventory/Stock` — on-hand per SKU (product, or one variant of a product) for the signed-in store: search (name, code, SKU, barcode, variant), category filter, status chips **All / Low stock / Out of stock** with counts, sortable columns, page size, paging, empty states, row actions **Adjust / Transfer / History**.
- `Inventory/LowStock` — SKUs at or below their reorder level with shortage and a suggested order quantity.
- `Inventory/History` — movement ledger filtered by product, movement type and date range, paging, times shown in the browser's local time.
- `Inventory/Opening` — first stock entry for a SKU; refused when opening stock already exists for that SKU at the store.
- `Inventory/Adjust` — add or remove stock with a required reason and a live "on hand now → after" preview.
- `Inventory/Transfer` — document-style, several lines, to another active store of the same company; all lines move in ONE database transaction.
- `Inventory/ProductPicker` — JSON typeahead source (`GET ?term=`): one row per sellable SKU with the on-hand quantity at the signed-in store.
- `Inventory` (bare address) redirects to `Inventory/Stock`.
- Platform Super Admin (no store) sees the friendly "Sign in as a store user to use this screen" panel (`NoStore.cshtml`) instead of every screen; mutating POSTs answer 403.
- Opening / Adjust / Transfer buttons and tabs are hidden without `inventory.adjust`.

### Routes and permissions
Module access: `[ModuleAccess("INVENTORY")]`.

| Route | Verb | Permission | Result |
|-------|------|------------|--------|
| `/Inventory` | GET | `inventory.view` | redirect to Stock |
| `/Inventory/Stock` | GET | `inventory.view` | `Stock.cshtml` |
| `/Inventory/LowStock` | GET | `inventory.view` | `LowStock.cshtml` |
| `/Inventory/History` | GET | `inventory.view` | `History.cshtml` |
| `/Inventory/ProductPicker` | GET | `inventory.view` | JSON `{ data: [SKU rows] }` |
| `/Inventory/Opening` | GET / POST | `inventory.adjust` | `Opening.cshtml` / JSON |
| `/Inventory/Adjust` | GET / POST | `inventory.adjust` | `Adjust.cshtml` / JSON |
| `/Inventory/Transfer` | GET / POST (JSON body) | `inventory.adjust` | `Transfer.cshtml` / JSON |

`inventory.manage` (not in the seeded catalog) is no longer used.

## Filters / Search / Sorting
- **Stock:** `Search`, `FkCategory`, `StockStatus` (`low` | `out`), `SortBy` (`productname` | `productcode` | `quantityonhand` | `reorderlevel`), `SortDesc`, `Page`, `PageSize` (UI offers 10–100, server clamps 1–200).
- **LowStock:** `Search`, `FkCategory`, `SortBy` (`productname` | `quantityonhand` | `reorderlevel` | `shortage`), `SortDesc`, `Page`, `PageSize`.
- **History:** `FkProduct` (all variants), `MovementType`, `From`, `To` (local calendar days, `yyyy-MM-dd`, whole days; a reversed range is swapped), `TzOffsetMinutes` (browser `getTimezoneOffset()`), `Page`, `PageSize`.
- Search is case-insensitive (`ILIKE`, wildcards in the term are escaped).

## Data
- Entities (repository only): `Inventory` → `inventory`, `StockMovement` → `stock_movements`; read-only joins to `Product`, `ProductVariant`, `Category`, `Unit`, `Store`, `User`, `Setting`.
- Models: `Models/InventoryModels.cs`, one region per operation:
  - Opening: `InOpeningStockView` → `InputStockOpening`.
  - Adjust: `InStockAdjustView` (`ReasonOptions`) → `InputStockAdjust`.
  - Transfer: `InStockTransferCreateView` + `InStockTransferLineView` → `InputStockTransferCreate` + `InputStockTransferLine` → `OutputStockTransferCreate`.
  - Lists: `InputInventoryList` / `OutputInventoryList`, `InputLowStockList` / `OutputLowStockList`, `InputStockMovementList` / `OutputStockMovementList` (+ `OutputStockMovementItem`).
  - Single SKU: `InputInventorySelect` / `OutputInventorySelect` (computed `AvailableQuantity`, `StockStatus`).
  - Picker / lookups: `InputStockProductSearch` / `OutputStockProductSearch` (+ item), `OutputStockLookupItem` (categories, destination stores).
  - Kept for other modules: `InputStockMovement` / `OutputStockMovement` (central write path), `InputStockTransfer` / `OutputStockTransfer` (single-line transfer).
- No `*Dto` classes; `DataAnnotations` only on `In…View` models.

## Database Dependencies
- `inventory` — unique per store / product / variant; CHECK `ck_inventory_qty` (`quantityonhand >= 0`).
- `stock_movements` — CHECK on `movementtype`: `purchase | sale | adjustment | transfer_in | transfer_out | return_in | return_out | opening`.
- Setting `pos.allow_negative_stock` (store scope wins over company scope; missing = false).
- No schema change was made for this module.

## API & Data Access
- `InventoryController` → `IStockService` / `StockService` → `IStockRepository` / `StockRepository` (EF LINQ only).
- `ApplyMovementAsync` (signature unchanged): validates type, quantity and variant; joins the caller's open transaction (`Database.CurrentTransaction`) or opens and commits its own; changes the balance with ONE guarded `UPDATE … WHERE quantityonhand + delta >= 0`, so two concurrent sales cannot both pass the stock check; writes the movement row.
- `CreateTransferAsync` — every line is a `transfer_out` (source) + `transfer_in` (destination, `referenceid` = the out movement, `referencetype` = `transfer`) inside one transaction; any failing line rolls back the whole transfer (audit item SC-SRV-004).
- `TransferAsync` (single line, used by older callers) delegates to `CreateTransferAsync`.
- Service adds: reorder-level suggestions, category lookups, UTC bounds for the history date filter, duplicate-SKU merge on transfers (max 100 lines), opening-stock-once rule.
- Failures are typed exceptions through `AppExceptionHandler`: 403 no store, 404 SKU not found, 409 opening already recorded, 422 insufficient stock / invalid transfer.

## UI Rules & Conventions
- Views in `Views/Inventory/`: `Stock`, `LowStock`, `History`, `Opening`, `Adjust`, `Transfer`, plus module partials `_InventoryTabs` (section pills) and `NoStore`.
- JS in `wwwroot/js/inventory/` on top of `sc-ajax.js`: `inventory-common.js` (`window.ScInventory`: product picker, quantity format, local times, busy state on list filters), `stock-form.js` (Opening / Adjust), `transfer.js`, `history.js`.
- Product pickers use `SmartCartAjax.typeahead` against `Inventory/ProductPicker`; a chosen row fills hidden `FkProduct` / `FkProductVariant`, typing over it clears them. The server re-reads the SKU and the on-hand quantity — nothing sent by the browser is trusted.
- Opening and Adjust are AJAX forms (`submitForm`): field errors under the inputs, success toast and redirect to the list the user came from (`returnUrl`, local URLs only) or to Stock.
- Transfer posts a JSON document with the antiforgery header (`postJson`), asks for confirmation first and redirects to History.
- History timestamps are rendered as `<time data-sc-local-time datetime="…Z">` and localised in the browser; the From / To days are the user's local days (offset sent as `TzOffsetMinutes`).
- All dynamic text is written with `.text()` / Razor encoding; no `Html.Raw` on data.

## Business Rules
- The store comes from the signed-in user's claims; a store id sent by the browser is ignored.
- Quantities have at most 3 decimals; stored balance is never below zero (DB CHECK).
- **Opening stock** is the first entry for a SKU at a store, once; later corrections are adjustments (409 otherwise).
- **Adjust:** direction `add` or `remove`, reason required (Damaged, Expired, Lost or stolen, Found stock, Count correction, Other); the movement note is `Reason: free text`; removing more than is on hand is refused.
- **Transfer:** destination must be an active store of the same company and different from the source; each line is checked against the source's on-hand quantity.
- Movement quantity is stored positive for every type except `adjustment` (signed); outbound types are `sale`, `transfer_out`, `return_out`.
- **Reorder level:** the store row's own level when above zero, otherwise the product's level. Status: *out* = on hand ≤ 0, *low* = above 0 and ≤ level, otherwise *ok*.
- **Suggested order** = `ceil(2 × reorder level − on hand)`; blank when no reorder level is set.
- **Negative stock:** only a `sale` may go below zero, and only when `pos.allow_negative_stock` is true (see Notes).
- Movements are append-only; no hard deletes.

## Dependencies & Related Modules
- `product.md`, `variant.md`, `category.md`, `unit.md`, `stores.md`, `settings.md`, `roles.md`
- Callers of `ApplyMovementAsync`: `purchase.md`, `pos.md`, `online-orders.md`, `returns.md`; low-stock report in `reports.md`.

## Notes
- **Negative stock needs a SQL patch.** The service honours `pos.allow_negative_stock`, but table CHECK `ck_inventory_qty` still rejects a negative balance. Until the CHECK is relaxed the sale fails with a friendly "Insufficient stock … ask your administrator to apply the stock patch" message instead of a server error.
- The per-store reorder level (`inventory.reorderlevel`) has no editing screen; rows inherit the product's level.
- The first movement ever recorded for a SKU creates its `inventory` row; two such first movements at the very same moment could collide on the unique index (rare, not retried).
- The sidebar shows one **Inventory** entry (Stock); the other screens are reached through the section pills at the top of every Inventory page.
- Stock valuation and the all-stores low-stock report stay in the Reports module.
