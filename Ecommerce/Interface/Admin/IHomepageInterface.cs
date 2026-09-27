using static Ecommerce.Models.Admin.HomepageModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Interface.Admin
{
    public interface IHomepageInterface
    {
        // Banners
        Task<List<BannerDto>> GetBannersAsync();
        Task<BannerDto?> GetBannerByIdAsync(int id);
        Task<CommonResponse> CreateBannerAsync(BannerSaveInput input);
        Task<CommonResponse> UpdateBannerAsync(BannerSaveInput input);
        Task SetBannerImageUrlAsync(int bannerId, string? imageUrl);
        Task<CommonResponse> DeleteBannerAsync(int id);

        // Featured Categories
        Task<List<FeaturedCategoryDto>> GetFeaturedCategoriesAsync();
        Task<CommonResponse> SaveFeaturedCategoryAsync(FeaturedCategorySaveInputVIEW input);
        Task<CommonResponse> ToggleFeaturedCategoryStatusAsync(int id);
        Task<CommonResponse> UpdateFeaturedCategoryOrderAsync(int id, int order);
        Task<CommonResponse> DeleteFeaturedCategoryAsync(int id);

        // Featured Products
        Task<List<FeaturedProductDto>> GetFeaturedProductsAsync();
        Task<CommonResponse> SaveFeaturedProductAsync(FeaturedProductSaveInputVIEW input);
        Task<CommonResponse> ToggleFeaturedProductStatusAsync(int id);
        Task<CommonResponse> UpdateFeaturedProductOrderAsync(int id, int order);
        Task<CommonResponse> DeleteFeaturedProductAsync(int id);

        // Lookups
        Task<List<CategoryOption>> GetAvailableCategoryOptionsAsync();
        Task<List<ProductOption>> GetAvailableProductOptionsAsync();
    }
}
