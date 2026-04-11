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
                var normalized = ShopHelper.NormalizeInput(input);
                var filteredQuery = ShopHelper.ApplyFilters(_dbContext.Products.AsNoTracking(), normalized);
                var totalCount = await filteredQuery.LongCountAsync();
                var sortedQuery = ShopHelper.ApplySorting(filteredQuery, normalized.SortColumn, normalized.SortMode);
                var rows = await sortedQuery
                    .Skip((normalized.PageIndex - 1) * normalized.PageSize)
                    .Take(normalized.PageSize)
                    .Select(p => new Product
                    {
                        ProductId = p.ProductId,
                        Name = p.Name,
                        CategoryId = p.CategoryId ?? 0,
                        SubCategoryId = p.SubCategoryId ?? 0,
                        BrandId = p.BrandId ?? 0,
                        Rating = (int)Math.Round((double)(p.Rating ?? 0m)),
                        Gender = p.Gender ?? string.Empty,
                        Price = p.Price,
                        MRP = p.MRP ?? 0m
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
