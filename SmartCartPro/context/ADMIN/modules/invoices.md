# Admin Module — Sales invoices

## Module Overview
- **Area:** ADMIN
- **Module Name:** Invoices
- **Current Status:** `IMPLEMENTED` (list, details, print; cancel from details)

## Purpose & Responsibilities
List and view POS invoices for the signed-in store, print an 80 mm or A4 receipt, and cancel a completed sale (stock reversed) when no returns exist.
Platform Super Admin (`fk_store = 0`) sees `Views/Sales/StoreRequired.cshtml` on every invoice screen.

## Implemented Features
- `Sales/Invoices`: search (invoice # / customer / phone), status, payment method, inclusive From/To (`locale.timezone`), `customerId` query, sortable headers, page size, pager, page totals (total and paid).
- `Sales/InvoiceDetails/{id}`: header (customer, store, cashier), lines, payments, notes with `__ref:` stripped, cancelled banner, Print / Return / Cancel buttons gated by status and permission.
- Cancel: details overlay posts `{ id, reason }` via `SmartCartAjax.post` to `POST Sales/CancelSale`. Reason min 3 characters. Completed sales of this store only; refused if returns exist; stock reversed in one transaction; payments soft-cancelled; no GET cancel page.
- `Sales/InvoicePrint/{id}`: `Layout = null`, `pos-print.css`, 80 mm default, `?paper=a4`, `?autoprint=1`. Store / company come from the sale projection.
- `Sales/InvoiceJson/{id}`: same payload as details for reprint helpers.

## Filters / Search / Sorting
- Query: `search`, `status` (`completed|cancelled`), `paymentMethod` (`cash|card|upi|wallet|bank|other`), `customerId`, `from`, `to` (`yyyy-MM-dd`, inclusive), `sortBy` (`id|invoicenumber|saledate|customer|totalamount|paidamount|status`, default `saledate` descending), `sortDesc`, `page`, `pageSize` (10 / 25 / 50 / 100).
- `SaleService.ListSalesAsync` normalises filters (unknown status/sort/method ignored, reversed dates swapped, last-page fallback) and echoes them as `OutputSaleList.Filter`.

## Data
- Same sale models as `pos.md` (`OutputSaleSelect`, `OutputSaleList`, `InSaleCancelView` / `InputSaleCancel` / `OutputSaleCancel`).
- `OutputSaleSelect` includes store and company name, address, phone, tax number, currency, cashier, `HasReturns`, `CanCancel`, `CanReturn`.

## Database Dependencies
- `sales`, `sale_details`, `payments`, `sales_returns` (cancel guard), `store`, `company`, `customers`, `users`.

## API & Data Access
- Same stack as POS (`SalesController` → `ISaleService` → `ISaleRepository`). Class `[ModuleAccess("POS")]`.

| Route | Verb | Permission |
|-------|------|------------|
| `/Sales/Invoices` | GET | `pos.sell` **or** `reports.view` |
| `/Sales/InvoiceDetails/{id}` | GET | `pos.sell` **or** `reports.view` |
| `/Sales/InvoicePrint/{id}` | GET | `pos.sell` **or** `reports.view` |
| `/Sales/InvoiceJson/{id}` | GET | `pos.sell` **or** `reports.view` |
| `/Sales/CancelSale/{id}` | POST | `pos.cancel` |

- Cancel POST: antiforgery, `ValidateModelState`, named `InputSaleCancel`, `SuccessResponse` with `redirectUrl` back to details.

## UI Rules & Conventions
- Views: `Invoices`, `InvoiceDetails`, `InvoicePrint`, `_InvoiceStatusBadge`, `StoreRequired`. JS: `invoices.js`, `invoice-print.js`. Print CSS: `wwwroot/css/pos-print.css`.
- Cancel overlay is `.sc-modal` (no `window.bootstrap`).
- Timestamps: `<time data-sc-utc="@TimeDisplay.Iso(x)" data-sc-format="datetime">`.

## Business Rules
- Cancel only `completed` invoices of this store that have no non-cancelled returns. Already cancelled → `ConflictException`.
- Stock return_in movements and payment soft-cancel run in the same transaction as the sale cancel.
- Notes shown to users never include the `__ref:{clientRef}` first line.

## Dependencies & Related Modules
- `pos.md`, `returns.md` (later wave), `customers.md`, `reports.md` (invoice list permission).

## Notes
- Return button on details goes to `Sales/Return/{id}` when `CanReturn` is true (completed invoice with remaining qty). The returns list is `Sales/Returns`.
- `_Layout` invoice / POS menu entries are owned by the orchestrator.
