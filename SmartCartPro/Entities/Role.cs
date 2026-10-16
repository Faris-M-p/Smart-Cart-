namespace SmartCartPro.Entities;

/// <summary>Maps to <c>roles</c>.</summary>
public class Role
{
    public int IdRole { get; set; }
    public string RoleCode { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool Cancelled { get; set; }
    public DateTime? CancelledOn { get; set; }
    public string? CancelledReason { get; set; }
}
