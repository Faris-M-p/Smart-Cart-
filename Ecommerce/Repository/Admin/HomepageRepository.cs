using Ecommerce.DataAccess;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static Ecommerce.Models.Admin.HomepageModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Repository.Admin
{
    public class HomepageRepository : IHomepageInterface
    {
        private readonly EcommerceDbContext _db;

        public HomepageRepository(EcommerceDbContext db)
        {
            _db = db;
        }

        // =========================================================================
        // Banners
        // =========================================================================
        public async Task<List<BannerDto>> GetBannersAsync()
        {
            var banners = await _db.HomepageBanners.AsNoTracking()
                .Where(b => !b.Cancelled)
                .OrderBy(b => b.DisplayOrder)
                .ThenByDescending(b => b.ID_Banner)
                .ToListAsync();

            if (banners.Count == 0)
            {
                return new List<BannerDto>();
            }

            var bannerIds = banners.Select(b => b.ID_Banner).ToList();
            var links = await (
                from hbc in _db.HomepageBannerCategories.AsNoTracking()
                join c in _db.Categories.AsNoTracking() on hbc.FK_Category equals c.ID_Category
                where bannerIds.Contains(hbc.FK_Banner) && !c.Cancelled
                select new { hbc.FK_Banner, c.ID_Category, c.Name }
            ).ToListAsync();

            var catGroup = links
                .GroupBy(x => x.FK_Banner)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        Ids = g.Select(x => x.ID_Category).ToList(),
                        Names = g.Select(x => x.Name).ToList()
                    });

            return banners.Select(b =>
            {
                catGroup.TryGetValue(b.ID_Banner, out var catInfo);
                return new BannerDto
                {
                    BannerId = b.ID_Banner,
                    Title = b.Title,
                    ImageUrl = b.ImageUrl,
                    IsActive = b.IsActive,
                    DisplayOrder = b.DisplayOrder,
                    CategoryIds = catInfo?.Ids ?? new List<int>(),
                    CategoryNames = catInfo?.Names ?? new List<string>(),
                    CreatedAt = b.CreatedAt
                };
            }).ToList();
        }

        public async Task<BannerDto?> GetBannerByIdAsync(int id)
        {
            if (id <= 0) return null;

            var banner = await _db.HomepageBanners.AsNoTracking()
                .FirstOrDefaultAsync(b => b.ID_Banner == id && !b.Cancelled);

            if (banner == null) return null;

            var categoryIds = await _db.HomepageBannerCategories.AsNoTracking()
                .Where(hbc => hbc.FK_Banner == id)
                .Select(hbc => hbc.FK_Category)
                .ToListAsync();

            var categoryNames = await _db.Categories.AsNoTracking()
                .Where(c => categoryIds.Contains(c.ID_Category) && !c.Cancelled)
                .Select(c => c.Name)
                .ToListAsync();

            return new BannerDto
            {
                BannerId = banner.ID_Banner,
                Title = banner.Title,
                ImageUrl = banner.ImageUrl,
                IsActive = banner.IsActive,
                DisplayOrder = banner.DisplayOrder,
                CategoryIds = categoryIds,
                CategoryNames = categoryNames,
                CreatedAt = banner.CreatedAt
            };
        }

        public async Task<CommonResponse> CreateBannerAsync(BannerSaveInput input)
        {
            if (input == null) return Fail("Invalid request.");

            var entity = new HomepageBannerEntity
            {
                Title = string.IsNullOrWhiteSpace(input.Title) ? null : input.Title.Trim(),
                ImageUrl = input.ImageUrl ?? string.Empty,
                IsActive = input.IsActive,
                DisplayOrder = input.DisplayOrder,
                CreatedAt = DateTime.UtcNow,
                Cancelled = false
            };

            _db.HomepageBanners.Add(entity);
            await _db.SaveChangesAsync();

            if (input.CategoryIds != null && input.CategoryIds.Count > 0)
            {
                var validCategoryIds = await _db.Categories.AsNoTracking()
                    .Where(c => input.CategoryIds.Contains(c.ID_Category) && !c.Cancelled)
                    .Select(c => c.ID_Category)
                    .ToListAsync();

                foreach (var catId in validCategoryIds.Distinct())
                {
                    _db.HomepageBannerCategories.Add(new HomepageBannerCategoryEntity
                    {
                        FK_Banner = entity.ID_Banner,
                        FK_Category = catId
                    });
                }
                await _db.SaveChangesAsync();
            }

            return Ok(entity.ID_Banner, "Banner created successfully.");
        }

        public async Task<CommonResponse> UpdateBannerAsync(BannerSaveInput input)
        {
            if (input == null || input.BannerId <= 0) return Fail("Invalid banner ID.");

            var entity = await _db.HomepageBanners
                .FirstOrDefaultAsync(b => b.ID_Banner == input.BannerId && !b.Cancelled);

            if (entity == null) return Fail("Banner not found.");

            entity.Title = string.IsNullOrWhiteSpace(input.Title) ? null : input.Title.Trim();
            if (!string.IsNullOrWhiteSpace(input.ImageUrl))
            {
                entity.ImageUrl = input.ImageUrl;
            }
            entity.IsActive = input.IsActive;
            entity.DisplayOrder = input.DisplayOrder;
            entity.UpdatedAt = DateTime.UtcNow;

            // Sync categories
            var existingLinks = await _db.HomepageBannerCategories
                .Where(hbc => hbc.FK_Banner == entity.ID_Banner)
                .ToListAsync();
            _db.HomepageBannerCategories.RemoveRange(existingLinks);

            if (input.CategoryIds != null && input.CategoryIds.Count > 0)
            {
                var validCategoryIds = await _db.Categories.AsNoTracking()
                    .Where(c => input.CategoryIds.Contains(c.ID_Category) && !c.Cancelled)
                    .Select(c => c.ID_Category)
                    .ToListAsync();

                foreach (var catId in validCategoryIds.Distinct())
                {
                    _db.HomepageBannerCategories.Add(new HomepageBannerCategoryEntity
                    {
                        FK_Banner = entity.ID_Banner,
                        FK_Category = catId
                    });
                }
            }

            await _db.SaveChangesAsync();
            return Ok(entity.ID_Banner, "Banner updated successfully.");
        }

        public async Task SetBannerImageUrlAsync(int bannerId, string? imageUrl)
        {
            var entity = await _db.HomepageBanners.FirstOrDefaultAsync(b => b.ID_Banner == bannerId);
            if (entity != null && !string.IsNullOrWhiteSpace(imageUrl))
            {
                entity.ImageUrl = imageUrl;
                await _db.SaveChangesAsync();
            }
        }

        public async Task<CommonResponse> DeleteBannerAsync(int id)
        {
            if (id <= 0) return Fail("Invalid banner ID.");

            var entity = await _db.HomepageBanners.FirstOrDefaultAsync(b => b.ID_Banner == id && !b.Cancelled);
            if (entity == null) return Fail("Banner not found.");

            entity.Cancelled = true;
            entity.CancelledOn = DateTime.UtcNow;
            entity.CancelledReason = "Deleted by admin";

            var links = await _db.HomepageBannerCategories.Where(hbc => hbc.FK_Banner == id).ToListAsync();
            _db.HomepageBannerCategories.RemoveRange(links);

            await _db.SaveChangesAsync();
            return Ok(id, "Banner deleted successfully.");
        }

        // =========================================================================
        // Featured Categories
        // =========================================================================
        public async Task<List<FeaturedCategoryDto>> GetFeaturedCategoriesAsync()
        {
            return await (
                from hc in _db.HomepageCategories.AsNoTracking()
                join c in _db.Categories.AsNoTracking() on hc.FK_Category equals c.ID_Category
                where !hc.Cancelled && !c.Cancelled
                orderby hc.DisplayOrder, hc.ID_HomepageCategory
                select new FeaturedCategoryDto
                {
                    HomepageCategoryId = hc.ID_HomepageCategory,
                    CategoryId = c.ID_Category,
                    CategoryName = c.Name,
                    ImageUrl = c.ImageUrl ?? string.Empty,
                    IsActive = hc.IsActive,
                    DisplayOrder = hc.DisplayOrder,
                    CreatedAt = hc.CreatedAt
                }
            ).ToListAsync();
        }

        public async Task<CommonResponse> SaveFeaturedCategoryAsync(FeaturedCategorySaveInputVIEW input)
        {
            if (input == null || input.CategoryId <= 0) return Fail("Please select a valid category.");

            var categoryExists = await _db.Categories.AsNoTracking()
                .AnyAsync(c => c.ID_Category == input.CategoryId && !c.Cancelled);

            if (!categoryExists) return Fail("Selected category does not exist.");

            if (input.HomepageCategoryId > 0)
            {
                var existing = await _db.HomepageCategories
                    .FirstOrDefaultAsync(hc => hc.ID_HomepageCategory == input.HomepageCategoryId && !hc.Cancelled);

                if (existing == null) return Fail("Featured category entry not found.");

                // Check duplicate
                var dup = await _db.HomepageCategories.AsNoTracking()
                    .AnyAsync(hc => hc.FK_Category == input.CategoryId
                        && hc.ID_HomepageCategory != input.HomepageCategoryId
                        && !hc.Cancelled);

                if (dup) return Fail("This category is already featured on the homepage.");

                existing.FK_Category = input.CategoryId;
                existing.IsActive = input.IsActive ?? true;
                existing.DisplayOrder = input.DisplayOrder;
                existing.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return Ok(existing.ID_HomepageCategory, "Featured category updated.");
            }
            else
            {
                var dup = await _db.HomepageCategories.AsNoTracking()
                    .AnyAsync(hc => hc.FK_Category == input.CategoryId && !hc.Cancelled);

                if (dup) return Fail("This category is already featured on the homepage.");

                var entity = new HomepageCategoryEntity
                {
                    FK_Category = input.CategoryId,
                    IsActive = input.IsActive ?? true,
                    DisplayOrder = input.DisplayOrder,
                    CreatedAt = DateTime.UtcNow,
                    Cancelled = false
                };

                _db.HomepageCategories.Add(entity);
                await _db.SaveChangesAsync();
                return Ok(entity.ID_HomepageCategory, "Category added to homepage.");
            }
        }

        public async Task<CommonResponse> ToggleFeaturedCategoryStatusAsync(int id)
        {
            var entity = await _db.HomepageCategories.FirstOrDefaultAsync(hc => hc.ID_HomepageCategory == id && !hc.Cancelled);
            if (entity == null) return Fail("Featured category not found.");

            entity.IsActive = !entity.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(id, $"Status updated to {(entity.IsActive ? "Active" : "Inactive")}.");
        }

        public async Task<CommonResponse> UpdateFeaturedCategoryOrderAsync(int id, int order)
        {
            var entity = await _db.HomepageCategories.FirstOrDefaultAsync(hc => hc.ID_HomepageCategory == id && !hc.Cancelled);
            if (entity == null) return Fail("Featured category not found.");

            entity.DisplayOrder = order;
            entity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(id, "Display order updated.");
        }

        public async Task<CommonResponse> DeleteFeaturedCategoryAsync(int id)
        {
            var entity = await _db.HomepageCategories.FirstOrDefaultAsync(hc => hc.ID_HomepageCategory == id && !hc.Cancelled);
            if (entity == null) return Fail("Featured category not found.");

            entity.Cancelled = true;
            entity.CancelledOn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(id, "Featured category removed.");
        }

        // =========================================================================
        // Featured Products
        // =========================================================================
        public async Task<List<FeaturedProductDto>> GetFeaturedProductsAsync()
        {
            var raw = await (
                from hp in _db.HomepageProducts.AsNoTracking()
                join p in _db.Products.AsNoTracking() on hp.FK_Product equals p.ID_Product
                join sc in _db.SubCategories.AsNoTracking() on p.FK_SubCategory equals sc.ID_SubCategory
                join c in _db.Categories.AsNoTracking() on sc.FK_Category equals c.ID_Category
                where !hp.Cancelled && !p.Cancelled
                orderby hp.DisplayOrder, hp.ID_HomepageProduct
                select new
                {
                    hp.ID_HomepageProduct,
                    p.ID_Product,
                    p.Name,
                    CategoryName = c.Name,
                    hp.IsActive,
                    hp.DisplayOrder,
                    hp.CreatedAt
                }
            ).ToListAsync();

            if (raw.Count == 0) return new List<FeaturedProductDto>();

            var pIds = raw.Select(r => r.ID_Product).ToList();

            // Fetch primary images from ProductMedia / SkuMedia
            var pImages = await _db.ProductMedia.AsNoTracking()
                .Where(pm => pIds.Contains(pm.FK_Product) && pm.MediaType == "Image" && !string.IsNullOrEmpty(pm.MediaUrl))
                .OrderByDescending(pm => pm.IsPrimary)
                .ThenBy(pm => pm.DisplayOrder)
                .GroupBy(pm => pm.FK_Product)
                .Select(g => new { ProductId = g.Key, Url = g.First().MediaUrl })
                .ToListAsync();

            var imgMap = pImages.ToDictionary(x => x.ProductId, x => x.Url);

            // Fetch starting price
            var prices = await _db.ProductVariants.AsNoTracking()
                .Where(pv => pIds.Contains(pv.FK_Product) && !pv.Cancelled && pv.IsActive)
                .GroupBy(pv => pv.FK_Product)
                .Select(g => new { ProductId = g.Key, MinPrice = g.Min(x => x.SellingPrice) })
                .ToListAsync();

            var priceMap = prices.ToDictionary(x => x.ProductId, x => x.MinPrice);

            return raw.Select(r => new FeaturedProductDto
            {
                HomepageProductId = r.ID_HomepageProduct,
                ProductId = r.ID_Product,
                ProductName = r.Name,
                CategoryName = r.CategoryName,
                ImageUrl = imgMap.TryGetValue(r.ID_Product, out var url) ? url : string.Empty,
                Price = priceMap.TryGetValue(r.ID_Product, out var p) ? p : 0,
                IsActive = r.IsActive,
                DisplayOrder = r.DisplayOrder,
                CreatedAt = r.CreatedAt
            }).ToList();
        }

        public async Task<CommonResponse> SaveFeaturedProductAsync(FeaturedProductSaveInputVIEW input)
        {
            if (input == null || input.ProductId <= 0) return Fail("Please select a valid product.");

            var productExists = await _db.Products.AsNoTracking()
                .AnyAsync(p => p.ID_Product == input.ProductId && !p.Cancelled);

            if (!productExists) return Fail("Selected product does not exist.");

            if (input.HomepageProductId > 0)
            {
                var existing = await _db.HomepageProducts
                    .FirstOrDefaultAsync(hp => hp.ID_HomepageProduct == input.HomepageProductId && !hp.Cancelled);

                if (existing == null) return Fail("Featured product entry not found.");

                var dup = await _db.HomepageProducts.AsNoTracking()
                    .AnyAsync(hp => hp.FK_Product == input.ProductId
                        && hp.ID_HomepageProduct != input.HomepageProductId
                        && !hp.Cancelled);

                if (dup) return Fail("This product is already featured on the homepage.");

                existing.FK_Product = input.ProductId;
                existing.IsActive = input.IsActive ?? true;
                existing.DisplayOrder = input.DisplayOrder;
                existing.UpdatedAt = DateTime.UtcNow;

                await _db.SaveChangesAsync();
                return Ok(existing.ID_HomepageProduct, "Featured product updated.");
            }
            else
            {
                var dup = await _db.HomepageProducts.AsNoTracking()
                    .AnyAsync(hp => hp.FK_Product == input.ProductId && !hp.Cancelled);

                if (dup) return Fail("This product is already featured on the homepage.");

                var entity = new HomepageProductEntity
                {
                    FK_Product = input.ProductId,
                    IsActive = input.IsActive ?? true,
                    DisplayOrder = input.DisplayOrder,
                    CreatedAt = DateTime.UtcNow,
                    Cancelled = false
                };

                _db.HomepageProducts.Add(entity);
                await _db.SaveChangesAsync();
                return Ok(entity.ID_HomepageProduct, "Product added to homepage.");
            }
        }

        public async Task<CommonResponse> ToggleFeaturedProductStatusAsync(int id)
        {
            var entity = await _db.HomepageProducts.FirstOrDefaultAsync(hp => hp.ID_HomepageProduct == id && !hp.Cancelled);
            if (entity == null) return Fail("Featured product not found.");

            entity.IsActive = !entity.IsActive;
            entity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(id, $"Status updated to {(entity.IsActive ? "Active" : "Inactive")}.");
        }

        public async Task<CommonResponse> UpdateFeaturedProductOrderAsync(int id, int order)
        {
            var entity = await _db.HomepageProducts.FirstOrDefaultAsync(hp => hp.ID_HomepageProduct == id && !hp.Cancelled);
            if (entity == null) return Fail("Featured product not found.");

            entity.DisplayOrder = order;
            entity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(id, "Display order updated.");
        }

        public async Task<CommonResponse> DeleteFeaturedProductAsync(int id)
        {
            var entity = await _db.HomepageProducts.FirstOrDefaultAsync(hp => hp.ID_HomepageProduct == id && !hp.Cancelled);
            if (entity == null) return Fail("Featured product not found.");

            entity.Cancelled = true;
            entity.CancelledOn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(id, "Featured product removed.");
        }

        // =========================================================================
        // Lookups
        // =========================================================================
        public async Task<List<CategoryOption>> GetAvailableCategoryOptionsAsync()
        {
            return await _db.Categories.AsNoTracking()
                .Where(c => !c.Cancelled && c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new CategoryOption
                {
                    CategoryId = c.ID_Category,
                    CategoryName = c.Name,
                    ImageUrl = c.ImageUrl ?? string.Empty
                })
                .ToListAsync();
        }

        public async Task<List<ProductOption>> GetAvailableProductOptionsAsync()
        {
            return await (
                from p in _db.Products.AsNoTracking()
                join sc in _db.SubCategories.AsNoTracking() on p.FK_SubCategory equals sc.ID_SubCategory
                join c in _db.Categories.AsNoTracking() on sc.FK_Category equals c.ID_Category
                where !p.Cancelled && p.IsActive && c.IsActive && sc.IsActive
                orderby p.Name
                select new ProductOption
                {
                    ProductId = p.ID_Product,
                    ProductName = p.Name,
                    CategoryName = c.Name
                }
            ).ToListAsync();
        }

        private static CommonResponse Ok(long code, string message) => new()
        {
            ResponseCode = code,
            StatusCode = true,
            ResponseMsg = message
        };

        private static CommonResponse Fail(string message) => new()
        {
            ResponseCode = -1,
            StatusCode = false,
            ResponseMsg = message
        };
    }
}
