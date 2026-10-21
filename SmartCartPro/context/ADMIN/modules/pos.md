# Admin Module — POS Billing

## Module Overview
- **Area:** ADMIN
- **Module Name:** POS
- **Current Status:** `IMPLEMENTED` (server + AJAX cashier UI; cash sessions and day-close included)

## Purpose & Responsibilities
Store-scoped point-of-sale billing for the signed-in user's store: ring up a sale, attach it to the open cash session, deduct stock, and print an 80 mm / A4 receipt.
Platform Super Admin has no store (`fk_store = 0`): every POS / session / day-close screen shows `Views/Sales/StoreRequired.cshtml` instead of data. Super Admin POSTs throw `ValidationException`.

## Implemented Features
- `Sales/Pos`: two-column cashier (search + live results on the left, bill on the right). F2 focuses search, Enter runs `ProductByBarcode`, F9 completes. Walk-in customer, `Customers/Search` typeahead, `Customers/QuickCreate` modal, cart qty/discount, totals preview matching the server formula, payments, ClientRef GUID per bill, `sessionStorage` draft. Success overlay: Print + New sale. No open session → opening-float card.
- `GET Sales/ProductSearch?term=&take=` and `GET Sales/ProductByBarcode?barcode=` (`pos.sell`, max 20, `AsNoTracking`). Barcode miss → `ResourceNotFoundException`.
- `POST Sales/CreateSale` (JSON): store from claims only; client prices ignored (variant selling price = product price + additional price, else product price); tax = product tax else `default_tax_code` else 0, tax-exclusive on (subtotal − discount), 2 dp; payments required, methods from `028_Payments.sql`, paid ≥ total, change only from cash; open cash session attached server-side; ClientRef stored as the first notes line `__ref:{id}` (stripped on read); stock via `ApplyMovementAsync` in the same transaction; invoice number `IDocumentNumberService("SALE")`.
- `Sales/Session`: one open session per store; open / close forms; session list with pager. Close variance = **counted − expected**. Expected cash = opening float + cash payments − change given.
- `Sales/DayClose/{id}`: counts, payment totals, returns, expected vs counted, print layout.
- Returns GET/POST stay compiled with `pos.return` (return UI is a later wave). `OnlineOrders` stub removed.

## Filters / Search / Sorting
- POS product search: name, code, SKU, barcode (product and variant). Take 1–20 (default 20).
- Session list: `status` (`open|closed`), `from` / `to` (inclusive calendar days in `locale.timezone`), `page`, `pageSize` (10 / 25 / 50 / 100).

## Data
- Entities (repositories only): `Sale`, `SaleDetail`, `Payment`, `CashSession`; reads `products`, `product_variants`, `taxes`, `units`, `inventory`, `customers`, `stores`, `company`, `users`.
- Models: `Models/SaleModels.cs` — POS page, product lookup, sale create / select / list / cancel / return, cash session, cash summary. Validation attributes only on `In…View`.

## Database Dependencies
- `sales`, `sale_details`, `payments` (`cash|card|upi|wallet|bank|other`), `cash_sessions`, `invoice_sequences` (`SALE`), `inventory` / `stock_movements`, settings `default_tax_code`, `locale.timezone`, `locale.currency`.
- `pos.invoice_prefix` (company or store override) is prepended to SALE numbers when the sequence row has no prefix of its own.
- Units have no `allowsfraction` column: POS treats PCS/PC/EA/EACH/NOS/NO/UNIT/UNT/BOX/PKT/PACK/SET/PAIR as whole units.

## API & Data Access
- Controller: `SalesController` → `ISaleService` → `ISaleRepository` (→ `IStockRepository`, `IDocumentNumberService`, `ISettingService`). Class `[ModuleAccess("POS")]`.
- Routes and permissions:

| Route | Verb | Permission |
|-------|------|------------|
| `/Sales/Pos` | GET | `pos.sell` |
| `/Sales/CreateSale` | POST | `pos.sell` |
| `/Sales/ProductSearch` | GET | `pos.sell` |
| `/Sales/ProductByBarcode` | GET | `pos.sell` |
| `/Sales/Session` | GET | `pos.sell` |
| `/Sales/OpenSession` | POST | `pos.sell` |
| `/Sales/CloseSession` | POST | `pos.sell` |
| `/Sales/DayClose/{id}` | GET | `pos.sell` |
| `/Sales/CashSummaryJson/{id}` | GET | `pos.sell` |
| `/Sales/Returns` | GET | `pos.return` (list) |
| `/Sales/Return/{id}` | GET, POST | `pos.return` (create against invoice) |
| `/Sales/ReturnDetails/{id}` | GET | `pos.return` |

- Mutating POSTs: `[ValidateAntiForgeryToken]`, `ValidateModelState`, named `Input…`, `SuccessResponse`. Failures are typed `AppException`s. No TempData / redirect from POST (`data.redirectUrl` after toast).

## UI Rules & Conventions
- Views: `Views/Sales/Pos`, `Session`, `DayClose`, `StoreRequired`. JS: `wwwroot/js/sales/pos.js`, `pos-cart.js`, `session.js`. CSS: `wwwroot/css/pos.css`.
- Custom overlays (`.sc-modal`) — `window.bootstrap` is not global.
- Timestamps: `<time data-sc-utc="@TimeDisplay.Iso(x)" data-sc-format="datetime">`.
- Lookup JSON camelCase: `{ idProduct, idProductVariant, name, variantName, sku, barcode, unitName, allowsFraction, sellingPrice, taxRate, stockOnHand }`.

## Business Rules
- Never trust the client for store, session, prices, tax or totals.
- One open cash session per store. Completing a sale without one is `ConflictException`.
- ClientRef idempotency: same store + first notes line `__ref:{id}` returns the existing sale without writing again.
- Soft cancel only (sales / payments). See `invoices.md` for invoice cancel.

## Dependencies & Related Modules
- `invoices.md`, `inventory.md` (`ApplyMovementAsync` joins the caller TX), `customers.md` (Search / QuickCreate — other agent), `settings.md`, `product.md` / `variant.md`.

## Notes
- Cashiers may lack `products.view`; POS has its own lookups.
- Customer Search / QuickCreate are owned by the customers agent; POS already calls those URLs.
- Known gap: units have no fraction flag (heuristic: PCS/EA/BOX/… are whole units).
