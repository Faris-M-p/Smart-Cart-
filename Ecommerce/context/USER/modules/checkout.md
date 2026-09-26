# User Module — Checkout

## Module Overview
- **Area:** USER (Storefront)
- **Module Name:** Checkout
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Handles order review, shipping address entry, Cash on Delivery (COD) payment selection, and placement of customer orders.

## Implemented Features
- **Checkout Preview:** Displays current cart line items, pricing breakdown, tax, shipping costs, and order total (`GetCheckoutPreview`).
- **Shipping Address Input:** Collects receiver name, phone number, address line, city, pincode, state.
- **Order Placement:** Validates stock levels, inserts `Orders`, `OrderItems`, `Payments`, `Shipping`, deducts SKU stock, and clears customer cart (`PlaceOrder`).

## Filters / Search / Sorting
- N/A.

## Data
- DTOs: `CheckoutPreviewView`, `PlaceOrderInput`, `OrderConfirmationView`, `CommonResponse`.

## Database Dependencies
- Tables: `Orders`, `OrderItems`, `Payments`, `Shipping`, `Cart`, `CartItems`, `Stock`, `ProductVariants`.
- Stored Procedures: `GetCheckoutPreview`, `PlaceOrder`.

## API & Data Access
- Controller: `Controllers/CheckoutController.cs` (`Index`, `GetPreview`, `PlaceOrder`, `Confirmation`).
- Repository: `OrderRepository.cs` via `IDataAccessDapper`.

## UI Rules & Conventions
- Checkout page: `views/Checkout/Index.cshtml`.
- Confirmation page: `views/Checkout/Confirmation.cshtml` displaying order number (e.g. `SC000123`).

## Business Rules
- **Requires Customer Authentication:** `UserId` extracted from JWT cookie.
- **COD Only:** Payment method must be Cash on Delivery (`COD`). Online UPI payment attempts are explicitly rejected by `PlaceOrder` stored procedure with an "available soon" message.
- SKU stock is validated and deducted atomically inside `PlaceOrder` SP.
- Auto-generates OrderNumber format: `SC` + 6-digit padded `OrderId`.

## Dependencies & Related Modules
- `USER/modules/cart.md` — Source of items to purchase.
- `USER/modules/orders.md` — View placed order history.
- `ADMIN/modules/order.md` — Admin order fulfillment.
