using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

/// <summary>
/// Audit log repository — read-only list + write. Entities stay internal.
/// Write API (WriteLogAsync / InputAuditLogCreate) is source-compatible with existing callers.
/// </summary>
public class AuditRepository : IAuditRepository
{
    private readonly SmartCartDbContext _db;

    public AuditRepository(SmartCartDbContext db) => _db = db;

    public async Task<OutputAuditLogList> ListLogsAsync(InputAuditLogList input, CancellationToken ct = default)
    {
        var q = ApplyCompanyScope(_db.AuditLogs.AsQueryable(), input.FkCompany);

        if (!string.IsNullOrWhiteSpace(input.EntityName))
            q = q.Where(a => a.EntityName.ToLower().Contains(input.EntityName.Trim().ToLower()));

        if (input.EntityId.HasValue)
            q = q.Where(a => a.EntityId == input.EntityId.Value);

        if (input.FkUser.HasValue)
            q = q.Where(a => a.FkUser == input.FkUser.Value);

        if (input.FkStore.HasValue)
            q = q.Where(a => a.FkStore == input.FkStore.Value);

        if (!string.IsNullOrWhiteSpace(input.Action))
            q = q.Where(a => a.Action.ToLower().Contains(input.Action.Trim().ToLower()));

        if (input.From.HasValue)
            q = q.Where(a => a.CreatedAt >= input.From.Value);

        if (input.To.HasValue)
        {
            var to = input.To.Value.TimeOfDay == TimeSpan.Zero
                ? input.To.Value.Date.AddDays(1)
                : input.To.Value;
            q = q.Where(a => a.CreatedAt < to);
        }

        var total = await q.CountAsync(ct);

        var rows = await q
            .OrderByDescending(a => a.CreatedAt)
            .Skip((input.Page - 1) * input.PageSize)
            .Take(input.PageSize)
            .Select(AuditProjection())
            .ToListAsync(ct);

        return new OutputAuditLogList
        {
            TotalCount = total,
            Page = input.Page,
            PageSize = input.PageSize,
            Items = rows
        };
    }

    public async Task<OutputAuditLogItem?> GetLogAsync(InputAuditLogSelect input, CancellationToken ct = default)
    {
        var q = ApplyCompanyScope(
            _db.AuditLogs.Where(a => a.IdAuditLog == input.IdAuditLog),
            input.FkCompany);

        return await q.Select(AuditProjection()).FirstOrDefaultAsync(ct);
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

    private IQueryable<AuditLog> ApplyCompanyScope(IQueryable<AuditLog> q, int? fkCompany)
    {
        if (fkCompany is not > 0) return q;

        var companyId = fkCompany.Value;
        return q.Where(a =>
            a.FkStore == null
            || _db.Stores.Any(s => s.IdStore == a.FkStore && s.FkCompany == companyId && !s.Cancelled));
    }

    private System.Linq.Expressions.Expression<Func<AuditLog, OutputAuditLogItem>> AuditProjection()
        => a => new OutputAuditLogItem
        {
            IdAuditLog = a.IdAuditLog,
            FkStore    = a.FkStore,
            StoreName  = a.FkStore == null
                ? null
                : _db.Stores.Where(s => s.IdStore == a.FkStore).Select(s => s.StoreName).FirstOrDefault(),
            FkUser     = a.FkUser,
            UserName   = a.User != null ? a.User.UserName : null,
            EntityName = a.EntityName,
            EntityId   = a.EntityId,
            Action     = a.Action,
            Details    = a.Details,
            CreatedAt  = a.CreatedAt
        };
}
