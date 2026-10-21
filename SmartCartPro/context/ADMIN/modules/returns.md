# Admin Module — Sales returns

## Module Overview
- **Area:** ADMIN
- **Module Name:** Returns
- **Current Status:** `IMPLEMENTED` (list, create against invoice, details)

## Purpose & Responsibilities
Process returns against completed store invoices: restock sold quantities, record a `sales_returns` document, and show staff the original invoice. Cash is not refunded by the system.

## Implemented Features
- `GET Sales/Returns`: store-scoped list (search return # / invoice #, inclusive From/To, status, sort, pager). Super Admin (`fk_store = 0`) sees `StoreRequired`.
- `GET Sales/Return/{id}` (`id` = sale id): return form for a **completed** invoice of this store. Lines show original qty, already returned, remaining (max returnable). Default ReturnQty 0. Reason required (min 3). `ResourceNotFoundException` otherwise.
- `POST Sales/Return/{id}`: antiforgery, `ValidateModelState`, named `InputSaleReturn`, `ReturnSaleAsync`, AJAX `SuccessResponse` with `redirectUrl` to `ReturnDetails`.
- `GET Sales/Returns/{id}` redirects to `Sales/Return/{id}`; `POST Sales/Returns/{id}` still processes (legacy invoice links).
- `GET Sales/ReturnDetails/{id}` (`id` = sales_return id): header, lines, original invoice link. Print is not implemented.
- Invoice details **Return** button points at `asp-action="Return"`.
- Complete is disabled while every ReturnQty is 0.

## Filters / Search / Sorting
- Query: `search` (return number or invoice number), `status` (`completed|cancelled`), `from`, `to` (`yyyy-MM-dd`, inclusive via `locale.timezone`), `sortBy` (`id|returnnumber|invoicenumber|returndate|totalamount|status|cashier`, default `returndate` descending), `sortDesc`, `page`, `pageSize` (10 / 25 / 50 / 100).
- `SaleService.ListSalesReturnsAsync` normalises filters (unknown status/sort ignored, reversed dates swapped, last-page fallback) and echoes them as `OutputSalesReturnList.Filter`.

## Data
- Create: `InSaleReturnView` / `InReturnLineView` / `InputSaleReturn` / `InputReturnLine` / `OutputSaleReturn`.
- Returnable invoice: `InputSaleReturnableSelect` / `OutputSaleReturnableSelect` / `OutputSaleReturnableLine`.
- List: `InputSalesReturnList` / `OutputSalesReturnList` / `OutputSalesReturnListItem`.
- Details: `InputSalesReturnSelect` / `OutputSalesReturnSelect` / `OutputSalesReturnLine`.
- `MaxQty` on the form is remaining qty (display). Posted `UnitPrice` is ignored; the repository uses the original sale line unit price.

## Database Dependencies
- `sales_returns`, `sales_return_details`, `sales`, `sale_details`, `stock_movements` / `inventory` (`return_in`), `invoice_sequences` (`RETURN`), `store`, `customers`, `users`.
- Soft cancel columns exist on `sales_returns` (`cancelled` / `cancelledon` / `cancelledreason`). Returns are not hard-deleted.
- **No refund / payment-reversal table.** Completing a return does not write `payments` or refund cash.

## API & Data Access
- `SalesController` → `ISaleService` → `ISaleRepository`. Class `[ModuleAccess("POS")]`.

| Route | Verb | Permission |
|-------|------|------------|
| `/Sales/Returns` | GET | `pos.return` |
| `/Sales/Returns/{id}` | GET | `pos.return` (redirect → `Return`) |
| `/Sales/Returns/{id}` | POST | `pos.return` (legacy; same as `Return` POST) |
| `/Sales/Return/{id}` | GET | `pos.return` |
| `/Sales/Return/{id}` | POST | `pos.return` |
| `/Sales/ReturnDetails/{id}` | GET | `pos.return` |

- Mutating POST: antiforgery, named `var input = new InputSaleReturn { … }`, then `ReturnSaleAsync`. Success `redirectUrl` = `ReturnDetails/{id}`.

## UI Rules & Conventions
- Views: `Returns` (list), `Return` (form), `ReturnDetails`, `_ReturnStatusBadge`. Shared `StoreRequired`. JS: `wwwroot/js/sales/returns.js` (`SmartCartAjax.submitForm`).
- List copies Category / Invoices: GET filters, sortable headers, empty state, `_Pager`. Timestamps: `<time data-sc-utc="@TimeDisplay.Iso(x)" data-sc-format="datetime">`.
- Form copies Purchase receive: qty inputs, remaining shown, fully-returned lines locked. Details copy Purchase / Invoice details (header cards + lines).
- Do not use C# in tag-helper attribute areas (RZ1031).

## Business Rules
- Store from claims on input (`fk_store`). Super Admin store 0 → `StoreRequired`, no query.
- Completed, non-cancelled sales of this store only. Cancelled / held / other store → `ResourceNotFoundException` on GET; POST `ReturnSaleAsync` throws typed `ResourceNotFoundException` / `ConflictException` (not ResponseCode bags).
- At least one line with qty &gt; 0. Each qty capped at remaining = original sale-line qty minus the sum of `sales_return_details.quantity` for that product/variant on **non-cancelled** prior returns of the same sale (FIFO across original lines).
- Client `UnitPrice` is ignored; original `sale_details.unitprice` is used. Line total = qty × unit price (no tax/discount rewrite).
- Reason min 3 characters (view `[Required]` + `[StringLengthValidation(3, 500)]`; service re-checks).
- One transaction: `NextAsync("RETURN")` and `ApplyMovementAsync` (`movement_type` = `return_in`, `reference_type` = `sales_return`) join the caller TX. Stock failure rolls the return back.
- Soft cancel only — no hard delete of return documents. Cancel-return UI is not implemented.
- **Payments / refunds:** there is no refund table. Cash is **not** auto-refunded; staff handle money at the till. The form and details pages say so.

## Dependencies & Related Modules
- `invoices.md` (Return button, `CanReturn` on completed invoices), `pos.md`, `inventory.md`.

## Notes
- Sidebar Sales / Returns points at `Sales/Returns`.
- Print of a return document is optional and not shipped.
- Invoice details Return button uses `Sales/Return/{id}` and is hidden when remaining qty is 0.
