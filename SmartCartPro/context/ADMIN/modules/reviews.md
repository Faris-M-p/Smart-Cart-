# Product Reviews — Phase 7

## Status
IMPLEMENTED (Phase 7) — admin moderation only; customer-facing submission is service-only (no MVC view yet).

## Table
`product_reviews` (with Phase 7 patch column `fk_onlineorderitem`)

## Business Rules
1. **Purchase required**: Customer must have a `fk_onlineorderitem` belonging to one of their orders.
2. **One per order item**: Unique constraint `(fk_customer, fk_onlineorderitem)` (partial, where `fk_onlineorderitem IS NOT NULL`).
3. **Edit/delete window**: Only within 7 days of the order's `orderdate`.
4. **Admin hide**: Sets `isapproved = false` (review remains in DB, hidden from public).
5. New reviews are `isapproved = false` by default (pending moderation).

## SQL Patch
`Database-Postgres/03_Patches/V007__Phase7OnlineOrders.sql`
Adds `fk_onlineorderitem INT NULL` + FK + unique partial index.

## Files
| Layer | File |
|---|---|
| Entity | `Entities/ProductReview.cs` |
| Models | `Models/ProductReviewModels.cs` |
| Repository interface | `Repositories/Interfaces/IProductReviewRepository.cs` |
| Repository impl | `Repositories/ProductReviewRepository.cs` |
| Service interface | `Services/Interfaces/IProductReviewService.cs` |
| Service impl | `Services/ProductReviewService.cs` |
| Admin controller | `Controllers/ProductReviewController.cs` |

## Admin Endpoints
| Method | Route | Permission | Description |
|---|---|---|---|
| GET | `/ProductReview` | products.manage | Paginated list with filters |
| GET | `/ProductReview/ListJson` | products.manage | JSON list |
| POST | `/ProductReview/Hide/{id}` | products.manage | Set isapproved = false |
| POST | `/ProductReview/Approve/{id}` | products.manage | Set isapproved = true |
| POST | `/ProductReview/HideJson` | products.manage | JSON hide/approve toggle |

## Customer Service Methods (no view — callable from future portal)
- `IProductReviewService.CreateReviewAsync` — submit review
- `IProductReviewService.DeleteReviewAsync` — soft-cancel within 7-day window
