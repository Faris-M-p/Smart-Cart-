namespace Ecommerce.Models
{
    public class CartModel
    {
        public class CartItemInput
        {
            public int ProductVariantId { get; set; }
            public int Quantity { get; set; } = 1;
        }

        public class CartItemUpdateInput
        {
            public int CartItemId { get; set; }
            public int Quantity { get; set; }
        }

        public class CartItemRemoveInput
        {
            public int CartItemId { get; set; }
        }

        public class CartLine
        {
            public int CartItemId { get; set; }
            public int ProductId { get; set; }
            public int ProductVariantId { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Slug { get; set; } = string.Empty;
            public string Label { get; set; } = string.Empty;
            public string SKU { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public decimal MRP { get; set; }
            public int Quantity { get; set; }
            public decimal LineTotal { get; set; }
            public int StockQuantity { get; set; }
            public bool InStock { get; set; }
            public string ImageUrl { get; set; } = string.Empty;
        }

        public class CartSummary
        {
            public int TotalQuantity { get; set; }
            public decimal Subtotal { get; set; }
            public int ItemCount { get; set; }
        }

        public class CartPage
        {
            public List<CartLine> Items { get; set; } = new();
            public CartSummary Summary { get; set; } = new();
        }

        public class BagCounts
        {
            public int CartCount { get; set; }
            public int WishlistCount { get; set; }
        }
    }
}
