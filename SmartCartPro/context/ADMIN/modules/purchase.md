# Admin Module — Purchase

## Module Overview
- **Area:** ADMIN
- **Module Name:** Purchase (purchase order + goods receipt / GRN)
- **Current Status:** `IMPLEMENTED` (server + AJAX UI)

## Purpose & Responsibilities
Store-scoped purchase orders (`fk_store` = the signed-in user's store): create/edit, print, receive goods against the order (partial receipts allowed) and cancel with a reason. Receiving posts stock movements, so on-hand quantities follow the order.
Platform administrators have no store (`fk_store = 0`): every purchase screen shows a "sign in as a store user" panel (`Views/Purchase/StoreRequired.cshtml`) instead of data. Suppliers stay available (see `supplier.md`).

## Implemented Features
- `Purchase/Index` list: search (PO number such as `PO-000123` / invoice # / supplier), status, supplier and purchase-date filters, sortable headers, page-size selector, pager, row actions (View / Receive / Edit / Cancel) driven by server flags `CanReceive` / `CanEdit` / `CanCancel`.
- `Purchase/Create` and `Purchase/Edit/{id}`: header (supplier, date, supplier invoice #, notes) + dynamic line grid (product/variant typeahead, quantity, unit cost, discount, tax % and live preview totals). The same product cannot be added twice (picking it again adds 1 to the quantity). "Save as draft" or "Create purchase order" on create; "Save & post" / "Save draft" when editing a draft; "Save changes" for a posted order.
- `Purchase/Details/{id}`: header, supplier/order/totals cards, lines with ordered / received / pending quantities, goods receipts list, cancelled banner with reason, guard notices (`?notice=edit-locked|receive-locked|cancel-locked`).
- `Purchase/Print/{id}`: standalone print sheet (no layout) that opens the browser print dialog.
- `Purchase/Receive/{id}`: goods receipt form showing ordered, already received and pending quantity per line (fixes SC-SRV-005), "Receive all remaining" / "Clear", receipt date and notes. Over-receipt is blocked on the server.
- `Purchase/Cancel/{id}`: cancellation form with a mandatory reason; goods already received stay in stock.
- `Purchase/ProductLookup?term=` JSON for the line picker (active products and variants by name, code, SKU or barcode; min 2 characters).

## Filters / Search / Sorting
- Query string: `search`, `status` (`draft|posted|partially_received|received|cancelled`), `fkSupplier`, `from`, `to` (`yyyy-MM-dd`, applied to the **purchase date**), `sortBy` (`id|purchasedate|supplier|invoicenumber|totalamount|status`, default `purchasedate` descending), `sortDesc`, `page`, `pageSize` (10 / 25 / 50 / 100).
- `PurchaseService.ListPurchasesAsync` normalises everything (unknown status/sort ignored, reversed dates swapped, last-page fallback, `PO-000123` parsed into an id search) and echoes the filter back as `OutputPurchaseList.Filter`.
- The list shows the **stored** status; the details page shows the status derived from received quantities (identical unless a receipt was interrupted).

## Data
- Entities (repositories only): `Purchase` → `purchases`, `PurchaseDetail` → `purchase_details`; reads `suppliers`, `products`, `product_variants`, `taxes`, `stock_movements`, `stores`.
- Models: `Models/PurchaseModels.cs` — regions Purchase Create / Update / Select / List, Goods Receipt, Purchase Cancel, Purchase Product Lookup (`InPurchaseCreateView`, `InPurchaseUpdateView`, `InGoodsReceiptView`, `InPurchaseCancelView`, `InputPurchase*`, `InputGoodsReceipt`, `Output*`). Validation attributes live only on the `In…View` classes.
- Derived data (no schema change):
  - **Received quantity** per line = stock movements with `movementtype = 'purchase'`, `referencetype = 'purchase'`, `referenceid = idPurchase`, allocated to lines by product/variant in order and capped at the ordered quantity.
  - **Line discount** = gross (`qty × unitcost`) + tax − line total; the header discount is stored in `purchases.discountamount`.
  - **PO number** is display-only: `PO-{idPurchase:D6}`.
  - **GRN number** comes from `IDocumentNumberService.NextAsync(store, "GRN")` and, together with the receipt date and the user's notes, is stored only in the stock movement `notes` (`GRN {no} | yyyy-MM-dd | notes`); the receipts table on the details page reads it from there.

## Database Dependencies
- `purchases`, `purchase_details` (quantity NUMERIC(14,3) with `CHECK > 0`, amounts NUMERIC(12,2)), `suppliers`, `stock_movements`, `invoice_sequences` (GRN numbers), `stores`.
- No GRN table and no received-quantity / PO-number columns exist (see Notes).

## API & Data Access
- Controller: `PurchaseController` → `IPurchaseService` (+ `ISupplierService` for dropdowns) → `IPurchaseRepository` (→ `IStockRepository` for movements). Module `PURCHASES`.
- Routes and permissions:
  - `GET Purchase/Index`, `GET Purchase/Details/{id}`, `GET Purchase/Print/{id}` — `purchases.view`
  - `GET|POST Purchase/Create`, `GET|POST Purchase/Edit/{id}`, `GET|POST Purchase/Receive/{id}`, `GET|POST Purchase/Cancel/{id}`, `GET Purchase/ProductLookup` — `purchases.manage`
- Document POSTs (create, edit, receive) are **JSON** sent with `SmartCartAjax.postJson` and bound with `[FromBody]` (`InPurchaseCreateView`, `InPurchaseUpdateView`, `InGoodsReceiptView`); the cancel form posts form-encoded. All POSTs use `[ValidateAntiForgeryToken]` (token sent as the `RequestVerificationToken` header), `CommonHelpers.ValidateModelState`, an explicit `InputXxx` variable, then `CommonHelpers.SuccessResponse(message, { id, redirectUrl })`. Failures are thrown typed exceptions (`ValidationException` 422 with `Lines[i].Field` keys, `ConflictException` 409, `ResourceNotFoundException` 404, `ForbiddenException` 403); there is no `TempData` or redirect from a POST.
- GET guards redirect to `Details?notice=<code>` (friendly alert) when the action is no longer allowed.

## UI Rules & Conventions
- Views: `Views/Purchase/` — `Index`, `Create`, `Edit`, `Details`, `Print`, `Receive`, `Cancel`, `StoreRequired`, partials `_PurchaseLines`, `_PurchaseStatusBadge`. The old placeholder `Views/Inventory/Purchases.cshtml` was removed.
- JS (`wwwroot/js/purchasing/`): `purchase.js` (list, details formatting, cancel form), `purchase-form.js` (create/edit grid + typeahead + preview totals), `purchase-receive.js` (GRN form), `purchase-print.js` (print sheet, no jQuery). Line inputs use `data-field` (no `name`) so shared error mapping cannot mis-target them; row errors are mapped from `Lines[i].Field` keys.
- All dynamic text is written with `.text()` / `SmartCartAjax.escapeHtml`; never `Html.Raw` on user data. Money cells use `data-sc-money`, UTC timestamps `<time data-sc-local-datetime>` and are localised in JS.
- Breadcrumb group label is "Purchase & stock". Follow `ADMIN.md` → *Front-end conventions*.

## Business Rules
- Never trust the client: store comes from the sign-in claim, product/variant/tax/prices are re-read from master data, and **subtotal, discount, tax and total are always recomputed on the server**. Tax is tax-exclusive on the discounted line amount using the product's tax profile; quantity is rounded to 3 decimals, money to 2; the discount cannot exceed the line amount; no duplicate product/variant lines; at most 200 lines.
- Status flow: `draft` → `posted` → `partially_received` → `received`; `cancelled` is terminal. A posted order can never go back to draft.
- **Edit** is allowed for `draft` and for `posted` orders with no receipts (flag `CanEdit`). Draft orders cannot be received until posted.
- **Receive** needs `posted` / `partially_received` with pending quantity (`CanReceive`). Each line's receive quantity is validated against the pending quantity re-read inside the lock (`Line N: only X is still pending (you entered Y).`); product/variant come from the order line, never from the client. The order moves to `received` when everything is in, otherwise `partially_received`.
- **Cancel** is allowed unless already cancelled or fully received (`CanCancel`); the reason is required (2–500 characters). Received goods stay in stock; the order is soft-cancelled (`cancelled`, `cancelledon`, `cancelledreason`), never deleted.
- Supplier must be active when chosen on create/edit (an existing inactive supplier remains visible on its documents).
- Concurrency: update/receive/cancel of the same order are serialised by in-process striped gates in `PurchaseService`.
- Stock movements and document numbers each run in their own transaction, so no outer transaction wraps them: create/edit use a single `SaveChanges` (atomic), receive writes the movements one by one and the status **last**, so a status never claims more than what was received.

## Dependencies & Related Modules
- `supplier.md`, `product.md`, `inventory.md` (`StockRepository.ApplyMovementAsync`), `stores.md`

## Notes
- Sidebar entry: "Purchases" → `Purchase/Index`, icon `bi-file-earmark-text`, permission `purchases.view`, module `PURCHASES`. `_Layout.cshtml` and the dashboard quick link still point at the removed `Inventory/Purchases` action until the orchestrator updates them.
- Known gaps: concurrency gates are per process (multi-instance deployments need a database lock/row version); an interrupted receipt can leave stock movements without the final status write (the details page derives the true status from the movements); the PO number and GRN number are not persisted as first-class data. Suggested schema work (outside this module): `purchase_details.receivedqty`, a `goods_receipts` / `goods_receipt_lines` pair and a stored PO number sequence.
- `SaleRepository` has the same nested-transaction pattern around `StockRepository.ApplyMovementAsync` (not part of this module) — see the orchestrator request.
