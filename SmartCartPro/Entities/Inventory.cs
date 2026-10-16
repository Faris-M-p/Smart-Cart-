using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCartPro.Entities;

/// <summary>
/// Store-scoped stock balance. SQL column <c>fk_store</c> maps to <see cref="FkStore"/> (never StoreId in SQL).
/// </summary>
[Table("inventory")]
public class Inventory
{
    [Key]
    [Column("id_inventory")]
    public int IdInventory { get; set; }

    /// <summary>SQL column <c>fk_store</c>.</summary>
    [Column("fk_store")]
    public int FkStore { get; set; }

    /// <summary>SQL column <c>fk_product</c>.</summary>
    [Column("fk_product")]
    public int FkProduct { get; set; }

    /// <summary>SQL column <c>fk_productvariant</c>.</summary>
    [Column("fk_productvariant")]
    public int? FkProductVariant { get; set; }

    [Precision(14, 3)]
    [Column("quantityonhand", TypeName = "numeric(14,3)")]
    public decimal QuantityOnHand { get; set; }

    [Precision(14, 3)]
    [Column("quantityreserved", TypeName = "numeric(14,3)")]
    public decimal QuantityReserved { get; set; }

    [Precision(14, 3)]
    [Column("reorderlevel", TypeName = "numeric(14,3)")]
    public decimal ReorderLevel { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(FkStore))]
    public Store? Store { get; set; }

    [ForeignKey(nameof(FkProduct))]
    public Product? Product { get; set; }
}

