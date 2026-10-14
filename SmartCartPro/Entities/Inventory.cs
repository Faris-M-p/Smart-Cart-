namespace SmartCartPro.Entities;

/// <summary>
/// Store-scoped stock balance. SQL column <c>fk_store</c> maps to <see cref="FkStore"/> (never StoreId in SQL).
/// </summary>
public class Inventory
{
    public int IdInventory { get; set; }

    /// <summary>SQL column <c>fk_store</c>.</summary>
    public int FkStore { get; set; }

    /// <summary>SQL column <c>fk_product</c>.</summary>
    public int FkProduct { get; set; }

    /// <summary>SQL column <c>fk_productvariant</c>.</summary>
    public int? FkProductVariant { get; set; }

    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal ReorderLevel { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Store? Store { get; set; }
    public Product? Product { get; set; }
}
