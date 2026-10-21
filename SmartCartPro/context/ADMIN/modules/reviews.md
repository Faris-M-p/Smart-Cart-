# Admin Module — Product Reviews

## Module Name & Area
- **Area:** ADMIN
- **Module Name:** Product reviews
- **Current Status:** `IMPLEMENTED` — admin moderation; customer-facing submit/delete remain service-only (no MVC view)

## Purpose & Responsibilities
Moderate customer product reviews. New reviews start unapproved. Approve makes them visible; Hide sets `isapproved = false` (row stays).

## Implemented Features
- Index filters: status (pending/approved), rating 1–5, search, date range, sort, pager
- Approve / Hide via AJAX (`data-sc-post` + antiforgery)
- Star display with Bootstrap Icons (`bi-star` / `bi-star-fill`)
- Created timestamps via `TimeDisplay.Iso` + `data-sc-utc`
- JSON list/hide endpoints return `CommonResponseModel`

## Filters / Search / Sorting
- `Status` (`pending` / `approved` / all), `Rating`, `Search`, `From`, `To`, `SortBy`, `SortDesc`, `Page`, `PageSize`
- Search: product name, customer name, title, review text
- Sort: `createdat` (default desc), `rating`, `productname`, `customername`, `isapproved`

## Data
- Create/delete models unchanged for the future portal (`InProductReviewCreateView`, `InputProductReviewCreate`, `InputProductReviewDelete`, …)
- `InputProductReviewList` / `OutputProductReviewList` / `OutputProductReviewListItem`
- `InputProductReviewHide` / `OutputProductReviewHide`
- Entity `ProductReview` stays in the repository

## Database Dependencies
- `product_reviews` (optional `fk_onlineorderitem` from V007)
- Purchase-required and one-review-per-order-item rules stay in `CreateReviewAsync`

## API & Data Access
| Method | Route | Permission | Description |
|---|---|---|---|
| GET | `/ProductReview` | products.manage | Paged moderation list |
| GET | `/ProductReview/ListJson` | products.manage | AJAX envelope list |
| POST | `/ProductReview/Approve/{id}` | products.manage | Set isapproved = true |
| POST | `/ProductReview/Hide/{id}` | products.manage | Set isapproved = false |
| POST | `/ProductReview/HideJson` | products.manage | JSON toggle (`IsApproved`) |

Controller: `[ModuleAccess("PRODUCTS")]`. Mutating POSTs have `[ValidateAntiForgeryToken]`.

Customer service methods (no admin view):
- `CreateReviewAsync` — submit (pending)
- `DeleteReviewAsync` — soft-cancel within 7 days of order date

## UI Rules & Conventions
- Category-style list; `wwwroot/js/reviews/reviews.js`
- Stars: `bi-star-fill text-warning` for filled, `bi-star text-muted` for empty
- Dates: `<time data-sc-utc="…" data-sc-format="datetime">`

## Business Rules
1. Purchase required (order item owned by the customer)
2. One review per `(fk_customer, fk_onlineorderitem)`
3. Customer delete only within 7 days of `orderdate`
4. Admin hide = `isapproved = false` (not a hard delete)
5. New reviews `isapproved = false`

## Dependencies & Related Modules
- `product.md`, `customers.md`, `online-orders.md`

## Notes
- Pending and hidden both use `isapproved = false`; the list Status filter cannot tell them apart.
