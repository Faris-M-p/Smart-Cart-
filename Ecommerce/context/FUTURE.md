# Future

Do not treat this file as implemented behavior.

## SmartCart now

A **single-store** supermarket shop: admin-managed SKU catalog and stock, customer COD orders, employee roles. Online visibility is per product/SKU (`SellOnline`).

## SmartCart then (planned or unfinished)

These appear in code comments, seed, leftover tables, or old `findings/` notes. They are **not** live products unless marked otherwise in module files.

### Payments and billing

- **UPI / online pay:** `PlaceOrder` rejects UPI with a “soon” message. PLANNED.
- **Billing module:** seeded `Billing.View/Create/Print/Cancel`. No controller, view, or sidebar link. PLANNED.
- **Invoices / UPI collection UI:** not implemented.

### Orders / ops

- Admin-created orders, line-item edits, tracking numbers, customer-facing shipment tracking: not implemented.
- Dashboard live KPIs: page is template placeholders. PLANNED / PARTIAL.

### Catalog / stock (from older findings)

`findings/decisions/Admin “Product Setup” workflow` describes an older stock model (`QuantityAvailable` / `QuantityReserved` on a unique SKU stock row) and ProductImages. **Current stock is batch `Stock` rows; media is ProductMedia/SkuMedia.** Do not implement the findings doc as if it were current schema.

### Guest / session shopping

`Cart.SessionKey` and `WishList.SessionKey` exist. Storefront C# requires login. Guest merge-on-login is **not** implemented.

### Ratings follow-ups and customers admin

Ratings & Reviews itself is **implemented** as order-item based reviews with a 7-day window (see `USER/modules/reviews.md`, `ADMIN/modules/review.md`). Still not built: a configurable review window (currently a constant 7 days in the procedures), a dedicated `deliveredon` column on `orders` (the window uses the `Delivered` shipping row's `shippingdate`), review titles (would need a schema change), rating filter/sort or rating badge on the shop listing (`Ratings`/`Gender` shop DTO fields remain unused), helpful-votes, review images, admin replies, `updatedat` on edits. A separate **Customers** admin module (list/manage storefront users) is **not** implemented; the "Customers" permission group label only hosts `Ratings` for now. PLANNED at most.

### Settings / multi-store / zones

Store-wide `StoreSettings` was **removed**. There is **no** multi-store or zone table or tenant column in the current schema. Any multi-store/zone expansion is **PLANNED only if product later adds it** — it is **not** specified in application code today.

### Leftover schema

AuditLogs, ProductStatus, ProductImages may stay for compatibility. New features should not assume they are wired.

### Template leftovers

Admin header dropdowns (edit profile, social, billing) are Mantis placeholders (`href="#!"`). Language switcher on the storefront header is static template markup.
