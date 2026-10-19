namespace SmartCartPro.Models;

#region Audit Create

public sealed class InputAuditLogCreate
{
    public int ActorUserId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
}

#endregion

#region Audit List (admin read)

public sealed class InputAuditLogList
{
    public string? EntityName { get; set; }
    public int? EntityId { get; set; }
    public int? FkUser { get; set; }
    public int? FkStore { get; set; }
    public string? Action { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed class OutputAuditLogItem
{
    public int IdAuditLog { get; set; }
    public int? FkStore { get; set; }
    public int? FkUser { get; set; }
    public string? UserName { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class OutputAuditLogList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputAuditLogItem> Items { get; set; } = new();
}

#endregion
