using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCartPro.Entities;

/// <summary>POS sale line item. Maps to <c>sale_details</c>.</summary>
[Table("sale_details")]
public class SaleDetail
{
    [Key]
    [Column("id_saledetail")]
    public int IdSaleDetail { get; set; }

    [Column("fk_sale")]
    public int FkSale { get; set; }

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
    [Column("taxamount", TypeName = "numeric(12,2)")]
    public decimal TaxAmount { get; set; }

    [Precision(12, 2)]
    [Column("discountamount", TypeName = "numeric(12,2)")]
    public decimal DiscountAmount { get; set; }

    [Precision(12, 2)]
    [Column("linetotal", TypeName = "numeric(12,2)")]
    public decimal LineTotal { get; set; }

    [Column("createdat")]
    public DateTime CreatedAt { get; set; }

    [Column("updatedat")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey(nameof(FkSale))]
    public Sale? Sale { get; set; }

    [ForeignKey(nameof(FkProduct))]
    public Product? Product { get; set; }
}
