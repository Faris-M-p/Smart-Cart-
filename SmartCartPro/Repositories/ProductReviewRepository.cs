using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

/// <summary>
/// Product review repository.
/// Business rules enforced here:
///   • Customer must have a qualifying online_order containing fk_onlineorderitem.
///   • Uniqueness: one review per (fk_customer, fk_onlineorderitem).
///   • Edit/delete allowed only within 7 days of order purchase date.
/// </summary>
public class ProductReviewRepository : IProductReviewRepository
{
    private readonly SmartCartDbContext _db;

    public ProductReviewRepository(SmartCartDbContext db) => _db = db;

    public async Task<OutputProductReviewCreate> CreateReviewAsync(
        InputProductReviewCreate input, CancellationToken ct = default)
    {
        // 1. Verify the order item belongs to an order that includes the product and the customer.
        var orderItem = await _db.OnlineOrderItems
            .Include(i => i.Order)
            .FirstOrDefaultAsync(
                i => i.IdOnlineOrderItem == input.FkOnlineOrderItem
                  && i.FkProduct == input.FkProduct
                  && i.Order != null && i.Order.FkCustomer == input.FkCustomer,
                ct);

        if (orderItem is null)
            return new OutputProductReviewCreate
            {
                ResponseCode = 1,
                ResponseMsg  = "No qualifying purchase found for this customer and product.",
                StatusCode   = 422
            };

        // 2. Check if a review already exists for this order item.
        var existing = await _db.ProductReviews
            .AnyAsync(r => r.FkCustomer == input.FkCustomer
                        && r.FkOnlineOrderItem == input.FkOnlineOrderItem
                        && !r.Cancelled, ct);

        if (existing)
            return new OutputProductReviewCreate
            {
                ResponseCode = 2,
                ResponseMsg  = "A review already exists for this order item.",
                StatusCode   = 409
            };

        var review = new ProductReview
        {
            FkProduct        = input.FkProduct,
            FkCustomer       = input.FkCustomer,
            FkStore          = input.FkStore,
            FkOnlineOrder    = orderItem.FkOnlineOrder,
            FkOnlineOrderItem = input.FkOnlineOrderItem,
            Rating           = input.Rating,
            Title            = input.Title,
            ReviewText       = input.ReviewText,
            IsApproved       = false, // requires admin approval
            CreatedAt        = DateTime.UtcNow
        };

        _db.ProductReviews.Add(review);
        await _db.SaveChangesAsync(ct);

        return new OutputProductReviewCreate
        {
            ResponseCode    = 0,
            ResponseMsg     = "Review submitted and pending approval.",
            StatusCode      = 201,
            IdProductReview = review.IdProductReview
        };
    }

    public async Task<OutputProductReviewList> ListReviewsAsync(
        InputProductReviewList input, CancellationToken ct = default)
    {
        var q = _db.ProductReviews.AsQueryable();

        if (!input.IncludeCancelled)
            q = q.Where(r => !r.Cancelled);

        if (input.FkProduct.HasValue)
            q = q.Where(r => r.FkProduct == input.FkProduct.Value);

        if (input.FkStore.HasValue)
            q = q.Where(r => r.FkStore == input.FkStore.Value);

        if (input.IsApproved.HasValue)
            q = q.Where(r => r.IsApproved == input.IsApproved.Value);

        if (input.Rating is >= 1 and <= 5)
            q = q.Where(r => r.Rating == input.Rating.Value);

        if (input.From.HasValue)
            q = q.Where(r => r.CreatedAt >= input.From.Value);

        if (input.To.HasValue)
        {
            var to = input.To.Value.TimeOfDay == TimeSpan.Zero
                ? input.To.Value.Date.AddDays(1)
                : input.To.Value;
            q = q.Where(r => r.CreatedAt < to);
        }

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            var term = input.Search.Trim().ToLower();
            q = q.Where(r =>
                (r.Title != null && r.Title.ToLower().Contains(term))
                || (r.ReviewText != null && r.ReviewText.ToLower().Contains(term))
                || (r.Product != null && r.Product.ProductName.ToLower().Contains(term))
                || (r.Customer != null && r.Customer.FullName.ToLower().Contains(term)));
        }

        q = (input.SortBy ?? string.Empty).ToLowerInvariant() switch
        {
            "rating" => input.SortDesc
                ? q.OrderByDescending(r => r.Rating).ThenByDescending(r => r.CreatedAt)
                : q.OrderBy(r => r.Rating).ThenByDescending(r => r.CreatedAt),
            "productname" => input.SortDesc
                ? q.OrderByDescending(r => r.Product != null ? r.Product.ProductName : string.Empty)
                : q.OrderBy(r => r.Product != null ? r.Product.ProductName : string.Empty),
            "customername" => input.SortDesc
                ? q.OrderByDescending(r => r.Customer != null ? r.Customer.FullName : string.Empty)
                : q.OrderBy(r => r.Customer != null ? r.Customer.FullName : string.Empty),
            "isapproved" => input.SortDesc
                ? q.OrderByDescending(r => r.IsApproved).ThenByDescending(r => r.CreatedAt)
                : q.OrderBy(r => r.IsApproved).ThenByDescending(r => r.CreatedAt),
            _ => input.SortDesc
                ? q.OrderByDescending(r => r.CreatedAt)
                : q.OrderBy(r => r.CreatedAt)
        };

        var total = await q.CountAsync(ct);

        var rows = await q
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .Select(r => new OutputProductReviewListItem
            {
                IdProductReview = r.IdProductReview,
                FkProduct       = r.FkProduct,
                ProductName     = r.Product != null ? r.Product.ProductName : string.Empty,
                FkCustomer      = r.FkCustomer,
                CustomerName    = r.Customer != null ? r.Customer.FullName : string.Empty,
                Rating          = r.Rating,
                Title           = r.Title,
                ReviewText      = r.ReviewText,
                IsApproved      = r.IsApproved,
                Cancelled       = r.Cancelled,
                CreatedAt       = r.CreatedAt
            })
            .ToListAsync(ct);

        return new OutputProductReviewList
        {
            TotalCount = total,
            Page       = input.Page,
            PageSize   = input.PageSize,
            Items      = rows
        };
    }

    public async Task<OutputProductReviewHide> HideReviewAsync(
        InputProductReviewHide input, CancellationToken ct = default)
    {
        var review = await _db.ProductReviews.FindAsync([input.IdProductReview], ct);

        if (review is null)
            return new OutputProductReviewHide
                { ResponseCode = 1, ResponseMsg = "Review not found.", StatusCode = 404 };

        review.IsApproved = input.IsApproved;
        review.UpdatedAt  = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return new OutputProductReviewHide
        {
            ResponseCode = 0,
            ResponseMsg  = input.IsApproved ? "Review approved / made visible." : "Review hidden.",
            StatusCode   = 200
        };
    }

    public async Task<OutputProductReviewDelete> DeleteReviewAsync(
        InputProductReviewDelete input, CancellationToken ct = default)
    {
        var review = await _db.ProductReviews
            .FirstOrDefaultAsync(r => r.IdProductReview == input.IdProductReview, ct);

        if (review is null)
            return new OutputProductReviewDelete
                { ResponseCode = 1, ResponseMsg = "Review not found.", StatusCode = 404 };

        if (review.FkCustomer != input.RequestingCustomerId)
            return new OutputProductReviewDelete
                { ResponseCode = 2, ResponseMsg = "Not authorised to delete this review.", StatusCode = 403 };

        // Edit/delete allowed only within 7 days of order date.
        var order = review.FkOnlineOrder.HasValue
            ? await _db.Orders.FindAsync([review.FkOnlineOrder.Value], ct)
            : null;
        if (order is not null && (DateTime.UtcNow - order.OrderDate).TotalDays > 7)
            return new OutputProductReviewDelete
            {
                ResponseCode = 3,
                ResponseMsg  = "Reviews can only be deleted within 7 days of purchase.",
                StatusCode   = 422
            };

        review.Cancelled      = true;
        review.CancelledOn    = DateTime.UtcNow;
        review.CancelledReason = "Customer deleted";
        review.UpdatedAt      = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return new OutputProductReviewDelete
            { ResponseCode = 0, ResponseMsg = "Review deleted.", StatusCode = 200 };
    }
}
