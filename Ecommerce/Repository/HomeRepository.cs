using Ecommerce.Interface;
using Ecommerce.Models;

namespace Ecommerce.Repository
{
    public class HomeRepository : HomeInterface
    {
        private readonly IDataAccessDapper _dapper;

        public HomeRepository(IDataAccessDapper dapper)
        {
            _dapper = dapper;
        }

        public async Task<HomeViewModel> GetHomePageDataAsync()
        {
            var result = new HomeViewModel();

            try
            {
                // 1. Banners
                const string sqlBanners = @"
                    SELECT 
                        b.id_banner AS BannerId,
                        COALESCE(b.title, '') AS Title,
                        '' AS Subtitle,
                        COALESCE(b.imageurl, '') AS ImageUrl,
                        '' AS TargetUrl
                    FROM homepage_banners b
                    WHERE b.isactive = true AND b.cancelled = false
                    ORDER BY b.displayorder, b.id_banner;";

                var banners = await _dapper.GetListByQuery<HomeBannerDto>(sqlBanners);

                if (banners.Count > 0)
                {
                    const string sqlBannerCats = @"
                        SELECT 
                            fk_banner AS BannerId,
                            fk_category AS CategoryId
                        FROM homepage_banner_categories;";

                    var bannerCatsRaw = await _dapper.GetListByQuery<BannerCatLink>(sqlBannerCats);
                    var bannerCatMap = bannerCatsRaw
                        .GroupBy(x => x.BannerId)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.CategoryId).ToList());

                    foreach (var b in banners)
                    {
                        if (bannerCatMap.TryGetValue(b.BannerId, out var catIds))
                        {
                            b.CategoryIds = catIds;
                        }

                        if (string.IsNullOrWhiteSpace(b.TargetUrl) && b.CategoryIds.Count > 0)
                        {
                            b.TargetUrl = $"/Shop?categoryIds={string.Join(",", b.CategoryIds)}";
                        }
                        else if (string.IsNullOrWhiteSpace(b.TargetUrl))
                        {
                            b.TargetUrl = "/Shop";
                        }
                    }

                    result.Banners = banners;
                }

                // 2. Featured Categories
                const string sqlCategories = @"
                    SELECT 
                        c.id_category AS CategoryId,
                        COALESCE(c.name, '') AS CategoryName,
                        COALESCE(c.imageurl, '') AS ImageUrl
                    FROM homepage_categories hc
                    JOIN category c ON hc.fk_category = c.id_category
                    WHERE hc.isactive = true AND hc.cancelled = false AND c.isactive = true AND c.cancelled = false
                    ORDER BY hc.displayorder, hc.id_homepagecategory;";

                result.FeaturedCategories = await _dapper.GetListByQuery<HomeCategoryDto>(sqlCategories);

                // 3. Featured Products
                const string sqlProducts = @"
                    SELECT 
                        p.id_product AS ProductId,
                        COALESCE(p.slug, '') AS Slug,
                        COALESCE(p.name, '') AS ProductName,
                        COALESCE(c.name, '') AS CategoryName,
                        COALESCE((
                            SELECT mediaurl FROM productmedia 
                            WHERE fk_product = p.id_product AND mediatype = 'Image' AND mediaurl IS NOT NULL AND mediaurl <> ''
                            ORDER BY isprimary DESC, displayorder ASC LIMIT 1
                        ), '') AS PrimaryImage,
                        COALESCE((
                            SELECT MIN(pv.sellingprice) FROM productvariants pv 
                            WHERE pv.fk_product = p.id_product AND pv.isactive = true AND pv.cancelled = false
                        ), 0) AS Price
                    FROM homepage_products hp
                    JOIN products p ON hp.fk_product = p.id_product
                    JOIN subcategory sc ON p.fk_subcategory = sc.id_subcategory
                    JOIN category c ON sc.fk_category = c.id_category
                    WHERE hp.isactive = true AND hp.cancelled = false AND p.isactive = true AND p.cancelled = false
                    ORDER BY hp.displayorder, hp.id_homepageproduct;";

                result.FeaturedProducts = await _dapper.GetListByQuery<HomeProductDto>(sqlProducts);
            }
            catch (Exception ex)
            {
                // Log or catch exception gracefully
                Console.WriteLine($"Error in GetHomePageDataAsync: {ex.Message}");
            }

            return result;
        }

        private class BannerCatLink
        {
            public int BannerId { get; set; }
            public int CategoryId { get; set; }
        }
    }
}
