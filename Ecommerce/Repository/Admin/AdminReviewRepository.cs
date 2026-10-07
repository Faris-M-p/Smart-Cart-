using Ecommerce.DataAccess;
using Ecommerce.Interface.Admin;
using Microsoft.EntityFrameworkCore;
using static Ecommerce.Models.Admin.AdminReviewModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Repository.Admin
{
    public class AdminReviewRepository : IAdminReviewInterface
    {
        private readonly EcommerceDbContext _db;

        public AdminReviewRepository(EcommerceDbContext db)
        {
            _db = db;
        }

        public async Task<TableOutput<AdminReviewListItem>> GetReviewListAsync(AdminReviewListInput input)
        {
            input ??= new AdminReviewListInput();
            var pageIndex = input.PageIndex < 1 ? 1 : input.PageIndex;
            var pageSize = input.PageSize < 1 ? 10 : Math.Min(input.PageSize, 100);
            var search = (input.SearchText ?? string.Empty).Trim();
            var status = (input.Status ?? string.Empty).Trim();

            var query =
                from r in _db.Ratings.AsNoTracking()
                join u in _db.Users.AsNoTracking() on r.FK_User equals u.ID_User
                join p in _db.Products.AsNoTracking() on r.FK_Product equals p.ID_Product
                select new { r, u, p };

            if (string.Equals(status, "Removed", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.r.Cancelled == true);
            }
            else if (!string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(x => x.r.Cancelled != true);
            }

            if (input.Rating >= 1 && input.Rating <= 5)
            {
                var low = (decimal)input.Rating;
                var high = low + 1;
                query = query.Where(x => x.r.RatingValue >= low && x.r.RatingValue < high);
            }

            if (!string.IsNullOrEmpty(search))
            {
                var term = search.ToLowerInvariant();
                query = query.Where(x =>
                    x.p.Name.ToLower().Contains(term)
                    || x.u.Email.ToLower().Contains(term)
                    || x.u.UserName.ToLower().Contains(term)
                    || (x.u.FullName != null && x.u.FullName.ToLower().Contains(term))
                    || (x.r.Review != null && x.r.Review.ToLower().Contains(term)));
            }

            var totalCount = await query.CountAsync();

            var page = await query
                .OrderByDescending(x => x.r.CreatedAt)
                .ThenByDescending(x => x.r.ID_Rating)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    x.r.ID_Rating,
                    x.r.FK_Product,
                    ProductName = x.p.Name,
                    ProductSlug = x.p.Slug,
                    x.r.FK_User,
                    x.u.FullName,
                    x.u.UserName,
                    x.u.Email,
                    x.r.RatingValue,
                    x.r.Review,
                    x.r.CreatedAt,
                    Removed = x.r.Cancelled == true,
                    x.r.CancelledOn,
                    x.r.CancelledReason,
                    // Verified purchase = the review is attached to a purchased order item.
                    IsVerifiedPurchase = x.r.FK_OrderItem != null,
                    x.r.FK_Order,
                    OrderNumber = (
                        from o in _db.Orders
                        where o.ID_Order == x.r.FK_Order
                        select o.OrderNumber).FirstOrDefault()
                })
                .ToListAsync();

            var rows = page.Select(x => new AdminReviewListItem
            {
                ReviewId = x.ID_Rating,
                ProductId = x.FK_Product,
                ProductName = x.ProductName,
                ProductSlug = x.ProductSlug,
                CustomerId = x.FK_User,
                CustomerName = string.IsNullOrWhiteSpace(x.FullName) ? x.UserName : x.FullName!,
                CustomerEmail = x.Email,
                Rating = (int)Math.Round(x.RatingValue, MidpointRounding.AwayFromZero),
                Review = x.Review ?? string.Empty,
                CreatedAt = x.CreatedAt,
                IsVerifiedPurchase = x.IsVerifiedPurchase,
                OrderId = x.FK_Order ?? 0,
                OrderNumber = x.OrderNumber ?? (x.FK_Order.HasValue ? "SC" + x.FK_Order.Value.ToString("D6") : string.Empty),
                Removed = x.Removed,
                RemovedOn = x.CancelledOn,
                RemovedReason = x.CancelledReason ?? string.Empty
            }).ToList();

            return new TableOutput<AdminReviewListItem>
            {
                TableData = rows,
                TableSettings = new TableOutput_Settings
                {
                    TotalCount = totalCount,
                    PageIndex = pageIndex,
                    PageSize = pageSize
                }
            };
        }

        public async Task<CommonResponse> RemoveReviewAsync(int reviewId, int employeeId, string? reason)
        {
            var review = await _db.Ratings.FirstOrDefaultAsync(r => r.ID_Rating == reviewId);
            if (review == null)
            {
                return Fail("Review not found.");
            }

            if (review.Cancelled == true)
            {
                return Fail("This review has already been removed.");
            }

            var note = (reason ?? string.Empty).Trim();
            review.Cancelled = true;
            review.CancelledOn = DateTime.Now;
            review.CancelledReason = string.IsNullOrEmpty(note)
                ? $"Removed by admin (employee #{employeeId})"
                : $"Removed by admin (employee #{employeeId}): {note}";

            await _db.SaveChangesAsync();

            return new CommonResponse
            {
                ResponseCode = reviewId,
                StatusCode = true,
                ResponseMsg = "Review removed."
            };
        }

        private static CommonResponse Fail(string message) => new()
        {
            ResponseCode = -1,
            StatusCode = false,
            ResponseMsg = message
        };
    }
}
