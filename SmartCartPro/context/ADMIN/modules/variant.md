# Admin Module — Variant / SKU

## Module Overview
- **Area:** ADMIN
- **Module Name:** Variant (product variants / SKU)
- **Current Status:** `IMPLEMENTED` (server + AJAX UI; served by `ProductController`, see `product.md` for shared rules and the picker JSON)

## Purpose & Responsibilities
Sellable child items of a product (size, colour, pack) with their own variant name, SKU code, barcode and price adjustment (`product_variants`). A variant never has its own category, tax or unit: it inherits them from the parent product.

## Implemented Features
- `Product/Variants?productId=N`: variants of one product (search, status filter, sorting, page size, pagination, add button, empty state).
- `Product/Variants` (no product): global SKU list across all products, with a product column that links to the product detail.
- `Product/CreateVariant?productId=N` and `Product/EditVariant/{id}`: full-page AJAX forms (field errors, toast, redirect back to the list). A product summary and a live preview of the variant's selling price (product price + additional price) sit above the form; the server always recalculates it.
- `Product/SetVariantActive` (POST): soft activate/deactivate with confirm; no hard delete.
- Variants are also listed on the product detail page (first 100, with add/edit/activate when allowed).
- Manage buttons are hidden without `products.manage`; the server enforces the same permission.

## Filters / Search / Sorting
- Query string: `productId`, `Search` (variant name, SKU, barcode, product name/code), `ActiveOnly` (`true|false`, absent = all), `SortBy` (`variantname|skucode|additionalprice|isactive|product`), `SortDesc`, `Page`, `PageSize` (default 25, max 100).
- Default sort: `variantname` when scoped to a product, `product` for the global list. An unknown `productId` returns 404.

## Data
- Entity (repository only): `ProductVariant` → `product_variants`, joined to `Product` for names and the base price.
- Models: `Models/ProductModels.cs` (variant regions)
  - `InVariantCreateView` / `InputVariantCreate` / `OutputVariantCreate`
  - `InVariantUpdateView` / `InputVariantUpdate` / `OutputVariantUpdate`
  - `InputVariantSelect` / `OutputVariantSelect` (product code/name/active/price, `EffectivePrice`, `ToUpdateView()`)
  - `InputVariantList` / `OutputVariantList`, `InputVariantSetActive` / `OutputVariantSetActive`, `InputVariantCheck` / `OutputVariantCheck`

## Database Dependencies
- `product_variants.fk_product` → `products`.
- Unique constraints (also cover cancelled rows): `uq_product_variants_product_name` (`fk_product`, `variantname`), `uq_product_variants_skucode`, `uq_product_variants_barcode`.

## API & Data Access
- Controller: `ProductController` (variant actions) → `IProductService` → `IProductRepository`.
- Views: `Views/ProductVariant/Variants.cshtml`, `CreateVariant.cshtml`, `EditVariant.cshtml`. JS: `wwwroot/js/product/variant.js` (forms only; the list uses declarative `data-sc-*`).

| Route | Verb | Permission |
|-------|------|------------|
| `/Product/Variants` | GET | `products.view` |
| `/Product/CreateVariant` | GET, POST | `products.manage` |
| `/Product/EditVariant/{id}` | GET, POST | `products.manage` |
| `/Product/SetVariantActive` | POST | `products.manage` (params `id`, `isActive`) |

- Picker JSON (`Product/Search`, `Product/FindByBarcode`) returns variant rows too (`idProductVariant` set, `sku`/`barcode` of the variant, `sellingPrice` = product price + additional price). Contracts are in `product.md`.

## UI Rules & Conventions
- Same conventions as `product.md` / `ADMIN.md`; `returnUrl` is checked with `Url.IsLocalUrl`; SKU and barcode inputs ignore Enter for scanners.
- A variant of an inactive product shows a "Product inactive" hint and is not offered in pickers.

## Business Rules
- Variant name is unique per product (case-insensitive). SKU code and barcode are optional (stored as `NULL` when blank) and unique across variants **and** products.
- The parent product must exist and be non-cancelled. On update the stored parent product is used; the posted `FkProduct` is ignored, so a variant cannot be moved between products.
- Additional price is >= 0 and rounded to 2 decimals; a variant's MRP in pickers is the product MRP plus the additional price (null when the product has no MRP).
- Soft cancel / activate-deactivate only. Deactivating a product does not change its variants' own flag.

## Dependencies & Related Modules
- `product.md`, `inventory.md` (stock rows carry `fk_product_variant`), `purchase.md`, `pos.md`

## Notes
- **Sidebar:** SKU / Variants → `Product/Variants` (`bi-upc`, `products.view`, module `PRODUCTS`). The layout still points at the removed `Catalog/Variants`; it must be switched.
- **Known gaps:** no per-variant cost price, tax or images; no bulk create (for example size matrices); no variant delete.
