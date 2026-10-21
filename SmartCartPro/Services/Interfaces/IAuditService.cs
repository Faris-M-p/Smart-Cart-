using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface IAuditService
{
    Task<OutputAuditLogList> ListLogsAsync(InputAuditLogList input, CancellationToken ct = default);
    Task<OutputAuditLogItem?> GetLogAsync(InputAuditLogSelect input, CancellationToken ct = default);
    Task WriteLogAsync(InputAuditLogCreate input, CancellationToken ct = default);
}
