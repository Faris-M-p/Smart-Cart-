using SmartCartPro.Exceptions;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class CustomerService : ICustomerService
{
    private static readonly int[] PageSizes = { 10, 25, 50, 100 };
    private static readonly string[] SortFields = { "customercode", "fullname", "phone", "email", "isactive", "createdat" };

    private readonly ICustomerRepository _repo;

    public CustomerService(ICustomerRepository repo) => _repo = repo;

    public Task<OutputCustomerSelect?> GetCustomerAsync(InputCustomerSelect input, CancellationToken ct = default)
        => _repo.GetCustomerAsync(input, ct);

    public async Task<OutputCustomerList> ListCustomersAsync(InputCustomerList input, CancellationToken ct = default)
    {
        input.Search = Clean(input.Search);
        if (input.Search is { Length: > 100 }) input.Search = input.Search[..100];
        input.PhoneDigits = DigitsOnly(input.Search);

        input.Page = Math.Max(1, input.Page);
        if (Array.IndexOf(PageSizes, input.PageSize) < 0) input.PageSize = 25;

        var sort = (input.SortBy ?? string.Empty).Trim().ToLowerInvariant();
        input.SortBy = Array.IndexOf(SortFields, sort) >= 0 ? sort : "fullname";

        var result = await _repo.ListCustomersAsync(input, ct);

        var lastPage = Math.Max(1, (int)Math.Ceiling(result.TotalCount / (double)input.PageSize));
        if (input.Page > lastPage)
        {
            input.Page = lastPage;
            result = await _repo.ListCustomersAsync(input, ct);
        }

        result.Filter = input;
        return result;
    }

    public async Task<OutputCustomerCreate> CreateCustomerAsync(InputCustomerCreate input, CancellationToken ct = default)
    {
        NormalizeCreate(input);
        var result = await _repo.CreateCustomerAsync(input, ct);
        if (!result.Ok)
            throw Failure(result.StatusCode, result.ResponseMsg, "Unable to create the customer.");
        return result;
    }

    public async Task<OutputCustomerUpdate> UpdateCustomerAsync(InputCustomerUpdate input, CancellationToken ct = default)
    {
        input.FullName = (input.FullName ?? string.Empty).Trim();
        RequireName(input.FullName);
        input.CustomerCode = Clean(input.CustomerCode);
        input.Email = Clean(input.Email);
        input.Phone = Clean(input.Phone);

        var result = await _repo.UpdateCustomerAsync(input, ct);
        if (!result.Ok)
            throw Failure(result.StatusCode, result.ResponseMsg, "Unable to update the customer.");
        return result;
    }

    public async Task<OutputCustomerSetActive> SetCustomerActiveAsync(InputCustomerSetActive input, CancellationToken ct = default)
    {
        var result = await _repo.SetCustomerActiveAsync(input, ct);
        if (!result.Ok)
            throw Failure(result.StatusCode, result.ResponseMsg, "Unable to change the customer status.");
        return result;
    }

    public async Task<IReadOnlyList<OutputCustomerSearchItem>> SearchCustomersAsync(
        InputCustomerSearch input, CancellationToken ct = default)
    {
        input.Term = Clean(input.Term);
        if (input.Term is { Length: > 100 }) input.Term = input.Term[..100];
        input.PhoneDigits = DigitsOnly(input.Term);
        if (input.Take <= 0) input.Take = 10;
        if (input.Take > 20) input.Take = 20;

        if (string.IsNullOrEmpty(input.Term))
            return Array.Empty<OutputCustomerSearchItem>();

        return await _repo.SearchCustomersAsync(input, ct);
    }

    public async Task<OutputCustomerCreate> QuickCreateCustomerAsync(InputCustomerQuickCreate input, CancellationToken ct = default)
    {
        var create = new InputCustomerCreate
        {
            FullName = input.FullName,
            Phone = input.Phone,
            Email = input.Email,
            IsActive = true
        };
        NormalizeCreate(create);
        var result = await _repo.CreateCustomerAsync(create, ct);
        if (!result.Ok)
            throw Failure(result.StatusCode, result.ResponseMsg, "Unable to create the customer.");
        return result;
    }

    public Task<OutputCustomerSelect?> FindByPhoneAsync(string phone, CancellationToken ct = default)
        => _repo.FindByPhoneAsync(phone, ct);

    private static void NormalizeCreate(InputCustomerCreate input)
    {
        input.FullName = (input.FullName ?? string.Empty).Trim();
        RequireName(input.FullName);
        input.CustomerCode = Clean(input.CustomerCode);
        input.Email = Clean(input.Email);
        input.Phone = Clean(input.Phone);
    }

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string DigitsOnly(string? value) =>
        string.IsNullOrEmpty(value) ? string.Empty : new string(value.Where(char.IsDigit).ToArray());

    private static void RequireName(string name)
    {
        if (name.Length >= 2) return;
        const string message = "The Full Name must be between 2 and 300 characters.";
        throw new ValidationException(message, message, new Dictionary<string, string[]> { ["FullName"] = new[] { message } });
    }

    private static AppException Failure(int statusCode, string? message, string fallback)
    {
        var text = string.IsNullOrWhiteSpace(message) ? fallback : message;
        var developer = $"Customer repository returned {statusCode}: {text}";
        return statusCode switch
        {
            404 => new ResourceNotFoundException(text, developer),
            409 => new ConflictException(text, developer),
            422 => new ValidationException(text, developer),
            _ => new UpdateFailedException(text, developer)
        };
    }
}
