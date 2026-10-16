namespace SmartCartPro.Entities;

/// <summary>Maps to <c>user_store_access</c>. Uses <c>fk_store</c>.</summary>
public class UserStoreAccess
{
    public int IdUserStoreAccess { get; set; }
    public int FkUser { get; set; }
    public int FkStore { get; set; }
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool Cancelled { get; set; }
    public DateTime? CancelledOn { get; set; }
    public string? CancelledReason { get; set; }

    public User? User { get; set; }
    public Store? Store { get; set; }
}
