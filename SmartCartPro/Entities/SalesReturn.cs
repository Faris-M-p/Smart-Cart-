using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SmartCartPro.Entities;

/// <summary>Sales return header. Maps to <c>sales_returns</c>.</summary>
[Table("sales_returns")]
public class SalesReturn
{
    [Key]
    [Column("id_salesreturn")]
    public int IdSalesReturn { get; set; }

    [Column("fk_store")]
    public int FkStore { get; set; }

    [Column("fk_sale")]
    public int FkSale { get; set; }

    [Required]
    [Column("returnnumber")]
    public string ReturnNumber { get; set; } = string.Empty;

    [Column("returndate")]
    public DateTime ReturnDate { get; set; }

    [Precision(12, 2)]
    [Column("totalamount", TypeName = "numeric(12,2)")]
    public decimal TotalAmount { get; set; }

    [Column("reason")]
    public string? Reason { get; set; }

    [Column("status")]
    public string Status { get; set; } = "completed";

    [Column("createdby")]
    public int? CreatedBy { get; set; }

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

    [ForeignKey(nameof(FkStore))]
    public Store? Store { get; set; }

    [ForeignKey(nameof(FkSale))]
    public Sale? Sale { get; set; }

    public ICollection<SalesReturnDetail> Details { get; set; } = new List<SalesReturnDetail>();
}
