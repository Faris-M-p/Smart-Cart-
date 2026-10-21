# Admin Module — Product

## Module Overview
- **Area:** ADMIN
- **Module Name:** Product
- **Current Status:** `IMPLEMENTED` (server + AJAX UI; variants are documented in `variant.md`, the screens share `ProductController`)

## Purpose & Responsibilities
Company-wide product master (shared by every store): browse, create, edit, activate/deactivate, detail page with variants and stock, plus the JSON lookups that purchase, stock and POS pickers use. Stock quantities stay store-scoped and are owned by `inventory.md`.

## Implemented Features
- `Product/Index` list: search (name, code, SKU, barcode and variant SKU/barcode), category → sub-category, brand and status filters, sorting, page size, pagination, empty state, confirm → POST → toast.
- `Product/Details/{id}`: header, classification, pricing (cost price + margin before tax only for staff who may see costs), variants table (first 100) and a "Stock in your store" card for store users with `inventory.view`.
- `Product/Create` and `Product/Edit/{id}`: full-page AJAX forms (field errors under inputs, flash toast, redirect). Category → sub-category cascade, brand/unit/tax dropdowns (a currently selected inactive row stays visible as "(inactive)"), prices, barcode/SKU, reorder level, active and online flags. Create lands on the new product's detail page so variants can be added.
- `Product/SetActive` (POST): soft activate/deactivate. **No hard delete.**
- `Product/Search` and `Product/FindByBarcode`: JSON for pickers (contracts below).
- Manage buttons (add/edit/activate) are hidden without `products.manage`; the server enforces the same permission.
- Platform Super Admin (no store): the stock card is replaced by a hint to sign in as a store user.

## Filters / Search / Sorting
- Query string: `Search`, `FkCategory`, `FkSubcategory`, `FkBrand`, `ActiveOnly` (`true|false`, absent = all), `SortBy` (`productname|productcode|category|sellingprice|isactive`), `SortDesc`, `Page`, `PageSize` (10/25/50/100, default 25, max 100).
- A sub-category filter is dropped when it does not belong to the chosen category.

## Data
- Entities (repository only): `Product` → `products`, plus read-only joins to `Category`, `SubCategory`, `Brand`, `Unit`, `Tax`, `ProductVariant`, `Inventory`.
- Models: `Models/ProductModels.cs`
  - Create: `InProductCreateView` / `InputProductCreate` / `OutputProductCreate`; Update: `InProductUpdateView` / `InputProductUpdate` / `OutputProductUpdate`.
  - `InputProductSelect` / `OutputProductSelect` (names of category, sub-category, brand, unit, tax, variant count; `ToUpdateView()` feeds the edit form).
  - `InputProductDetail` / `OutputProductDetail` (+ `OutputProductStockItem`), `InputProductList` / `OutputProductList`, `InputProductSetActive` / `OutputProductSetActive`.
  - `InputProductCheck` / `OutputProductCheck` (facts the service uses to validate), `InputProductLookup` / `OutputProductLookup` (+ `OutputProductLookupItem`).
  - Pickers: `InputProductSearch`, `InputProductBarcode`, `OutputProductSearchItem`.
- Fill (Filter API mode 1): `FillProduct` in `FilterModels.cs` (unchanged).

## Database Dependencies
- `products` (`fk_category`, `fk_subcategory`, `fk_brand`, `fk_unit`, `fk_tax` all optional), `product_variants`, `inventory` (read-only stock card), `categories`, `sub_categories`, `brands`, `units`, `taxes`.
- Unique constraints used as the race backstop: `uq_products_productcode`, `uq_products_sku`, `uq_products_barcode` (they also cover cancelled rows).

## API & Data Access
- Controller: `ProductController` → `IProductService` / `ProductService` → `IProductRepository` / `ProductRepository` (EF Core, no SQL).
- Views: `Views/Product/` (`Products`, `ProductDetails`, `CreateProduct`, `EditProduct`), loaded with explicit `~/Views/Product/…` paths.
- JS: `wwwroot/js/product/product.js` on top of `wwwroot/js/sc-ajax.js`.

| Route | Verb | Permission | Notes |
|-------|------|------------|-------|
| `/Product` | GET | `products.view` | List |
| `/Product/Details/{id}` | GET | `products.view` | Detail; 404 when missing/cancelled |
| `/Product/Create` | GET, POST | `products.manage` | POST returns `redirectUrl` to the detail page |
| `/Product/Edit/{id}` | GET, POST | `products.manage` | The route id wins over the posted id |
| `/Product/SetActive` | POST | `products.manage` | Params `id`, `isActive` |
| `/Product/Search` | GET | signed in + `PRODUCTS` module | Params `term`, `take` (1-50, default 20), `expandVariants` (default true) |
| `/Product/FindByBarcode` | GET | signed in + `PRODUCTS` module | Param `barcode` |

All of them sit behind `[Authorize]` + `[ModuleAccess("PRODUCTS")]`; every mutating POST is `[ValidateAntiForgeryToken]`, validates with `CommonHelpers.ValidateModelState` and answers with `CommonHelpers.SuccessResponse`.

### JSON contracts
- `GET /Product/Search` → `CommonResponseModel` envelope, rows in `data`. Row fields: `idProduct`, `idProductVariant` (null on product rows), `productCode`, `productName`, `variantName`, `sku`, `barcode`, `sellingPrice` (variant rows already include the additional price), `mrp`, `costPrice`, `fkUnit`, `unitName`, `unitCode`, `fkTax`, `taxRate`, `taxInclusive`, `isActive`, `displayName`. Only active, non-cancelled products and variants of active products are returned. With `expandVariants=true` a product that has active variants is represented by its variants only. Exact barcode/SKU/code hits are listed first, then by name.
- `GET /Product/FindByBarcode?barcode=` → **bare JSON object** with the same fields, `400` for a blank barcode, `404` when nothing active matches. Match order: variant barcode, product barcode, variant SKU, product SKU (case-insensitive, exact).
- `costPrice` is `null` unless the caller has `products.manage`, `purchases.view` or `purchases.manage` (`products.view` alone never sees costs). The detail page hides the cost row with the same rule.
- Sub-categories for the form cascade come from `GET Catalog/SubCategoryLookup?fkCategory=N` (owned by `subcategory.md`).

## UI Rules & Conventions
- Follow `ADMIN.md` → *Front-end conventions*; list screens use `_PageHeader`, `_StatusBadge`, `_Pager`, `_ConfirmModal` and `data-sc-post` / `data-sc-auto-submit`.
- Page state lives in the query string, so Back/refresh keep filters; create/edit/detail pass a local `returnUrl` (checked with `Url.IsLocalUrl`).
- Scanner friendly: SKU and barcode inputs ignore Enter (`data-no-enter-submit`).
- Everything is rendered with Razor encoding; JS uses `.text()` / `SmartCartAjax.escapeHtml`. No `Html.Raw` on data.

## Business Rules
- Soft cancel only; no hard delete. Deactivating a product hides it (and its variants) from pickers and `FindByBarcode` but keeps history.
- Product code is unique (case-insensitive, cancelled rows included). SKU and barcode are optional, stored as `NULL` when blank, and unique **across products and variants** (case-insensitive).
- Category, sub-category, brand, unit and tax must exist and be non-cancelled; the sub-category must belong to the chosen category. Violations come back as field errors; a database unique/foreign-key violation (race) maps to 409.
- Money is rounded to 2 decimals, reorder level to 3. Prices from the client are only stored; picker prices are always re-read from the database.
- Company-wide master — no `fk_store`; stock is per store.

## Dependencies & Related Modules
- `category.md`, `subcategory.md`, `brand.md`, `unit.md`, `tax.md`, `variant.md`, `inventory.md`, `purchase.md`, `pos.md`, `filter.md`

## Notes
- **Sidebar:** Products → `Product/Index` (`bi-box`, `products.view`, module `PRODUCTS`). The layout still points at the removed `Catalog/Products`; it must be switched to `Product/Index`, and `Views/Home/Index.cshtml` ("Manage products") likewise.
- **Known gaps:** no product images (no entity/columns); no bulk import/export or bulk activate; stock card only for store users; a product's active flag does not cascade to its variants (variants simply stop being offered while the product is inactive).
- `DecimalLength` validation reads decimals with the current culture; the app is expected to run with an invariant/en culture.
