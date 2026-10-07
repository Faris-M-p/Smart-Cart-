using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Models
{
    /// <summary>
    /// Customer ratings &amp; reviews (existing <c>ratings</c> table), based on purchased ORDER ITEMS:
    /// a review belongs to Customer + Order + OrderItem + Product, one active review per order item,
    /// and it can only be added / edited / deleted during the 7-day window after delivery.
    /// The customer id is never part of any input model — it always comes from the signed-in session,
    /// and the order / order item / product ids are re-validated against each other in the database.
    /// </summary>
    public class ReviewModel
    {
        public const int MaxReviewLength = 1000;
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 50;

        public class ReviewSubmitInput
        {
            [Display(Name = "Order")]
            [Range(1, int.MaxValue, ErrorMessage = "Order is missing.")]
            public int OrderId { get; set; }

            [Display(Name = "Order item")]
            [Range(1, int.MaxValue, ErrorMessage = "Order item is missing.")]
            public int OrderItemId { get; set; }

            [Display(Name = "Product")]
            [Range(1, int.MaxValue, ErrorMessage = "Product is missing.")]
            public int ProductId { get; set; }

            [Display(Name = "Rating")]
            [Range(1, 5, ErrorMessage = "Please select a rating from 1 to 5 stars.")]
            public int Rating { get; set; }

            [Display(Name = "Review")]
            [MaxLength(MaxReviewLength, ErrorMessage = "{0} cannot exceed 1000 characters.")]
            public string? Review { get; set; }
        }

        public class ReviewUpdateInput
        {
            [Display(Name = "Review")]
            [Range(1, int.MaxValue, ErrorMessage = "Review is missing.")]
            public int ReviewId { get; set; }

            [Display(Name = "Rating")]
            [Range(1, 5, ErrorMessage = "Please select a rating from 1 to 5 stars.")]
            public int Rating { get; set; }

            [Display(Name = "Review")]
            [MaxLength(MaxReviewLength, ErrorMessage = "{0} cannot exceed 1000 characters.")]
            public string? Review { get; set; }
        }

        public class ReviewDeleteInput
        {
            [Display(Name = "Review")]
            [Range(1, int.MaxValue, ErrorMessage = "Review is missing.")]
            public int ReviewId { get; set; }
        }

        public class ReviewSummary
        {
            public int ProductId { get; set; }
            public decimal AverageRating { get; set; }
            public int TotalRatings { get; set; }
            public int TotalReviews { get; set; }
        }

        public class ReviewDistributionRow
        {
            public int Stars { get; set; }
            public int RatingCount { get; set; }
            public int Percentage { get; set; }
        }

        public class ReviewItem
        {
            public int ReviewId { get; set; }
            public string ReviewerName { get; set; } = string.Empty;
            public int Rating { get; set; }
            public string Review { get; set; } = string.Empty;
            public DateTime? CreatedAt { get; set; }
            /// <summary>True when the review is attached to a purchased order item.</summary>
            public bool IsVerifiedPurchase { get; set; }
            public bool IsMine { get; set; }
            /// <summary>Only set for the viewer's own review: the order it can be managed from.</summary>
            public int MyOrderId { get; set; }
        }

        public class ProductReviewsResponse
        {
            public ReviewSummary Summary { get; set; } = new();
            public List<ReviewDistributionRow> Distribution { get; set; } = new();
            public List<ReviewItem> Reviews { get; set; } = new();
            public int PageIndex { get; set; }
            public int PageSize { get; set; }
            public int TotalCount { get; set; }
            public int TotalPages { get; set; }
        }

        /// <summary>
        /// Review state of one order item, calculated entirely on the server.
        /// <c>ReviewStatus</c>: NotEligible | CanReview | Reviewed | Locked | Expired.
        /// </summary>
        public class OrderItemReviewStatus
        {
            public int OrderItemId { get; set; }
            public int OrderId { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public string VariantLabel { get; set; } = string.Empty;
            public int Quantity { get; set; }

            /// <summary>The order is delivered (and not cancelled), so the item can be reviewed.</summary>
            public bool IsEligible { get; set; }
            public DateTime? DeliveredOn { get; set; }
            public DateTime? ReviewExpiresOn { get; set; }
            public bool ReviewWindowExpired { get; set; }
            public int DaysLeft { get; set; }

            public int ReviewId { get; set; }
            public int Rating { get; set; }
            public string Review { get; set; } = string.Empty;
            public DateTime? ReviewedOn { get; set; }

            public bool CanAddReview { get; set; }
            public bool CanEditReview { get; set; }
            public bool CanDeleteReview { get; set; }
            public string ReviewStatus { get; set; } = "NotEligible";
        }
    }
}
