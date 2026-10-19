using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>Customer wishlist header (one per customer per store). Maps to <c>wishlists</c>.</summary>
[Table("wishlists")]
public class Wishlist
{
    [Key]
    [Column("id_wishlist")]
    public int IdWishlist { get; set; }

    [Column("fk_store")]
    public int FkStore { get; set; }

    [Column("fk_customer")]
    public int FkCustomer { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(FkStore))]
    public Store? Store { get; set; }

    [ForeignKey(nameof(FkCustomer))]
    public Customer? Customer { get; set; }

    public ICollection<WishlistItem> Items { get; set; } = new List<WishlistItem>();
}
