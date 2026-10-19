using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartCartPro.Entities;

/// <summary>Customer review for a product (post-purchase). Maps to <c>product_reviews</c>.</summary>
[Table("product_reviews")]
public class ProductReview
{
    [Key]
    [Column("id_productreview")]
    public int IdProductReview { get; set; }

    [Column("fk_product")]
    public int FkProduct { get; set; }

    [Column("fk_customer")]
    public int FkCustomer { get; set; }

    [Column("fk_store")]
    public int? FkStore { get; set; }

    /// <summary>Reference to the online order that qualifies the customer to write this review.</summary>
    [Column("fk_onlineorder")]
    public int? FkOnlineOrder { get; set; }

    /// <summary>
    /// Reference to the specific order item being reviewed (patch column added in V007).
    /// Uniqueness constraint (fk_customer, fk_onlineorderitem) enforces one review per order item.
    /// </summary>
    [Column("fk_onlineorderitem")]
    public int? FkOnlineOrderItem { get; set; }

    /// <summary>1–5 star rating (CHECK in DB).</summary>
    [Column("rating")]
    public int Rating { get; set; }

    [Column("title")]
    public string? Title { get; set; }

    [Column("reviewtext")]
    public string? ReviewText { get; set; }

    /// <summary>Admin approval / visible flag. Hidden reviews have <c>isapproved = false</c>.</summary>
    [Column("isapproved")]
    public bool IsApproved { get; set; }

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

    [ForeignKey(nameof(FkProduct))]
    public Product? Product { get; set; }

    [ForeignKey(nameof(FkCustomer))]
    public Customer? Customer { get; set; }
}
