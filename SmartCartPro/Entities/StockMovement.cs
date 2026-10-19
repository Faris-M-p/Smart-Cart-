using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCartPro.Entities;

/// <summary>
/// Audit trail for every stock in/out event. Maps to <c>stock_movements</c>.
/// Only ever written via <see cref="SmartCartPro.Repositories.Interfaces.IStockRepository.ApplyMovementAsync"/>.
/// </summary>
[Table("stock_movements")]
public class StockMovement
{
    [Key]
    [Column("id_stockmovement")]
    public int IdStockMovement { get; set; }

    [Column("fk_store")]
    public int FkStore { get; set; }

    [Column("fk_product")]
    public int FkProduct { get; set; }

    [Column("fk_productvariant")]
    public int? FkProductVariant { get; set; }

    /// <summary>
    /// Must match DB CHECK: purchase|sale|adjustment|transfer_in|transfer_out|return_in|return_out|opening
    /// </summary>
    [Required]
    [Column("movementtype")]
    public string MovementType { get; set; } = string.Empty;

    [Precision(14, 3)]
    [Column("quantity", TypeName = "numeric(14,3)")]
    public decimal Quantity { get; set; }

    [Column("referencetype")]
    public string? ReferenceType { get; set; }

    [Column("referenceid")]
    public int? ReferenceId { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("createdby")]
    public int? CreatedBy { get; set; }

    [ForeignKey(nameof(FkStore))]
    public Store? Store { get; set; }

    [ForeignKey(nameof(FkProduct))]
    public Product? Product { get; set; }
}
