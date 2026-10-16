# User (storefront) — reserved

## Purpose

Customer-facing shop (browse, cart, checkout, orders, reviews). **Not the primary SmartCartPro surface yet.**

## Status

`PLANNED` — PostgreSQL tables for carts, wishlists, online orders, reviews exist in `Database-Postgres/`. No dedicated USER MVC area comparable to Ecommerce storefront.

## Rules

- Keep USER docs under `context/USER/modules/` when storefront modules are implemented.
- Do not mix storefront rules into ADMIN module files.
- Shared tables (products, customers) may be referenced from both areas; document ownership clearly.

## Modules

None yet. Add files under `modules/` when work starts (e.g. `shop.md`, `cart.md`, `checkout.md`).
