namespace SmartCartPro.Entities;

/// <summary>Maps to <c>user_roles</c>.</summary>
public class UserRole
{
    public int IdUserRole { get; set; }
    public int FkUser { get; set; }
    public int FkRole { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public User? User { get; set; }
    public Role? Role { get; set; }
}
