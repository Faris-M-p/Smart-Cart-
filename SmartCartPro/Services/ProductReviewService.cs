using SmartCartPro.Exceptions;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class ProductReviewService : IProductReviewService
{
    private static readonly int[] PageSizes = { 10, 25, 50, 100 };
    private static readonly string[] SortFields = { "createdat", "rating", "productname", "customername", "isapproved" };

    private readonly IProductReviewRepository _repo;

    public ProductReviewService(IProductReviewRepository repo) => _repo = repo;

    public async Task<OutputProductReviewCreate> CreateReviewAsync(
        InputProductReviewCreate input, CancellationToken ct = default)
    {
        var result = await _repo.CreateReviewAsync(input, ct);
        if (!result.Ok)
            throw Failure(result.StatusCode, result.ResponseMsg, "Unable to submit the review.");
        return result;
    }

    public async Task<OutputProductReviewList> ListReviewsAsync(
        InputProductReviewList input, CancellationToken ct = default)
    {
        input.Search = string.IsNullOrWhiteSpace(input.Search) ? null : input.Search.Trim();
        if (input.Search is { Length: > 100 }) input.Search = input.Search[..100];

        if (input.Rating is < 1 or > 5) input.Rating = null;

        var status = (input.Status ?? string.Empty).Trim().ToLowerInvariant();
        if (status == "pending")
        {
            input.Status = "pending";
            input.IsApproved = false;
        }
        else if (status == "approved")
        {
            input.Status = "approved";
            input.IsApproved = true;
        }
        else
        {
            input.Status = null;
        }

        input.Page = Math.Max(1, input.Page);
        if (Array.IndexOf(PageSizes, input.PageSize) < 0) input.PageSize = 25;

        var sort = (input.SortBy ?? string.Empty).Trim().ToLowerInvariant();
        input.SortBy = Array.IndexOf(SortFields, sort) >= 0 ? sort : "createdat";

        var result = await _repo.ListReviewsAsync(input, ct);

        var lastPage = Math.Max(1, (int)Math.Ceiling(result.TotalCount / (double)input.PageSize));
        if (input.Page > lastPage)
        {
            input.Page = lastPage;
            result = await _repo.ListReviewsAsync(input, ct);
        }

        result.Filter = input;
        return result;
    }

    public async Task<OutputProductReviewHide> HideReviewAsync(
        InputProductReviewHide input, CancellationToken ct = default)
    {
        var result = await _repo.HideReviewAsync(input, ct);
        if (!result.Ok)
            throw Failure(result.StatusCode, result.ResponseMsg, "Unable to update the review.");
        return result;
    }

    public async Task<OutputProductReviewDelete> DeleteReviewAsync(
        InputProductReviewDelete input, CancellationToken ct = default)
    {
        var result = await _repo.DeleteReviewAsync(input, ct);
        if (!result.Ok)
            throw Failure(result.StatusCode, result.ResponseMsg, "Unable to delete the review.");
        return result;
    }

    private static AppException Failure(int statusCode, string? message, string fallback)
    {
        var text = string.IsNullOrWhiteSpace(message) ? fallback : message;
        var developer = $"Product review repository returned {statusCode}: {text}";
        return statusCode switch
        {
            403 => new ForbiddenException(text, developer),
            404 => new ResourceNotFoundException(text, developer),
            409 => new ConflictException(text, developer),
            422 => new ValidationException(text, developer),
            _ => new UpdateFailedException(text, developer)
        };
    }
}
