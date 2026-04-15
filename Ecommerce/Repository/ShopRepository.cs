using Ecommerce.DataAccess;
using Ecommerce.Helpers.Common;
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
                var categoryIds = StringHelper.ParseFilterIds(input.CategoryIds);
                var mergedInput = new InputProduct
                {
                    PageIndex = input.PageIndex,
                    PageSize = input.PageSize,
                    SearchName = input.SearchName,
                    SortColumn = input.SortColumn,
                    SortMode = input.SortMode,
                    CategoryIds = string.Empty,
                    SubCategoryIds = input.SubCategoryIds,
                    BrandIds = input.BrandIds,
                    Ratings = string.Empty,
                    Gender = string.Empty,
                    PriceFrom = null,
                    PriceTo = null,
                    Status = string.Empty
                };

                if (categoryIds.Count > 0)
                {
                    var fromCats = await _dbContext.SubCategories.AsNoTracking()
                        .Where(sc => categoryIds.Contains(sc.FK_Category) && sc.Cancelled != true)
                        .Select(sc => sc.ID_SubCategory)
                        .ToListAsync();
                    var existing = StringHelper.ParseFilterIds(input.SubCategoryIds);
                    var merged = existing.Concat(fromCats).Distinct().ToList();
                    mergedInput.SubCategoryIds = StringHelper.FormatFilterIds(merged);
                }

                var normalized = ShopHelper.NormalizeInput(mergedInput);
                var filteredQuery = ShopHelper.ApplyFilters(_dbContext.Products.AsNoTracking(), normalized);
                var totalCount = await filteredQuery.LongCountAsync();
                var sortedQuery = ShopHelper.ApplySorting(filteredQuery, normalized.SortColumn, normalized.SortMode);
                var rows = await sortedQuery
                    .Skip((normalized.PageIndex - 1) * normalized.PageSize)
                    .Take(normalized.PageSize)
                    .Select(p => new Product
                    {
                        ProductId = p.ID_Product,
                        Name = p.Name,
                        CategoryId = 0,
                        SubCategoryId = p.FK_SubCategory,
                        BrandId = p.FK_Brand ?? 0,
                        Rating = 0,
                        Gender = string.Empty,
                        Price = 0,
                        MRP = 0
                    })
                    .ToListAsync();

                return new TableOutput<Product>
                {
                    TableData = rows,
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
                return ShopHelper.EmptyTableOutput(input);
            }
        }
    }
}
