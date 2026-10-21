using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface IAuditRepository
{
    Task<OutputAuditLogList> ListLogsAsync(InputAuditLogList input, CancellationToken ct = default);
    Task<OutputAuditLogItem?> GetLogAsync(InputAuditLogSelect input, CancellationToken ct = default);
    Task WriteLogAsync(InputAuditLogCreate input, CancellationToken ct = default);
}
