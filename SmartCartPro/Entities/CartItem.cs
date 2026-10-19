using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCartPro.Entities;

/// <summary>Line in a storefront cart. Maps to <c>cart_items</c>.</summary>
[Table("cart_items")]
public class CartItem
{
    [Key]
    [Column("id_cartitem")]
    public int IdCartItem { get; set; }

    [Column("fk_cart")]
    public int FkCart { get; set; }

    [Column("fk_store")]
    public int FkStore { get; set; }

    [Column("fk_product")]
    public int FkProduct { get; set; }

    [Column("fk_productvariant")]
    public int? FkProductVariant { get; set; }

    [Precision(14, 3)]
    [Column("quantity", TypeName = "numeric(14,3)")]
    public decimal Quantity { get; set; } = 1;

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(FkCart))]
    public Cart? Cart { get; set; }

    [ForeignKey(nameof(FkProduct))]
    public Product? Product { get; set; }
}
