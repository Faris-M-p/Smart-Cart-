using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>Maps to <c>user_invitations</c>. Company Admin invitation/account-setup flow.</summary>
[Table("user_invitations")]
public class UserInvitation
{
    [Key]
    [Column("id_invitation")]
    public int IdInvitation { get; set; }

    [Column("fk_user")]
    public int? FkUser { get; set; }

    [Required]
    [Column("invited_email")]
    public string InvitedEmail { get; set; } = string.Empty;

    [Required]
    [Column("invited_fullname")]
    public string InvitedFullName { get; set; } = string.Empty;

    [Required]
    [Column("invitation_token")]
    public string InvitationToken { get; set; } = string.Empty;

    [Column("fk_role")]
    public int FkRole { get; set; }

    [Column("fk_store")]
    public int FkStore { get; set; }

    /// <summary>pending | accepted | expired | cancelled</summary>
    [Required]
    [Column("status")]
    public string Status { get; set; } = "pending";

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; }

    [Column("accepted_at")]
    public DateTime? AcceptedAt { get; set; }

    /// <summary>pending | sent | failed</summary>
    [Required]
    [Column("delivery_status")]
    public string DeliveryStatus { get; set; } = "pending";

    [Column("delivery_error")]
    public string? DeliveryError { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(FkUser))]
    public User? User { get; set; }

    [ForeignKey(nameof(FkRole))]
    public Role? Role { get; set; }

    [ForeignKey(nameof(FkStore))]
    public Store? Store { get; set; }
}
