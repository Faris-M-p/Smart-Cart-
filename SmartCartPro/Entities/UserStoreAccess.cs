using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>Maps to <c>user_store_access</c>. Uses <c>fk_store</c>.</summary>
[Table("user_store_access")]
public class UserStoreAccess
{
    [Key]
    [Column("id_userstoreaccess")]
    public int IdUserStoreAccess { get; set; }

    [Column("fk_user")]
    public int FkUser { get; set; }

    [Column("fk_store")]
    public int FkStore { get; set; }

    [Column("isdefault")]
    public bool IsDefault { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime? UpdatedAt { get; set; }

    [Column("cancelled")]
    public bool Cancelled { get; set; }

    [Column("cancelledon")]
    public DateTime? CancelledOn { get; set; }

    [Column("cancelledreason")]
    public string? CancelledReason { get; set; }

    [ForeignKey(nameof(FkUser))]
    public User? User { get; set; }

    [ForeignKey(nameof(FkStore))]
    public Store? Store { get; set; }
}

