# User Module — Checkout

## Module Overview
- **Area:** USER (Storefront)
- **Module Name:** Checkout
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Handles order review, Flipkart-style saved delivery address selection, modal-based address creation & editing, location detection & geocoding via dedicated location modal, Cash on Delivery (COD) payment selection, and placement of customer orders.

## Implemented Features
- **Checkout Preview:** Displays current cart line items, pricing breakdown, tax, shipping costs, order total, customer details, and saved customer addresses (`GetCheckoutPreview`).
- **Flipkart-Style Saved Addresses:** Displays clean, compact address cards with type badges (`HOME`, `WORK`, `OFFICE`, `OTHER`), receiver name, complete delivery address, mobile number, and "DELIVER HERE" action button. Highlighted selected state with checkmark badge.
- **Add / Edit Address Modal:** Modal dialog (`#addressModal`) featuring clean Address Type pill/chip selector (`Home`, `Work`, `Office`, `Other`), input fields for receiver name, phone, address line, city, pincode, and location fetch shortcut.
- **Location Detection Modal:** Modal (`#locationFetchModal`) using browser Geolocation API (`navigator.geolocation.getCurrentPosition`), OpenStreetMap Nominatim reverse geocoding, and manual India state & city dropdown proxies (`/Checkout/Location/IndiaStates`, `/Checkout/Location/IndiaCities/{stateIso}`) to auto-fill city, pincode, address line, latitude, and longitude directly into form fields.
- **Order Placement:** Validates stock levels, binds selected saved or newly added delivery address (`UserAddresses`), inserts `Orders`, `OrderItems`, `Payments`, `Shipping`, deducts SKU stock, and clears customer cart (`PlaceOrder`).

## Filters / Search / Sorting
- Saved addresses are ordered by `IsDefault DESC, AddressId DESC`.

## Data
- DTOs: `CheckoutPage`, `CheckoutPreviewInput`, `PlaceOrderInput`, `UserAddress`, `SaveAddressInput`, `OrderConfirmationView`, `CommonResponse`.

## Database Dependencies
- Tables: `Orders`, `OrderItems`, `Payments`, `Shipping`, `Cart`, `CartItems`, `Stock`, `ProductVariants`, `UserAddresses`.
- Stored Procedures: `GetCheckoutPreview`, `PlaceOrder`, `GetUserAddresses`, `SaveUserAddress`, `DeleteUserAddress`.

## API & Data Access
- Controller: `Controllers/CheckoutController.cs` (`Index`, `Preview`, `Place`, `Confirmation`, `GetAddresses`, `SaveAddress`, `DeleteAddress`, `GetIndiaStates`, `GetIndiaCitiesForState`).
- Repository: `OrderRepository.cs` via `IDataAccessDapper`.

## UI Rules & Conventions
- Checkout page: `views/Checkout/Index.cshtml`.
- Confirmation page: `views/Checkout/Confirmation.cshtml` displaying order number (e.g. `SC000123`).
- Client script: `wwwroot/js/shop-checkout.js`.
- Stylesheet: `wwwroot/css/shop-checkout.css`.

## Business Rules
- **Requires Customer Authentication:** `UserId` extracted from JWT cookie.
- **COD Only:** Payment method must be Cash on Delivery (`COD`). Online UPI payment attempts are explicitly rejected by `PlaceOrder` stored procedure with an "available soon" message.
- SKU stock is validated and deducted atomically inside `PlaceOrder` SP.
- Auto-generates OrderNumber format: `SC` + 6-digit padded `OrderId`.
- **Saved Address Type:** Categorized as `Home`, `Work`, `Office`, or `Other`.
- **Idempotent Address Save:** Checking "Make this my default delivery address" automatically persists or updates the address record in `UserAddresses` table.

## Dependencies & Related Modules
- `USER/modules/cart.md` — Source of items to purchase.
- `USER/modules/orders.md` — View placed order history.
- `ADMIN/modules/order.md` — Admin order fulfillment.
- `ADMIN/modules/supplier.md` — Shared location endpoints proxy logic.
