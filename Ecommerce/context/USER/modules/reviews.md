# User Module — Ratings & Reviews (order-item based)

## Module Overview
- **Area:** USER (Storefront)
- **Module Name:** Ratings & Reviews
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Flipkart-style reviews. A customer rates a product **from My Orders → Order Details**, once per **order item** (never per order and never per quantity). The Product Details page only *shows* ratings and reviews (read-only) — there is **no "write a review" button or form on the product page**. Uses the existing `ratings` table with two new nullable link columns; **no second reviews table.**

## Business Rules
| Rule | Behaviour |
| --- | --- |
| Who | Only the customer who bought the item (identity from the JWT cookie — never from the request body). |
| What | One review per **order item**. An item bought with quantity 10 still has exactly one review slot. |
| Many items in one order | Every item has its own independent review (review item 1 does not touch items 2…N). |
| Same product, different orders | Each order item can have its own review (they appear as separate reviews on the product page). |
| Eligible | The order is `Delivered` (not cancelled). Not delivered / cancelled → no review action at all. |
| Review window | **7 days from the delivery date**, enforced in the database procedures (the browser only displays what the server returns). Delivery date = `shipping.shippingdate` of the non-cancelled `Delivered` shipping row (fallback `orders.orderdate`). |
| Inside the window | Add (once), edit and delete are allowed. After delete the item returns to "Add Review" (while still inside the window). |
| After the window | Add / edit / delete are all rejected by the API (`-6`, HTTP 403). A review written earlier stays visible and is locked ("Review period ended"). |
| Validation | Rating 1–5 required; text optional, ≤ 1000 chars (HTML-escaped on output). No title (table has no column). |
| Delete | Soft delete (`cancelled=TRUE`, reason `Deleted by customer`). |
| Admin removal | Soft delete by admin (`Removed by admin…`). The item is then reviewable again inside the window. |

## Order Details UI states (per item, `views/Orders/Details.cshtml` + `wwwroot/js/shop-order-reviews.js`)
| `reviewStatus` | What the customer sees |
| --- | --- |
| `CanReview` | 5 quick-rate stars + "You can review this product until 13 Oct 2026 · 6 days left." + **Add Review** (clicking a star opens the modal with that rating pre-selected). |
| `Reviewed` | "✓ Reviewed", stars, text, "You can edit or delete your review until …", **Edit Review** / **Delete Review** (delete opens a "Delete Review?" confirmation: Cancel / Delete). |
| `Locked` | "✓ Reviewed", stars, text, "Review period ended" (no actions). |
| `Expired` | "Review period ended" (never reviewed, window over). |
| `NotEligible` | Nothing is rendered (not delivered / cancelled). |

The expiry text, days left and every `can…` flag come from the backend (`/Reviews/Order`). If a page was left open and the window ends, the server rejects the call, the modal closes, a message is shown and the item switches to "Review period ended".

## Product Details (read-only)
- **Rating summary near product name** (`#details-rating`), **Ratings & Reviews** section (`#reviews`): average, totals, 5→1 star distribution, review list, pagination (10/page), empty / loading / error states. All numbers come from the DB.
- **Verified Purchase** badge = review is linked to an order item (`ratings.fk_orderitem IS NOT NULL`).
- The customer's own review is pinned first with a "You" tag and a "View in your order" link (to `/Orders/Details/{id}`).
- A static hint explains: "Reviews come from customers who bought this product. Bought it? Rate it from My Orders within 7 days of delivery."

## Legacy / sample reviews
The 20 seeded sample ratings have no order link (`fk_order` / `fk_orderitem` = NULL). They are still shown and counted on the product page, **without** a Verified Purchase badge, cannot be edited/deleted by customers (`-6`), and can be removed by an admin. No fake order-item links are created.

## API (`Controllers/ReviewsController.cs`, conventional routes)
| Route | Method | Auth | Notes |
| --- | --- | --- | --- |
| `/Reviews/Get?productId=&pageIndex=&pageSize=` | GET | public | `ProductReviewsResponse { summary, distribution[5], reviews[], pageIndex, pageSize, totalCount, totalPages }`; `reviews[]` has `isVerifiedPurchase`, `isMine`, `myOrderId`. 404 unknown/hidden product. |
| `/Reviews/Order?orderId=` | GET | customer | One status row per item of the caller's **own** order: `orderItemId, orderId, productId, productName, variantLabel, quantity, isEligible, deliveredOn, reviewExpiresOn, reviewWindowExpired, daysLeft, reviewId, rating, review, reviewedOn, canAddReview, canEditReview, canDeleteReview, reviewStatus`. Someone else's order → 404. |
| `/Reviews/Submit` | POST | customer | `{ orderId, orderItemId, productId, rating, review }` |
| `/Reviews/Update` | POST | customer (owner) | `{ reviewId, rating, review }` |
| `/Reviews/Delete` | POST | customer (owner) | `{ reviewId }` |

`CommonResponse.ResponseCode`: `>0` new/affected id, `-1` validation / not signed in (400/401), `-2` already reviewed (409), `-3` order not delivered (403), `-4` review not found (404), `-5` not your order / item mismatch (403 — unknown ids also return this so existence is not leaked), `-6` review window over / locked / legacy review (403).

## Security checks (all in the stored procedures)
1. Signed-in user comes from the token (any client `userId` is ignored — the DTOs have no such field).
2. The order belongs to the user.
3. The order item belongs to that order, and the product id matches the item.
4. Order is delivered, not cancelled.
5. Within 7 days of delivery (server clock / DB delivery date).
6. No active review exists for the item — enforced three ways: `pg_advisory_xact_lock(hashtextextended('review:orderitem:<id>', 0))`, an `EXISTS` check, and the partial unique index `ux_ratings_orderitem_active`. Parallel double-submits yield exactly one review.
7. Update / delete additionally require the review to belong to the caller and to be inside the window.

## Data
- DTOs: `Models/ReviewModel.cs` — `ReviewSubmitInput`, `ReviewUpdateInput`, `ReviewDeleteInput`, `ReviewSummary`, `ReviewDistributionRow`, `ReviewItem`, `ProductReviewsResponse`, `OrderItemReviewStatus`.
- `Models/OrderModel.cs`: `OrderLine : CartLine` adds `OrderItemId` (needed by the Order Details page to attach the review slot).
- Interface / repository: `Interface/ReviewInterface.cs`, `Repository/ReviewRepository.cs` (Dapper; registered in `Program.cs`). Constants in `StoredProcedures.Review`.

## Database Dependencies
- Table `ratings` + `fk_order`, `fk_orderitem` (nullable, FKs to `orders` / `orderitems`), partial unique index `ux_ratings_orderitem_active (fk_orderitem) WHERE fk_orderitem IS NOT NULL AND cancelled IS NOT TRUE`, `ix_ratings_product_active`. Defined idempotently in `01_Tables/ratings.sql` (called from `01_Tables/patch.sql`).
- Stored procedures (`02_Procedures/02_Users/06_Reviews/`): `get_product_reviews`, `get_order_item_reviews`, `submit_order_item_review`, `update_order_item_review`, `delete_order_item_review`. `Patch.sql` there first drops every overload of the retired product-based procs (`submit/update/delete_product_review`) so they cannot be called any more. Registered in `Database-Patch.sql` and `Database.sql`.

## UI Rules & Conventions
- Styles: `wwwroot/css/shop-reviews.css` (summary, bars, buttons, order-item slot, modal) and `shop-details.css` (details page only). The theme's `.modal` is `visibility:hidden` until `.is-visible`, so `.shop-review-modal.show` forces visibility. Reuses the existing modal/button/card look.
- Responsive: summary grid 3 → 2 (≤1000px) → 1 col (≤560px); modal buttons stack full width on phones.

## Dependencies & Related Modules
- `USER/modules/orders.md` — host page (Order Details) and delivery data.
- `USER/modules/shop.md` — read-only display on Product Details.
- `USER/modules/account.md` — customer login / JWT.
- `ADMIN/modules/review.md` — moderation of the same rows.
