namespace Ecommerce.Models
{
    public class ProductModel
    {
        public class InputProduct
        {
            public int PageIndex { get; set; }
            public int PageSize { get; set; }
            public string SearchName { get; set; } = "";
            public string SortColumn { get; set; } = "ProductId";
            public string SortMode { get; set; } = "ASC";
            public string CategoryIds { get; set; } = "";
            public string SubCategoryIds { get; set; } = "";
            public string BrandIds { get; set; } = "";
            public string Ratings { get; set; } = "";
            public string Gender { get; set; } = "";
            public decimal? PriceFrom { get; set; }
            public decimal? PriceTo { get; set; }
            public string Status { get; set; } = "";
        }
        public class Product
        {
            public int ProductId { get; set; }
            public string Name { get; set; }
            public int CategoryId { get; set; }
            public int SubCategoryId { get; set; }
            public int BrandId { get; set; }
            public int Rating { get; set; }
            public string Gender { get; set; }
            public decimal Price { get; set; }
            public decimal MRP { get; set; }
        }


    }
}
