using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class ProductReviewService : IProductReviewService
{
    private readonly IProductReviewRepository _repo;

    public ProductReviewService(IProductReviewRepository repo) => _repo = repo;

    public Task<OutputProductReviewCreate> CreateReviewAsync(
        InputProductReviewCreate input, CancellationToken ct = default) =>
        _repo.CreateReviewAsync(input, ct);

    public Task<OutputProductReviewList> ListReviewsAsync(
        InputProductReviewList input, CancellationToken ct = default) =>
        _repo.ListReviewsAsync(input, ct);

    public Task<OutputProductReviewHide> HideReviewAsync(
        InputProductReviewHide input, CancellationToken ct = default) =>
        _repo.HideReviewAsync(input, ct);

    public Task<OutputProductReviewDelete> DeleteReviewAsync(
        InputProductReviewDelete input, CancellationToken ct = default) =>
        _repo.DeleteReviewAsync(input, ct);
}
