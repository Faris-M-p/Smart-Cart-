using Ecommerce.Interface;
using Ecommerce.Models;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.ReviewModel;

namespace Ecommerce.Repository
{
    public class ReviewRepository : ReviewInterface
    {
        private readonly IDataAccessDapper _dapper;

        public ReviewRepository(IDataAccessDapper dapper)
        {
            _dapper = dapper;
        }

        public async Task<ProductReviewsResponse?> GetProductReviewsAsync(int productId, int userId, int pageIndex, int pageSize)
        {
            if (productId < 1)
            {
                return null;
            }

            var index = pageIndex < 1 ? 1 : pageIndex;
            var size = pageSize < 1 ? ReviewModel.DefaultPageSize : Math.Min(pageSize, ReviewModel.MaxPageSize);

            var multi = await _dapper.GetMultipleListsByProcedure<ReviewSummary, ReviewDistributionRow, ReviewItem, object>(
                StoredProcedures.Review.GetProductReviews,
                new
                {
                    ProductId = productId,
                    UserId = userId,
                    PageIndex = index,
                    PageSize = size
                },
                new[] { "p_result", "p_result2", "p_result3" });

            var summary = multi.TableOut1?.FirstOrDefault();
            if (summary == null)
            {
                return null;
            }

            return new ProductReviewsResponse
            {
                Summary = summary,
                Distribution = multi.TableOut2 ?? new List<ReviewDistributionRow>(),
                Reviews = multi.TableOut3 ?? new List<ReviewItem>(),
                PageIndex = index,
                PageSize = size,
                TotalCount = summary.TotalRatings,
                TotalPages = summary.TotalRatings == 0 ? 0 : (int)Math.Ceiling(summary.TotalRatings / (double)size)
            };
        }

        public async Task<List<OrderItemReviewStatus>> GetOrderItemReviewsAsync(int userId, int orderId)
        {
            if (userId < 1 || orderId < 1)
            {
                return new List<OrderItemReviewStatus>();
            }

            return await _dapper.GetListByProcedure<OrderItemReviewStatus, object>(
                StoredProcedures.Review.GetOrderItemReviews,
                new
                {
                    UserId = userId,
                    OrderId = orderId
                });
        }

        public async Task<CommonResponse> SubmitReviewAsync(int userId, ReviewSubmitInput input)
        {
            return await _dapper.GetSingleByProcedure<CommonResponse, object>(
                StoredProcedures.Review.SubmitOrderItemReview,
                new
                {
                    UserId = userId,
                    OrderId = input.OrderId,
                    OrderItemId = input.OrderItemId,
                    ProductId = input.ProductId,
                    Rating = input.Rating,
                    Review = input.Review
                }) ?? StatusFail();
        }

        public async Task<CommonResponse> UpdateReviewAsync(int userId, int reviewId, int rating, string? review)
        {
            return await _dapper.GetSingleByProcedure<CommonResponse, object>(
                StoredProcedures.Review.UpdateOrderItemReview,
                new
                {
                    UserId = userId,
                    ReviewId = reviewId,
                    Rating = rating,
                    Review = review
                }) ?? StatusFail();
        }

        public async Task<CommonResponse> DeleteReviewAsync(int userId, int reviewId)
        {
            return await _dapper.GetSingleByProcedure<CommonResponse, object>(
                StoredProcedures.Review.DeleteOrderItemReview,
                new
                {
                    UserId = userId,
                    ReviewId = reviewId
                }) ?? StatusFail();
        }

        private static CommonResponse StatusFail() => new()
        {
            ResponseCode = -1,
            StatusCode = false,
            ResponseMsg = "No response from stored procedure."
        };
    }
}
