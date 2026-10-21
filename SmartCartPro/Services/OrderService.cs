using SmartCartPro.Exceptions;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

/// <summary>
/// Online order service — Phase 7 rewrite.
/// Delegates to IOnlineOrderRepository; throws typed AppExceptions on failure.
/// </summary>
public class OnlineOrderService : IOnlineOrderService
{
    private static readonly int[] PageSizes = { 10, 25, 50, 100 };
    private static readonly string[] SortFields = { "orderdate", "ordernumber", "status", "totalamount", "customername" };
    private static readonly string[] Statuses = { "pending", "confirmed", "packed", "shipped", "delivered", "cancelled" };

    private readonly IOnlineOrderRepository _repo;

    public OnlineOrderService(IOnlineOrderRepository repo) => _repo = repo;

    public Task<OutputOnlineOrderSelect?> GetOrderAsync(
        InputOnlineOrderSelect input, CancellationToken ct = default) =>
        _repo.GetOrderAsync(input, ct);

    public async Task<OutputOnlineOrderList> ListOrdersAsync(
        InputOnlineOrderList input, CancellationToken ct = default)
    {
        input.Search = string.IsNullOrWhiteSpace(input.Search) ? null : input.Search.Trim();
        if (input.Search is { Length: > 100 }) input.Search = input.Search[..100];

        if (!string.IsNullOrWhiteSpace(input.Status)
            && Array.IndexOf(Statuses, input.Status.Trim().ToLowerInvariant()) < 0)
        {
            input.Status = null;
        }
        else if (!string.IsNullOrWhiteSpace(input.Status))
        {
            input.Status = input.Status.Trim().ToLowerInvariant();
        }

        input.Page = Math.Max(1, input.Page);
        if (Array.IndexOf(PageSizes, input.PageSize) < 0) input.PageSize = 25;

        var sort = (input.SortBy ?? string.Empty).Trim().ToLowerInvariant();
        input.SortBy = Array.IndexOf(SortFields, sort) >= 0 ? sort : "orderdate";

        var result = await _repo.ListOrdersAsync(input, ct);

        var lastPage = Math.Max(1, (int)Math.Ceiling(result.TotalCount / (double)input.PageSize));
        if (input.Page > lastPage)
        {
            input.Page = lastPage;
            result = await _repo.ListOrdersAsync(input, ct);
        }

        result.Filter = input;
        return result;
    }

    public async Task<OutputOnlineOrderStatusUpdate> UpdateStatusAsync(
        InputOnlineOrderStatusUpdate input, CancellationToken ct = default)
    {
        input.NewStatus = (input.NewStatus ?? string.Empty).Trim().ToLowerInvariant();
        input.Reason = string.IsNullOrWhiteSpace(input.Reason) ? null : input.Reason.Trim();

        var result = await _repo.UpdateStatusAsync(input, ct);
        if (!result.Ok)
            throw Failure(result.StatusCode, result.ResponseMsg, "Unable to update the order status.");
        return result;
    }

    private static AppException Failure(int statusCode, string? message, string fallback)
    {
        var text = string.IsNullOrWhiteSpace(message) ? fallback : message;
        var developer = $"Online order repository returned {statusCode}: {text}";
        return statusCode switch
        {
            404 => new ResourceNotFoundException(text, developer),
            409 => new ConflictException(text, developer),
            422 => new ValidationException(text, developer),
            _ => new UpdateFailedException(text, developer)
        };
    }
}
