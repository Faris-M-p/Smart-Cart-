namespace Ecommerce.Models
{
    public class HomeViewModel
    {
        public List<HomeBannerDto> Banners { get; set; } = new();
        public List<HomeCategoryDto> FeaturedCategories { get; set; } = new();
        public List<HomeProductDto> FeaturedProducts { get; set; } = new();
    }

    public class HomeBannerDto
    {
        public int BannerId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string TargetUrl { get; set; } = string.Empty;
        public List<int> CategoryIds { get; set; } = new();
    }

    public class HomeCategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class HomeProductDto
    {
        public int ProductId { get; set; }
        public string Slug { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string PrimaryImage { get; set; } = string.Empty;
        public string SecondaryImage { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
