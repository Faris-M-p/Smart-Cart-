# Admin Module — Inventory / Stock

## Module Overview
- **Area:** ADMIN
- **Module Name:** Inventory
- **Current Status:** `IMPLEMENTED` (server + server-rendered UI with AJAX saves; opening stock, adjustment, transfer, history, low stock). See *Notes* for the known gaps.

## Purpose & Responsibilities
Store-scoped on-hand stock per product / variant and the immutable ledger behind it (`stock_movements`).
Staff of a store see what is on the shelves, record opening stock, correct it with adjustments, move stock to another store of the company, trace every change and see what must be re-ordered.
`StockRepository.ApplyMovementAsync` is also the **single write gateway** used by purchasing, POS, online orders and returns.

## Implemented Features
| Route | Action → View | Permission | What it does |
|-------|---------------|------------|--------------|
| `GET /Inventory` | `Index` → redirect to `Stock` | `inventory.view` | Keeps old links working |
| `GET /Inventory/Stock` | `Stock` → `Stock.cshtml` | `inventory.view` | Balance list: search, category, All / Low / Out chips with counts, sortable columns, paging, row actions (Adjust, Transfer, History) |
| `GET /Inventory/LowStock` | `LowStock` → `LowStock.cshtml` | `inventory.view` | Rows at or below the reorder level with shortage and suggested order quantity |
| `GET /Inventory/History` | `History` → `History.cshtml` | `inventory.view` | Movement ledger: product, type, date range, paging, signed quantities, local times |
| `GET/POST /Inventory/Opening` | `Opening` → `Opening.cshtml` | `inventory.adjust` | First stock entry of one SKU at the store |
| `GET/POST /Inventory/Adjust` | `Adjust` → `Adjust.cshtml` | `inventory.adjust` | Add or remove stock with a reason, live "now → after" preview |
| `GET/POST /Inventory/Transfer` | `Transfer` → `Transfer.cshtml` | `inventory.adjust` | Document-style transfer (many lines) to another store; POST is a JSON body |
| `GET /Inventory/ProductPicker?term=` | `ProductPicker` (JSON) | `inventory.view` | Typeahead source: one row per sellable SKU with on-hand at the store |

- Rows, buttons and tabs for `inventory.adjust` are hidden from users without it (`Access.CanAsync`).
- Platform users without a store (Platform Super Admin) get the friendly *"Sign in as a store user to use this screen"* panel (`NoStore.cshtml`) on every screen; mutating POSTs answer 403.
- Successful saves flash a toast and return to the list the user came from (`returnUrl`, local URLs only) or to `Stock` / `History`.

## Filters / Search / Sorting
- **Stock:** `Search` (name, code, SKU, barcode, variant name / SKU / barcode, case-insensitive, `%`/`_` escaped), `FkCategory`, `StockStatus` (`""` all · `low` · `out`), `SortBy` (`productname` · `productcode` · `quantityonhand` · `reorderlevel`), `SortDesc`, `Page`, `PageSize` (10/25/50/100, server clamps 1–200).
- **Low stock:** `Search`, `FkCategory`, `SortBy` (`productname` · `quantityonhand` · `reorderlevel` · `shortage`), `SortDesc`, `Page`, `PageSize`.
- **History:** `FkProduct`, `MovementType`, `From`, `To` (the user's local calendar days, inclusive), `TzOffsetMinutes` (browser `getTimezoneOffset()`), `Page`, `PageSize`.
- Store is **never** a filter: it always comes from the signed-in user's claims.

## Data
- Entities (repository only): `Inventory` → `inventory`, `StockMovement` → `stock_movements`. Read-only lookups: `products`, `product_variants`, `categories`, `units`, `stores`, `users`, `settings`.
- Models: `Models/InventoryModels.cs`, one region per operation.
  - Movement: `InputStockMovement` / `OutputStockMovement` (shared gateway).
  - Balances: `InputInventorySelect` / `OutputInventorySelect`, `InputInventoryList` / `OutputInventoryList`.
  - Opening: `InOpeningStockView` → `InputStockOpening`. Adjust: `InStockAdjustView` → `InputStockAdjust`.
  - Transfer: `InStockTransferCreateView` (+ `InStockTransferLineView`) → `InputStockTransferCreate` (+ `InputStockTransferLine`) → `OutputStockTransferCreate`. Legacy single-line `InputStockTransfer` / `OutputStockTransfer` kept for existing callers.
  - History: `InputStockMovementList` / `OutputStockMovementList` / `OutputStockMovementItem`. Low stock: `InputLowStockList` / `OutputLowStockList`.
  - Picker and lookups: `InputStockProductSearch` / `OutputStockProductSearch(Item)`, `OutputStockLookupItem`.

## Database Dependencies
- `inventory`: one row per store + product + variant (unique), `quantityonhand`, `quantityreserved`, `reorderlevel`; CHECK `ck_inventory_qty` keeps `quantityonhand >= 0`.
- `stock_movements`: append-only ledger; CHECK on `movementtype` (`purchase`, `sale`, `adjustment`, `transfer_in`, `transfer_out`, `return_in`, `return_out`, `opening`); `quantity` is stored positive except `adjustment` (signed).
- `settings`: key `pos.allow_negative_stock` (store scope wins over company scope).
- No schema change was made by this module.

## API & Data Access
- Controller: `InventoryController` (`[Authorize]`, `[ModuleAccess("INVENTORY")]`) → `IStockService` / `StockService` → `IStockRepository` / `StockRepository` (EF Core + LINQ, no Dapper).
- Permissions: `inventory.view`, `inventory.adjust` (both in the seeded catalog; the old non-catalog `inventory.manage` is gone).
- AJAX contract: every POST has `[ValidateAntiForgeryToken]`, `CommonHelpers.ValidateModelState`, an explicit `var input = new Input…` mapping and returns `CommonHelpers.SuccessResponse(message, data)` with `redirectUrl`; failures are typed exceptions (`ValidationException`, `ConflictException` 409, `UpdateFailedException` 422, `ForbiddenException` 403).
- Existing public members of `IStockRepository` / `IStockService` are unchanged (purchase, sale, online-order and report code call them); new members were only added.

## UI Rules & Conventions
- Views: `Views/Inventory/` — `Stock`, `LowStock`, `History`, `Opening`, `Adjust`, `Transfer`, plus module partials `_InventoryTabs` (section pills shown on every screen) and `NoStore`.
- JS (`wwwroot/js/inventory/`, on top of `sc-ajax.js`): `inventory-common.js` (`ScInventory`: product picker, quantity format, local times, busy state of filter forms), `stock-form.js` (Opening + Adjust), `transfer.js` (line document), `history.js` (product filter, timezone offset).
- Product pickers use `SmartCartAjax.typeahead` against `Inventory/ProductPicker`; picking a SKU sets the hidden product / variant ids, typing over it clears them.
- All dynamic text is written with `.text()` / Razor encoding; nothing from the user is rendered as HTML.
- Movement times are stored in UTC and rendered as `<time data-sc-local-time datetime="…Z">`, localised by JS (the UTC text is the no-JS fallback).
- Follow `ADMIN.md` → *Front-end conventions*.

## Business Rules
- **Store scope:** the store is read from the user's claims; any store id sent by the browser is ignored.
- **Stock status:** `out` = on hand ≤ 0; `low` = above 0 and ≤ reorder level; otherwise `ok`.
- **Effective reorder level:** the store row's own level when above 0, otherwise the product's level.
- **Suggested order** (Low stock): `ceil(2 × reorder level − on hand)`, never below 0; blank when no reorder level is set.
- **Opening stock:** allowed once per SKU and store; a second attempt is refused with 409 and points to Adjust.
- **Adjustment:** reasons Damaged, Expired, Lost or stolen, Found stock, Count correction, Other; written as one signed `adjustment` movement with notes `"Reason: free text"`; removing more than is on hand is refused.
- **Transfer:** destination must be another active store of the same company; all lines (merged by SKU, max 100) are moved as `transfer_out` + `transfer_in` pairs inside ONE database transaction — if any line is short, nothing moves (audit SC-SRV-004). The `transfer_in` row carries the `transfer_out` movement id as `referenceid`.
- **Concurrency:** the balance changes with one guarded `UPDATE … WHERE quantityonhand + delta >= 0`, so two simultaneous sales cannot both pass the stock check.
- **Transactions:** `ApplyMovementAsync` joins the caller's transaction when one is open (sale / purchase / online-order flows) and owns one otherwise.
- **Negative stock (SC-SRV-006):** only `sale` may go below zero, and only when `pos.allow_negative_stock` is `true`. See *Notes* — the table CHECK still blocks it until a SQL patch is applied.
- Movements are never edited or deleted (no hard deletes, no cascade).

## Dependencies & Related Modules
- `product.md`, `variant.md` (SKUs, reorder level, unit), `purchase.md` (receipts → `purchase`), `pos.md` (sales → `sale`), `returns.md` (`return_in` / `return_out`), `online-orders.md` (confirm / cancel), `stores.md`, `settings.md` (`pos.allow_negative_stock`), `roles.md` (permission catalog), `reports.md` (stock valuation and low-stock report reuse `IStockRepository`).

## Notes
- **Negative stock needs a SQL patch:** CHECK `ck_inventory_qty` (`quantityonhand >= 0`) must be relaxed or replaced before `pos.allow_negative_stock = true` has any effect. Until then a sale that would go negative fails with a friendly "Insufficient stock" message and the whole sale rolls back.
- No screen edits the per-store `reorderlevel`; stores inherit the product's level.
- The very first movement of a never-stocked SKU racing another first movement for the same SKU can hit the unique index (rare, not retried).
- One store per user (from claims): there is no store switcher on these screens.
- Forms rely on server validation plus light JS checks (jQuery validation is not wired on these pages).
- Sidebar: only the `Inventory` entry (→ `Stock`) exists; Low stock, History, Opening, Adjust and Transfer are reached through the in-page section pills.
- Written without a local build or run; the first build is the compile check.
