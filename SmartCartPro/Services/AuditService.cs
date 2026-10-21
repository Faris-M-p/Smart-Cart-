using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class AuditService : IAuditService
{
    private static readonly int[] PageSizes = { 10, 25, 50, 100 };

    private readonly IAuditRepository _repo;

    public AuditService(IAuditRepository repo) => _repo = repo;

    public async Task<OutputAuditLogList> ListLogsAsync(InputAuditLogList input, CancellationToken ct = default)
    {
        input.EntityName = Clean(input.EntityName);
        if (input.EntityName is { Length: > 100 }) input.EntityName = input.EntityName[..100];
        input.Action = Clean(input.Action);
        if (input.Action is { Length: > 80 }) input.Action = input.Action[..80];

        input.Page = Math.Max(1, input.Page);
        if (Array.IndexOf(PageSizes, input.PageSize) < 0) input.PageSize = 25;

        var result = await _repo.ListLogsAsync(input, ct);

        var lastPage = Math.Max(1, (int)Math.Ceiling(result.TotalCount / (double)input.PageSize));
        if (input.Page > lastPage)
        {
            input.Page = lastPage;
            result = await _repo.ListLogsAsync(input, ct);
        }

        result.Filter = input;
        return result;
    }

    public Task<OutputAuditLogItem?> GetLogAsync(InputAuditLogSelect input, CancellationToken ct = default) =>
        _repo.GetLogAsync(input, ct);

    public Task WriteLogAsync(InputAuditLogCreate input, CancellationToken ct = default) =>
        _repo.WriteLogAsync(input, ct);

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
