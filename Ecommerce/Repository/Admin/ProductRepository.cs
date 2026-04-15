using Ecommerce.DataAccess;
using Ecommerce.Helpers.Common;
using Ecommerce.Helpers.Products;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.ProductModel;

namespace Ecommerce.Repository.Admin
{
    public class ProductRepository : IProductInterface
    {
        private readonly EcommerceDbContext _dbContext;

        public ProductRepository(EcommerceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TableOutput<Product>> GetProductListAsync(ProductListInput input)
        {
            if (input == null)
            {
                return ProductHelper.EmptyTableOutput(null);
            }

            try
            {
                var categoryIds = StringHelper.ParseFilterIds(input.FilterCategoryIDs);
                var normalized = ProductHelper.NormalizeInput(input);

                var query = _dbContext.Products.AsNoTracking().Where(p => p.Cancelled != true);

                if (categoryIds.Count > 0)
                {
                    var allowedSubIds = _dbContext.SubCategories.AsNoTracking()
                        .Where(sc => categoryIds.Contains(sc.FK_Category) && sc.Cancelled != true)
                        .Select(sc => sc.ID_SubCategory);
                    query = query.Where(p => allowedSubIds.Contains(p.FK_SubCategory));
                }

                query = ProductHelper.ApplyFilters(query, normalized);

                var totalCount = await query.LongCountAsync();
                var sortedQuery = ProductHelper.ApplySorting(query, normalized.SortColumn, normalized.SortMode);

                var pageIds = await sortedQuery
                    .Skip((normalized.PageIndex - 1) * normalized.PageSize)
                    .Take(normalized.PageSize)
                    .Select(p => p.ID_Product)
                    .ToListAsync();

                if (pageIds.Count == 0)
                {
                    return new TableOutput<Product>
                    {
                        TableData = new List<Product>(),
                        TableSettings = new TableOutput_Settings
                        {
                            PageIndex = normalized.PageIndex,
                            PageSize = normalized.PageSize,
                            TotalCount = totalCount
                        }
                    };
                }

                var rowsUnordered = await (
                    from p in _dbContext.Products.AsNoTracking()
                    join sc in _dbContext.SubCategories.AsNoTracking() on p.FK_SubCategory equals sc.ID_SubCategory
                    join c in _dbContext.Categories.AsNoTracking() on sc.FK_Category equals c.ID_Category
                    join b in _dbContext.Brands.AsNoTracking() on p.FK_Brand equals b.ID_Brand into bg
                    from b in bg.DefaultIfEmpty()
                    where pageIds.Contains(p.ID_Product)
                    select new Product
                    {
                        ID_Product = p.ID_Product,
                        Name = p.Name,
                        Slug = p.Slug,
                        FK_SubCategory = p.FK_SubCategory,
                        FK_Brand = p.FK_Brand,
                        FK_Category = sc.FK_Category,
                        CategoryName = c.Name,
                        SubCategoryName = sc.Name,
                        BrandName = b != null ? b.BrandName : null,
                        Description = p.Description,
                        IsActive = p.IsActive,
                        Cancelled = p.Cancelled
                    }).ToListAsync();

                var index = pageIds.Select((id, i) => (id, i)).ToDictionary(x => x.id, x => x.i);
                var rows = rowsUnordered.OrderBy(r => index[r.ID_Product]).ToList();

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
                return ProductHelper.EmptyTableOutput(input);
            }
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            return await (
                from p in _dbContext.Products.AsNoTracking()
                join sc in _dbContext.SubCategories.AsNoTracking() on p.FK_SubCategory equals sc.ID_SubCategory
                join c in _dbContext.Categories.AsNoTracking() on sc.FK_Category equals c.ID_Category
                join b in _dbContext.Brands.AsNoTracking() on p.FK_Brand equals b.ID_Brand into bg
                from b in bg.DefaultIfEmpty()
                where p.ID_Product == id && !p.Cancelled
                select new Product
                {
                    ID_Product = p.ID_Product,
                    Name = p.Name,
                    Slug = p.Slug,
                    FK_SubCategory = p.FK_SubCategory,
                    FK_Brand = p.FK_Brand,
                    FK_Category = sc.FK_Category,
                    CategoryName = c.Name,
                    SubCategoryName = sc.Name,
                    BrandName = b != null ? b.BrandName : null,
                    Description = p.Description,
                    IsActive = p.IsActive,
                    Cancelled = p.Cancelled
                }).FirstOrDefaultAsync();
        }

        public async Task<CommonResponse> CreateProductAsync(ProductUpdateInput input)
        {
            if (input == null)
            {
                return Fail("Invalid request.");
            }

            try
            {
                var n = ProductHelper.NormalizeWriteInput(input);
                if (n.Name.Length == 0)
                {
                    return Fail("Please enter product name.");
                }

                if (!await SubCategoryExistsAsync(n.FK_SubCategory))
                {
                    return Fail("Invalid subcategory.");
                }

                if (n.FK_Brand.HasValue && !await BrandExistsAsync(n.FK_Brand.Value))
                {
                    return Fail("Invalid brand.");
                }

                var baseSlug = n.SlugOverride != null
                    ? ProductHelper.GenerateSlug(n.SlugOverride)
                    : ProductHelper.GenerateSlug(n.Name);
                var slug = await EnsureUniqueSlugAsync(baseSlug, excludeProductId: 0);

                var entity = new ProductEntity
                {
                    FK_SubCategory = n.FK_SubCategory,
                    FK_Brand = n.FK_Brand,
                    Name = n.Name,
                    Slug = slug,
                    Description = n.Description,
                    IsActive = n.IsActive,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = null,
                    Cancelled = false,
                    CancelledOn = null
                };

                _dbContext.Products.Add(entity);
                await _dbContext.SaveChangesAsync();

                return Ok(entity.ID_Product, "Product created successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while creating product: {ex.Message}");
            }
        }

        public async Task<CommonResponse> UpdateProductAsync(ProductUpdateInput input)
        {
            if (input == null)
            {
                return Fail("Invalid request.");
            }

            try
            {
                if (input.ID_Product <= 0)
                {
                    return Fail("Invalid product ID.");
                }

                var n = ProductHelper.NormalizeWriteInput(input);
                if (n.Name.Length == 0)
                {
                    return Fail("Please enter product name.");
                }

                if (!await SubCategoryExistsAsync(n.FK_SubCategory))
                {
                    return Fail("Invalid subcategory.");
                }

                if (n.FK_Brand.HasValue && !await BrandExistsAsync(n.FK_Brand.Value))
                {
                    return Fail("Invalid brand.");
                }

                var entity = await _dbContext.Products.FirstOrDefaultAsync(p => p.ID_Product == input.ID_Product);
                if (entity == null)
                {
                    return Fail("Invalid product ID.");
                }

                if (entity.Cancelled)
                {
                    return Fail("This product is deleted and cannot be edited.");
                }

                var baseSlug = n.SlugOverride != null
                    ? ProductHelper.GenerateSlug(n.SlugOverride)
                    : ProductHelper.GenerateSlug(n.Name);
                var slug = await EnsureUniqueSlugAsync(baseSlug, excludeProductId: input.ID_Product);

                entity.Name = n.Name;
                entity.Slug = slug;
                entity.Description = n.Description;
                entity.FK_SubCategory = n.FK_SubCategory;
                entity.FK_Brand = n.FK_Brand;
                entity.IsActive = n.IsActive;
                entity.ModifiedAt = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                return Ok(input.ID_Product, "Product updated successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while updating product: {ex.Message}");
            }
        }

        public async Task<CommonResponse> DeleteProductAsync(ProductDeleteInput input)
        {
            if (input == null)
            {
                return Fail("Invalid product ID.");
            }

            try
            {
                if (input.ID_Product <= 0)
                {
                    return Fail("Invalid product ID.");
                }

                var entity = await _dbContext.Products.FirstOrDefaultAsync(p => p.ID_Product == input.ID_Product);
                if (entity == null)
                {
                    return Fail("Invalid product ID.");
                }

                if (entity.Cancelled)
                {
                    return Fail("This product is already deleted.");
                }

                entity.Cancelled = true;
                entity.CancelledOn = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                return Ok(input.ID_Product, "Product deleted successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while deleting product: {ex.Message}");
            }
        }

        private async Task<string> EnsureUniqueSlugAsync(string baseSlug, int excludeProductId)
        {
            var slug = baseSlug;
            var n = 0;
            while (await SlugInUseAsync(slug, excludeProductId))
            {
                n++;
                slug = $"{baseSlug}-{n}";
            }

            return slug;
        }

        private Task<bool> SlugInUseAsync(string slug, int excludeProductId)
        {
            var key = slug.ToLowerInvariant();
            return _dbContext.Products.AnyAsync(p =>
                !p.Cancelled &&
                p.ID_Product != excludeProductId &&
                p.Slug.ToLower() == key);
        }

        private Task<bool> SubCategoryExistsAsync(int subCategoryId) =>
            _dbContext.SubCategories.AnyAsync(sc =>
                sc.ID_SubCategory == subCategoryId &&
                sc.Cancelled != true);

        private Task<bool> BrandExistsAsync(int brandId) =>
            _dbContext.Brands.AnyAsync(b => b.ID_Brand == brandId && !b.Cancelled);

        private static CommonResponse Ok(long responseCode, string message) =>
            new()
            {
                ResponseCode = responseCode,
                StatusCode = true,
                ResponseMsg = message
            };

        private static CommonResponse Fail(string message) =>
            new()
            {
                ResponseCode = -1,
                StatusCode = false,
                ResponseMsg = message
            };
    }
}
