using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.ReviewModel;

namespace Ecommerce.Interface
{
    public interface ReviewInterface
    {
        /// <summary>Read-only reviews for the Product Details page. Returns null when the product is not available in the shop.</summary>
        Task<ProductReviewsResponse?> GetProductReviewsAsync(int productId, int userId, int pageIndex, int pageSize);

        /// <summary>Review state of every item of one of the customer's own orders (empty for someone else's order).</summary>
        Task<List<OrderItemReviewStatus>> GetOrderItemReviewsAsync(int userId, int orderId);

        Task<CommonResponse> SubmitReviewAsync(int userId, ReviewSubmitInput input);

        Task<CommonResponse> UpdateReviewAsync(int userId, int reviewId, int rating, string? review);

        Task<CommonResponse> DeleteReviewAsync(int userId, int reviewId);
    }
}
