using System.ComponentModel.DataAnnotations;
using Ecommerce.CustomModelValidation;

namespace Ecommerce.Models.Admin
{
    /// <summary>Admin moderation of customer ratings &amp; reviews (existing <c>ratings</c> table).</summary>
    public class AdminReviewModel
    {
        public class AdminReviewListInputVIEW
        {
            [Display(Name = "Search Text")]
            public string SearchText { get; set; } = string.Empty;

            /// <summary>0 = all ratings, otherwise 1..5.</summary>
            [Display(Name = "Rating")]
            [Range(0, 5, ErrorMessage = "{0} must be between {1} and {2}.")]
            public int Rating { get; set; }

            /// <summary>Active (default), Removed or All.</summary>
            [Display(Name = "Status")]
            [System.ComponentModel.DataAnnotations.MaxLength(20, ErrorMessage = "{0} cannot exceed 20 characters.")]
            public string Status { get; set; } = "Active";

            [Display(Name = "Page Index")]
            [GreaterThanZero]
            public int PageIndex { get; set; } = 1;

            [Display(Name = "Page Size")]
            [GreaterThanZero]
            [Range(1, 100, ErrorMessage = "{0} must be between 1 and 100.")]
            public int PageSize { get; set; } = 10;
        }

        public class AdminReviewRemoveInputVIEW
        {
            [Display(Name = "Review ID")]
            [Required(ErrorMessage = "{0} is required.")]
            [GreaterThanZero]
            public int ReviewId { get; set; }

            [Display(Name = "Reason")]
            [System.ComponentModel.DataAnnotations.MaxLength(255, ErrorMessage = "{0} cannot exceed 255 characters.")]
            public string Reason { get; set; } = string.Empty;
        }

        public class AdminReviewListInput
        {
            public string SearchText { get; set; } = string.Empty;
            public int Rating { get; set; }
            public string Status { get; set; } = "Active";
            public int PageIndex { get; set; } = 1;
            public int PageSize { get; set; } = 10;
        }

        public class AdminReviewListItem
        {
            public int ReviewId { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public string ProductSlug { get; set; } = string.Empty;
            public int CustomerId { get; set; }
            public string CustomerName { get; set; } = string.Empty;
            public string CustomerEmail { get; set; } = string.Empty;
            public int Rating { get; set; }
            public string Review { get; set; } = string.Empty;
            public DateTime? CreatedAt { get; set; }
            public bool IsVerifiedPurchase { get; set; }
            public int OrderId { get; set; }
            public string OrderNumber { get; set; } = string.Empty;
            public bool Removed { get; set; }
            public DateTime? RemovedOn { get; set; }
            public string RemovedReason { get; set; } = string.Empty;
        }
    }
}
