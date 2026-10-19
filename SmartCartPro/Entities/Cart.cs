using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>Storefront cart header. Maps to <c>carts</c>.</summary>
[Table("carts")]
public class Cart
{
    [Key]
    [Column("id_cart")]
    public int IdCart { get; set; }

    [Column("fk_store")]
    public int FkStore { get; set; }

    /// <summary>Null for guest carts (identified by <see cref="SessionKey"/> instead).</summary>
    [Column("fk_customer")]
    public int? FkCustomer { get; set; }

    [Column("sessionkey")]
    public string? SessionKey { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(FkStore))]
    public Store? Store { get; set; }

    [ForeignKey(nameof(FkCustomer))]
    public Customer? Customer { get; set; }

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
