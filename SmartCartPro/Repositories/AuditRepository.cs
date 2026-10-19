using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

/// <summary>
/// Audit log repository — read-only list + write. Entities stay internal.
/// </summary>
public class AuditRepository : IAuditRepository
{
    private readonly SmartCartDbContext _db;

    public AuditRepository(SmartCartDbContext db) => _db = db;

    public async Task<OutputAuditLogList> ListLogsAsync(InputAuditLogList input, CancellationToken ct = default)
    {
        var q = _db.AuditLogs
            .Include(a => a.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(input.EntityName))
            q = q.Where(a => a.EntityName == input.EntityName);

        if (input.EntityId.HasValue)
            q = q.Where(a => a.EntityId == input.EntityId.Value);

        if (input.FkUser.HasValue)
            q = q.Where(a => a.FkUser == input.FkUser.Value);

        if (input.FkStore.HasValue)
            q = q.Where(a => a.FkStore == input.FkStore.Value);

        if (!string.IsNullOrWhiteSpace(input.Action))
            q = q.Where(a => a.Action == input.Action);

        if (input.From.HasValue)
            q = q.Where(a => a.CreatedAt >= input.From.Value);

        if (input.To.HasValue)
            q = q.Where(a => a.CreatedAt <= input.To.Value);

        var total = await q.CountAsync(ct);

        var rows = await q
            .OrderByDescending(a => a.CreatedAt)
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .Select(a => new OutputAuditLogItem
            {
                IdAuditLog  = a.IdAuditLog,
                FkStore     = a.FkStore,
                FkUser      = a.FkUser,
                UserName    = a.User != null ? a.User.UserName : null,
                EntityName  = a.EntityName,
                EntityId    = a.EntityId,
                Action      = a.Action,
                Details     = a.Details,
                CreatedAt   = a.CreatedAt
            })
            .ToListAsync(ct);

        return new OutputAuditLogList
        {
            TotalCount = total,
            Page       = input.Page,
            PageSize   = input.PageSize,
            Items      = rows
        };
    }

    public async Task WriteLogAsync(InputAuditLogCreate input, CancellationToken ct = default)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            FkUser     = input.ActorUserId > 0 ? input.ActorUserId : null,
            EntityName = input.EntityName,
            EntityId   = input.EntityId,
            Action     = input.Action,
            Details    = input.Details,
            CreatedAt  = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
    }
}
