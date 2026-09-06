using System.Data;
using Dapper;
using Ecommerce.Helpers.Shop;
using Ecommerce.Interface;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.ProductModel;

namespace Ecommerce.Repository
{
    public class ShopRepository : ShopInterface
    {
        private readonly IDataAccessDapper _dapper;

        public ShopRepository(IDataAccessDapper dapper)
        {
            _dapper = dapper;
        }

        public async Task<TableOutput<Product>> GetProductListAsync(InputProduct input)
        {
            if (input == null)
            {
                return ShopHelper.EmptyTableOutput(null);
            }

            var normalized = ShopHelper.NormalizeInput(input);
            var result = await _dapper.GetMultipleListByStoredProcedure<Product>("GetProducts", new
            {
                normalized.PageIndex,
                normalized.PageSize,
                SearchName = input.SearchName?.Trim(),
                normalized.SortColumn,
                normalized.SortMode,
                CategoryIds = JoinIds(ShopHelper.ParseShopIds(input.CategoryIds)),
                SubCategoryIds = JoinIds(normalized.SubCategoryIds),
                BrandIds = JoinIds(normalized.BrandIds),
                normalized.PriceFrom,
                normalized.PriceTo
            });

            return result ?? ShopHelper.EmptyTableOutput(input);
        }

        public async Task<ShopFilterLookups> GetFilterLookupsAsync()
        {
            using var connection = _dapper.CreateConnection();
            using var multi = await connection.QueryMultipleAsync(
                "GetShopFilters",
                commandType: CommandType.StoredProcedure);

            return new ShopFilterLookups
            {
                Categories = (await multi.ReadAsync<ShopFilterOption>()).ToList(),
                SubCategories = (await multi.ReadAsync<ShopFilterOption>()).ToList(),
                Brands = (await multi.ReadAsync<ShopFilterOption>()).ToList()
            };
        }

        public async Task<ShopProductDetails?> GetProductDetailsAsync(string slug)
        {
            var key = (slug ?? string.Empty).Trim();
            if (key.Length == 0)
            {
                return null;
            }

            using var connection = _dapper.CreateConnection();
            using var multi = await connection.QueryMultipleAsync(
                "GetProductDetails",
                new { Slug = key },
                commandType: CommandType.StoredProcedure);

            var product = await multi.ReadFirstOrDefaultAsync<ShopProductDetails>();
            if (product == null)
            {
                return null;
            }

            var productImages = (await multi.ReadAsync<ShopMediaRow>())
                .Select(x => x.MediaUrl)
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .ToList();

            var skus = (await multi.ReadAsync<ShopSkuOption>()).ToList();
            var attributes = (await multi.ReadAsync<ShopSkuAttributeRow>()).ToList();
            var skuImages = (await multi.ReadAsync<ShopSkuImageRow>()).ToList();

            foreach (var sku in skus)
            {
                sku.Attributes = attributes
                    .Where(a => a.ProductVariantId == sku.ProductVariantId)
                    .Select(a => new ShopVariantAttribute
                    {
                        VariantId = a.VariantId,
                        VariantName = a.VariantName,
                        VariantValueId = a.VariantValueId,
                        VariantValueName = a.VariantValueName
                    })
                    .ToList();

                sku.ImageUrls = skuImages
                    .Where(img => img.ProductVariantId == sku.ProductVariantId)
                    .Select(img => img.ImageUrl)
                    .Where(url => !string.IsNullOrWhiteSpace(url))
                    .Distinct()
                    .ToList();
            }

            var inStockSkus = skus.Where(s => s.InStock).ToList();
            var selected = inStockSkus.FirstOrDefault(s => s.IsDefault)
                ?? inStockSkus.FirstOrDefault()
                ?? skus.FirstOrDefault(s => s.IsDefault)
                ?? skus.FirstOrDefault();

            var selectedImages = selected != null && selected.ImageUrls.Count > 0
                ? selected.ImageUrls
                : productImages;

            product.ProductImageUrls = productImages;
            product.ImageUrls = selectedImages;
            product.ImageUrl = selectedImages.FirstOrDefault() ?? string.Empty;
            product.Price = selected?.Price ?? 0;
            product.MRP = selected?.MRP ?? 0;
            product.InStock = selected?.InStock ?? false;
            product.SelectedVariantId = selected?.ProductVariantId ?? 0;
            product.Skus = skus;
            product.AttributeGroups = skus
                .SelectMany(s => s.Attributes)
                .GroupBy(a => new { a.VariantId, a.VariantName })
                .Select(g => new ShopAttributeGroup
                {
                    VariantId = g.Key.VariantId,
                    Name = g.Key.VariantName,
                    Values = g
                        .GroupBy(a => new { a.VariantValueId, a.VariantValueName })
                        .Select(v => new ShopAttributeValue
                        {
                            VariantValueId = v.Key.VariantValueId,
                            Name = v.Key.VariantValueName
                        })
                        .ToList()
                })
                .ToList();

            return product;
        }

        private static string? JoinIds(IReadOnlyList<int> ids)
        {
            return ids.Count == 0 ? null : string.Join(",", ids);
        }

        private sealed class ShopMediaRow
        {
            public string MediaUrl { get; set; } = string.Empty;
        }

        private sealed class ShopSkuImageRow
        {
            public int ProductVariantId { get; set; }
            public string ImageUrl { get; set; } = string.Empty;
        }

        private sealed class ShopSkuAttributeRow
        {
            public int ProductVariantId { get; set; }
            public int VariantId { get; set; }
            public string VariantName { get; set; } = string.Empty;
            public int VariantValueId { get; set; }
            public string VariantValueName { get; set; } = string.Empty;
        }
    }
}
