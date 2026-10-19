using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class AuditService : IAuditService
{
    private readonly IAuditRepository _repo;

    public AuditService(IAuditRepository repo) => _repo = repo;

    public Task<OutputAuditLogList> ListLogsAsync(InputAuditLogList input, CancellationToken ct = default) =>
        _repo.ListLogsAsync(input, ct);

    public Task WriteLogAsync(InputAuditLogCreate input, CancellationToken ct = default) =>
        _repo.WriteLogAsync(input, ct);
}
