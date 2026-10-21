using System.ComponentModel.DataAnnotations;
using SmartCartPro.CustomModelValidation;

namespace SmartCartPro.Models;

// ═══════════════════════════════════════════════════════════════════
// PRODUCT REVIEW
// Rules enforced by service:
//   • Customer must have purchased the product (via online_orders + online_order_items)
//   • One review per order item (fk_onlineorderitem uniqueness)
//   • Edit / delete only within 7 days of order purchase date
//   • Admin can hide (set isapproved = false)
// ═══════════════════════════════════════════════════════════════════

#region Review Create

public sealed class InProductReviewCreateView
{
    [Display(Name = "Product")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Product.")]
    public int FkProduct { get; set; }

    [Display(Name = "Customer")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Customer.")]
    public int FkCustomer { get; set; }

    [Display(Name = "Store")]
    [GreaterThanOrEqualToZero(ErrorMessage = "Something went wrong/Please select a valid Store.")]
    public int? FkStore { get; set; }

    /// <summary>Must reference a qualifying order item owned by the customer.</summary>
    [Display(Name = "Order Item")]
    [Required]
    [GreaterThanZero(ErrorMessage = "Something went wrong/Please select a valid Order Item.")]
    public int FkOnlineOrderItem { get; set; }

    [Display(Name = "Rating")]
    [Required]
    [Range(1, 5, ErrorMessage = "The Rating must be between 1 and 5.")]
    public int Rating { get; set; }

    [Display(Name = "Title")]
    [StringLengthValidation(0, 200, ErrorMessage = "The Title cannot exceed 200 characters.")]
    [NoScriptTags(ErrorMessage = "The Title cannot contain script tags.")]
    public string? Title { get; set; }

    [Display(Name = "Review")]
    [StringLengthValidation(0, 2000, ErrorMessage = "The Review cannot exceed 2000 characters.")]
    [NoScriptTags(ErrorMessage = "The Review cannot contain script tags.")]
    public string? ReviewText { get; set; }
}

public sealed class InputProductReviewCreate
{
    public int FkProduct { get; set; }
    public int FkCustomer { get; set; }
    public int? FkStore { get; set; }
    public int FkOnlineOrderItem { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? ReviewText { get; set; }
}

public sealed class OutputProductReviewCreate
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public int IdProductReview { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Review Select / List (Admin)

public sealed class InputProductReviewList
{
    public int? FkProduct { get; set; }
    public int? FkStore { get; set; }
    public bool? IsApproved { get; set; }
    /// <summary>pending | approved | empty = all. Prepared by the service from the filter form.</summary>
    public string? Status { get; set; }
    public int? Rating { get; set; }
    public string? Search { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public bool IncludeCancelled { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string SortBy { get; set; } = "createdat";
    public bool SortDesc { get; set; } = true;
}

public sealed class OutputProductReviewListItem
{
    public int IdProductReview { get; set; }
    public int FkProduct { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int FkCustomer { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? ReviewText { get; set; }
    public bool IsApproved { get; set; }
    public bool Cancelled { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class OutputProductReviewList
{
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<OutputProductReviewListItem> Items { get; set; } = new();
    public InputProductReviewList Filter { get; set; } = new();
}

#endregion

#region Review Hide (Admin)

public sealed class InputProductReviewHide
{
    public int IdProductReview { get; set; }
    /// <summary>True = visible/approved; false = hidden.</summary>
    public bool IsApproved { get; set; }
    public int UpdatedBy { get; set; }
}

public sealed class OutputProductReviewHide
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion

#region Review Delete (Customer — service only, no MVC view yet)

public sealed class InputProductReviewDelete
{
    public int IdProductReview { get; set; }
    public int RequestingCustomerId { get; set; }
}

public sealed class OutputProductReviewDelete
{
    public int ResponseCode { get; set; }
    public string ResponseMsg { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public bool Ok => ResponseCode == 0;
}

#endregion
