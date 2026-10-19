using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>A single product in a customer wishlist. Maps to <c>wishlist_items</c>.</summary>
[Table("wishlist_items")]
public class WishlistItem
{
    [Key]
    [Column("id_wishlistitem")]
    public int IdWishlistItem { get; set; }

    [Column("fk_wishlist")]
    public int FkWishlist { get; set; }

    [Column("fk_store")]
    public int FkStore { get; set; }

    [Column("fk_product")]
    public int FkProduct { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(FkWishlist))]
    public Wishlist? Wishlist { get; set; }

    [ForeignKey(nameof(FkProduct))]
    public Product? Product { get; set; }
}
