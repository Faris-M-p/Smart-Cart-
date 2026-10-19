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
