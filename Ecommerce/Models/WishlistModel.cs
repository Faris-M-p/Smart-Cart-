namespace Ecommerce.Models
{
    public class WishlistModel
    {
        public class WishlistToggleInput
        {
            public int ProductId { get; set; }
        }

        public class WishlistRemoveInput
        {
            public int WishlistItemId { get; set; }
        }

        public class WishlistLine
        {
            public int WishlistItemId { get; set; }
            public int ProductId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Slug { get; set; } = string.Empty;
            public string CategoryName { get; set; } = string.Empty;
            public string BrandName { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public decimal MRP { get; set; }
            public bool InStock { get; set; }
            public string ImageUrl { get; set; } = string.Empty;
        }

        public class WishlistStatus
        {
            public bool InWishlist { get; set; }
        }
    }
}
