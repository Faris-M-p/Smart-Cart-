using Ecommerce.DataAccess;
using Ecommerce.Helpers.Shop;
using Ecommerce.Interface;
using Ecommerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.ProductModel;

namespace Ecommerce.Repository
{
    public class ShopRepository : ShopInterface
    {
        private readonly EcommerceDbContext _dbContext;

        public ShopRepository(EcommerceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TableOutput<Product>> GetProductListAsync(InputProduct input)
        {
            if (input == null)
            {
                return ShopHelper.EmptyTableOutput(null);
            }

            try
            {
                var categoryIds = ShopHelper.ParseShopIds(input.CategoryIds);
                var subCategoryIds = ShopHelper.ParseShopIds(input.SubCategoryIds);

                if (categoryIds.Count > 0)
                {
                    var fromCats = await _dbContext.SubCategories.AsNoTracking()
                        .Where(sc => categoryIds.Contains(sc.FK_Category) && sc.Cancelled != true)
                        .Select(sc => sc.ID_SubCategory)
                        .ToListAsync();

                    subCategoryIds = subCategoryIds.Count > 0
                        ? subCategoryIds.Intersect(fromCats).ToList()
                        : fromCats;
                }

                var mergedInput = new InputProduct
                {
                    PageIndex = input.PageIndex,
                    PageSize = input.PageSize,
                    SearchName = input.SearchName,
                    SortColumn = input.SortColumn,
                    SortMode = input.SortMode,
                    CategoryIds = string.Empty,
                    SubCategoryIds = string.Join(",", subCategoryIds),
                    BrandIds = input.BrandIds,
                    PriceFrom = input.PriceFrom,
                    PriceTo = input.PriceTo
                };

                var normalized = ShopHelper.NormalizeInput(mergedInput);
                var productQuery = ShopHelper.ApplyFilters(_dbContext.Products.AsNoTracking(), normalized);

                var skuMin = _dbContext.ProductVariants.AsNoTracking()
                    .Where(v => !v.Cancelled && v.IsActive)
                    .GroupBy(v => v.FK_Product)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        MinPrice = g.Min(x => x.SellingPrice)
                    });

                var listed =
                    from p in productQuery
                    join sc in _dbContext.SubCategories.AsNoTracking() on p.FK_SubCategory equals sc.ID_SubCategory
                    join cat in _dbContext.Categories.AsNoTracking() on sc.FK_Category equals cat.ID_Category
                    join b in _dbContext.Brands.AsNoTracking() on p.FK_Brand equals b.ID_Brand into bj
                    from b in bj.DefaultIfEmpty()
                    join price in skuMin on p.ID_Product equals price.ProductId into pj
                    from price in pj.DefaultIfEmpty()
                    select new ShopProductListItem
                    {
                        ProductId = p.ID_Product,
                        Name = p.Name,
                        Slug = p.Slug,
                        CategoryId = cat.ID_Category,
                        CategoryName = cat.Name,
                        SubCategoryId = sc.ID_SubCategory,
                        BrandId = b != null ? b.ID_Brand : 0,
                        BrandName = b != null ? b.BrandName : string.Empty,
                        MinPrice = price != null ? price.MinPrice : null,
                        CreatedAt = p.CreatedAt
                    };

                listed = ShopHelper.ApplyPriceFilter(listed, normalized);
                var totalCount = await listed.LongCountAsync();
                var pageRows = await ShopHelper.ApplySorting(listed, normalized.SortColumn, normalized.SortMode)
                    .Skip((normalized.PageIndex - 1) * normalized.PageSize)
                    .Take(normalized.PageSize)
                    .ToListAsync();

                var cards = await MapCardsAsync(pageRows);

                return new TableOutput<Product>
                {
                    TableData = cards,
                    TableSettings = new TableOutput_Settings
                    {
                        PageIndex = normalized.PageIndex,
                        PageSize = normalized.PageSize,
                        TotalCount = totalCount
                    }
                };
            }
            catch
            {
                throw;
            }
        }

        public async Task<ShopFilterLookups> GetFilterLookupsAsync()
        {
            var categories = await _dbContext.Categories.AsNoTracking()
                .Where(c => !c.Cancelled && c.IsActive)
                .OrderBy(c => c.Name)
                .Select(c => new ShopFilterOption
                {
                    Id = c.ID_Category,
                    Name = c.Name
                })
                .ToListAsync();

            var subCategories = await _dbContext.SubCategories.AsNoTracking()
                .Where(sc => sc.Cancelled != true && sc.IsActive)
                .OrderBy(sc => sc.Name)
                .Select(sc => new ShopFilterOption
                {
                    Id = sc.ID_SubCategory,
                    Name = sc.Name,
                    CategoryId = sc.FK_Category
                })
                .ToListAsync();

            var brands = await _dbContext.Brands.AsNoTracking()
                .Where(b => !b.Cancelled && b.IsActive)
                .OrderBy(b => b.BrandName)
                .Select(b => new ShopFilterOption
                {
                    Id = b.ID_Brand,
                    Name = b.BrandName
                })
                .ToListAsync();

            return new ShopFilterLookups
            {
                Categories = categories,
                SubCategories = subCategories,
                Brands = brands
            };
        }

        public async Task<ShopProductDetails?> GetProductDetailsAsync(string slug)
        {
            var key = (slug ?? string.Empty).Trim();
            if (key.Length == 0)
            {
                return null;
            }

            var query = _dbContext.Products.AsNoTracking()
                .Where(p => p.Cancelled != true && p.IsActive);

            ProductEntity? product;
            if (int.TryParse(key, out var productId) && productId > 0)
            {
                product = await query.FirstOrDefaultAsync(p => p.ID_Product == productId || p.Slug == key);
            }
            else
            {
                product = await query.FirstOrDefaultAsync(p => p.Slug == key);
            }

            if (product == null)
            {
                return null;
            }

            var categoryRow = await (
                from sc in _dbContext.SubCategories.AsNoTracking()
                join cat in _dbContext.Categories.AsNoTracking() on sc.FK_Category equals cat.ID_Category
                where sc.ID_SubCategory == product.FK_SubCategory
                select new { CategoryId = cat.ID_Category, CategoryName = cat.Name, SubCategoryName = sc.Name })
                .FirstOrDefaultAsync();

            string brandName = string.Empty;
            if (product.FK_Brand.HasValue)
            {
                brandName = await _dbContext.Brands.AsNoTracking()
                    .Where(b => b.ID_Brand == product.FK_Brand.Value)
                    .Select(b => b.BrandName)
                    .FirstOrDefaultAsync() ?? string.Empty;
            }

            var variants = await _dbContext.ProductVariants.AsNoTracking()
                .Where(v => v.FK_Product == product.ID_Product && !v.Cancelled && v.IsActive)
                .Select(v => new { v.ID_ProductVariant, v.SellingPrice, v.MRP, v.IsDefault })
                .ToListAsync();

            var cheapest = variants
                .OrderBy(v => v.SellingPrice)
                .ThenByDescending(v => v.IsDefault)
                .FirstOrDefault();

            var imageUrls = await _dbContext.ProductMedia.AsNoTracking()
                .Where(m => m.FK_Product == product.ID_Product
                    && m.MediaType == "Image"
                    && m.MediaUrl != null
                    && m.MediaUrl != "")
                .OrderByDescending(m => m.IsPrimary)
                .ThenBy(m => m.DisplayOrder)
                .Select(m => m.MediaUrl)
                .ToListAsync();

            if (imageUrls.Count == 0 && variants.Count > 0)
            {
                var skuIds = variants.Select(v => v.ID_ProductVariant).ToList();
                imageUrls = await _dbContext.ProductVariantImages.AsNoTracking()
                    .Where(m => skuIds.Contains(m.FK_ProductVariant)
                        && m.MediaType == "Image"
                        && m.ImageUrl != null
                        && m.ImageUrl != "")
                    .OrderByDescending(m => m.IsPrimary)
                    .ThenBy(m => m.DisplayOrder)
                    .Select(m => m.ImageUrl)
                    .Distinct()
                    .ToListAsync();
            }

            var stockQty = 0;
            if (variants.Count > 0)
            {
                var skuIds = variants.Select(v => v.ID_ProductVariant).ToList();
                stockQty = await _dbContext.Stock.AsNoTracking()
                    .Where(s => skuIds.Contains(s.FK_ProductVariant) && !s.Cancelled)
                    .SumAsync(s => (int?)s.Quantity) ?? 0;
            }

            return new ShopProductDetails
            {
                ProductId = product.ID_Product,
                Name = product.Name,
                Slug = product.Slug,
                Description = product.Description ?? string.Empty,
                CategoryId = categoryRow?.CategoryId ?? 0,
                CategoryName = categoryRow?.CategoryName ?? string.Empty,
                SubCategoryId = product.FK_SubCategory,
                SubCategoryName = categoryRow?.SubCategoryName ?? string.Empty,
                BrandId = product.FK_Brand ?? 0,
                BrandName = brandName,
                ImageUrl = imageUrls.FirstOrDefault() ?? string.Empty,
                ImageUrls = imageUrls,
                Price = cheapest?.SellingPrice ?? 0,
                MRP = cheapest?.MRP ?? 0,
                InStock = stockQty > 0
            };
        }

        private async Task<List<Product>> MapCardsAsync(List<ShopProductListItem> pageRows)
        {
            if (pageRows.Count == 0)
            {
                return new List<Product>();
            }

            var productIds = pageRows.Select(r => r.ProductId).ToList();

            var variants = await _dbContext.ProductVariants.AsNoTracking()
                .Where(v => productIds.Contains(v.FK_Product) && !v.Cancelled && v.IsActive)
                .Select(v => new
                {
                    v.ID_ProductVariant,
                    v.FK_Product,
                    v.SellingPrice,
                    v.MRP,
                    v.IsDefault
                })
                .ToListAsync();

            var cheapestByProduct = variants
                .GroupBy(v => v.FK_Product)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderBy(v => v.SellingPrice).ThenByDescending(v => v.IsDefault).First());

            var productImages = await _dbContext.ProductMedia.AsNoTracking()
                .Where(m => productIds.Contains(m.FK_Product)
                    && m.MediaType == "Image"
                    && m.MediaUrl != null
                    && m.MediaUrl != "")
                .OrderByDescending(m => m.IsPrimary)
                .ThenBy(m => m.DisplayOrder)
                .Select(m => new { m.FK_Product, m.MediaUrl })
                .ToListAsync();

            var imageByProduct = productImages
                .GroupBy(m => m.FK_Product)
                .ToDictionary(g => g.Key, g => g.First().MediaUrl);

            var missingImageIds = productIds.Where(id => !imageByProduct.ContainsKey(id)).ToList();
            if (missingImageIds.Count > 0)
            {
                var skuIds = variants
                    .Where(v => missingImageIds.Contains(v.FK_Product))
                    .Select(v => v.ID_ProductVariant)
                    .ToList();

                if (skuIds.Count > 0)
                {
                    var skuImages = await _dbContext.ProductVariantImages.AsNoTracking()
                        .Where(m => skuIds.Contains(m.FK_ProductVariant)
                            && m.MediaType == "Image"
                            && m.ImageUrl != null
                            && m.ImageUrl != "")
                        .OrderByDescending(m => m.IsPrimary)
                        .ThenBy(m => m.DisplayOrder)
                        .Select(m => new { m.FK_ProductVariant, m.ImageUrl })
                        .ToListAsync();

                    var skuToProduct = variants.ToDictionary(v => v.ID_ProductVariant, v => v.FK_Product);
                    foreach (var image in skuImages)
                    {
                        if (!skuToProduct.TryGetValue(image.FK_ProductVariant, out var productId)
                            || imageByProduct.ContainsKey(productId))
                        {
                            continue;
                        }

                        imageByProduct[productId] = image.ImageUrl;
                    }
                }
            }

            var stockByProduct = await (
                from s in _dbContext.Stock.AsNoTracking()
                join v in _dbContext.ProductVariants.AsNoTracking() on s.FK_ProductVariant equals v.ID_ProductVariant
                where productIds.Contains(v.FK_Product)
                    && !s.Cancelled
                    && !v.Cancelled
                    && v.IsActive
                group s.Quantity by v.FK_Product into g
                select new { ProductId = g.Key, Qty = g.Sum() })
                .ToDictionaryAsync(x => x.ProductId, x => x.Qty);

            return pageRows.Select(row =>
            {
                cheapestByProduct.TryGetValue(row.ProductId, out var cheapest);
                imageByProduct.TryGetValue(row.ProductId, out var imageUrl);
                stockByProduct.TryGetValue(row.ProductId, out var qty);

                return new Product
                {
                    ProductId = row.ProductId,
                    Name = row.Name,
                    Slug = row.Slug,
                    CategoryId = row.CategoryId,
                    CategoryName = row.CategoryName,
                    SubCategoryId = row.SubCategoryId,
                    BrandId = row.BrandId,
                    BrandName = row.BrandName ?? string.Empty,
                    ImageUrl = imageUrl ?? string.Empty,
                    Price = cheapest?.SellingPrice ?? row.MinPrice ?? 0,
                    MRP = cheapest?.MRP ?? 0,
                    InStock = qty > 0
                };
            }).ToList();
        }
    }
}
