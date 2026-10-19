using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface IProductReviewRepository
{
    Task<OutputProductReviewCreate> CreateReviewAsync(InputProductReviewCreate input, CancellationToken ct = default);
    Task<OutputProductReviewList> ListReviewsAsync(InputProductReviewList input, CancellationToken ct = default);
    Task<OutputProductReviewHide> HideReviewAsync(InputProductReviewHide input, CancellationToken ct = default);
    Task<OutputProductReviewDelete> DeleteReviewAsync(InputProductReviewDelete input, CancellationToken ct = default);
}
