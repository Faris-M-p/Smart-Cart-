using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCartPro.Entities;

/// <summary>Sales return line item. Maps to <c>sales_return_details</c>.</summary>
[Table("sales_return_details")]
public class SalesReturnDetail
{
    [Key]
    [Column("id_salesreturndetail")]
    public int IdSalesReturnDetail { get; set; }

    [Column("fk_salesreturn")]
    public int FkSalesReturn { get; set; }

    [Column("fk_store")]
    public int FkStore { get; set; }

    [Column("fk_product")]
    public int FkProduct { get; set; }

    [Column("fk_productvariant")]
    public int? FkProductVariant { get; set; }

    [Precision(14, 3)]
    [Column("quantity", TypeName = "numeric(14,3)")]
    public decimal Quantity { get; set; }

    [Precision(12, 2)]
    [Column("unitprice", TypeName = "numeric(12,2)")]
    public decimal UnitPrice { get; set; }

    [Precision(12, 2)]
    [Column("linetotal", TypeName = "numeric(12,2)")]
    public decimal LineTotal { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(FkSalesReturn))]
    public SalesReturn? SalesReturn { get; set; }

    [ForeignKey(nameof(FkProduct))]
    public Product? Product { get; set; }
}
