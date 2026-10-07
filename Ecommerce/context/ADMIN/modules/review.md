# Admin Module — Ratings & Reviews

## Module Overview
- **Area:** ADMIN (Back Office)
- **Module Name:** Ratings & Reviews (permission module `Ratings`)
- **Current Status:** `IMPLEMENTED`

## Purpose & Responsibilities
Read-only listing and moderation (removal) of customer reviews stored in the existing `ratings` table.

## Implemented Features
- **Review listing:** paginated table — product (link to storefront details), customer + email, star rating, purchase column ("Verified Purchase · {order number}" for order-item reviews, "Sample review (no order)" for the legacy seeded rows), review text, date, status (Active / Removed + reason; customer self-deletes show `Deleted by customer`).
- **Filters:** search (product, customer, email, review text), rating 1–5, status `Active` (default) / `Removed` / `All`.
- **Remove review:** modal with optional reason (≤ 255 chars). Soft delete: `cancelled=TRUE`, `cancelledon=NOW()`, `cancelledreason = 'Removed by admin (employee #N): <reason>'`. The review disappears from the storefront and averages recalculate; the order item becomes reviewable again for the customer while the 7-day window is still open (after the window nothing can be added).
- Admins cannot edit review content or rating (moderation only).

## Permissions
- `Ratings.View` — page, sidebar item ("Ratings & Reviews", icon `ti ti-star`), list endpoint.
- `Ratings.Delete` — remove endpoint and the Remove button.
- Module `Ratings` seeded in `05_Seed/modules.sql` (order 135); permissions in `05_Seed/permissions.sql`. Admin role receives them through the "all remaining permissions" seed; Manager role is granted both in `05_Seed/complete_dummy_data.sql`. Other roles (e.g. Cashier) do not have them → HTTP 403.
- `UserRoleHelper` already listed "Ratings" under the "Customers" permission group, so the role-permission tree shows it automatically.
- `admin-permissions.js` `PAGE_RULES` guards `/Admin/Review` with `Ratings.View`.

## API & Data Access
- Controller: `Controllers/Admin/ReviewController.cs` (`[Route("Admin/Review")]`): `Index` (page), `POST GetReviewList`, `POST Remove`. Employee id for the audit reason comes from `AdminAuthHelper.TryGetEmployeeId`.
- Interface / repository: `Interface/Admin/IAdminReviewInterface.cs` / `Repository/Admin/AdminReviewRepository.cs` (**EF Core**, consistent with other admin modules). `EcommerceDbContext` gained `DbSet<RatingEntity> Ratings`.
- Models: `Models/Admin/AdminReviewModel.cs` — `AdminReviewListInputVIEW`, `AdminReviewRemoveInputVIEW`, `AdminReviewListInput`, `AdminReviewListItem` (includes `OrderId`, `OrderNumber`, `IsVerifiedPurchase`). `RatingEntity` maps the new nullable `FK_Order` / `FK_OrderItem` columns.
- Response: `ApiResponse<TableOutput<AdminReviewListItem>>`.
- View: `views/Admin/Review/Index.cshtml`.

## Business Rules
- Soft delete only; rows are never physically deleted.
- Verified Purchase = the review is linked to an order item (`ratings.fk_orderitem IS NOT NULL`) — the same rule as the storefront. The legacy sample reviews (no order link) are listed as "Sample review (no order)" and can be removed like any other.
- Customers can no longer edit/delete after 7 days from delivery; admin removal is not time-limited.

## Dependencies & Related Modules
- `USER/modules/reviews.md` — customer-facing side.
- `ADMIN/modules/user-role.md` — assigning `Ratings.*` permissions to roles.
