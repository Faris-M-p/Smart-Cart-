using Ecommerce.DataAccess;
using Ecommerce.Helpers.Common;
using Ecommerce.Helpers.Products;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static Ecommerce.Models.Admin.ProductModel;
using static Ecommerce.Models.CommonModel;

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
                var normalized = ProductHelper.NormalizeInput(input);
                var filtered = ProductHelper.ApplyFilters(_dbContext.Products.AsNoTracking(), normalized);
                var totalCount = await filtered.LongCountAsync();
                var sorted = ProductHelper.ApplySorting(filtered, normalized.SortColumn, normalized.SortMode);
                var paged = sorted
                    .Skip((normalized.PageIndex - 1) * normalized.PageSize)
                    .Take(normalized.PageSize);

                var db = _dbContext;
                var rows = await paged
                    .Select(p => new Product
                    {
                        ID_Product = p.ProductId,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        MRP = p.MRP,
                        FK_Category = p.CategoryId ?? 0,
                        FK_SubCategory = p.SubCategoryId ?? 0,
                        FK_Brand = p.BrandId,
                        Rating = p.Rating,
                        Gender = p.Gender,
                        FK_Status = p.StatusId ?? 0,
                        CreatedOn = p.CreatedAt,
                        UpdatedOn = p.UpdatedAt,
                        ImageData = db.ProductImages
                            .Where(pi => pi.ProductId == p.ProductId && pi.Cancelled != true)
                            .OrderBy(pi => pi.ProductImageId)
                            .Select(pi => pi.ImageUrl)
                            .FirstOrDefault(),
                        IsBase64 = null,
                        Cancelled = p.Cancelled ?? false
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
                return ProductHelper.EmptyTableOutput(input);
            }
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            if (id <= 0)
            {
                return null!;
            }

            try
            {
                var db = _dbContext;
                return await _dbContext.Products
                    .AsNoTracking()
                    .Where(p => p.ProductId == id)
                    .Select(p => new Product
                    {
                        ID_Product = p.ProductId,
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        MRP = p.MRP,
                        FK_Category = p.CategoryId ?? 0,
                        FK_SubCategory = p.SubCategoryId ?? 0,
                        FK_Brand = p.BrandId,
                        Rating = p.Rating,
                        Gender = p.Gender,
                        FK_Status = p.StatusId ?? 0,
                        CreatedOn = p.CreatedAt,
                        UpdatedOn = p.UpdatedAt,
                        ImageData = db.ProductImages
                            .Where(pi => pi.ProductId == p.ProductId && pi.Cancelled != true)
                            .OrderBy(pi => pi.ProductImageId)
                            .Select(pi => pi.ImageUrl)
                            .FirstOrDefault(),
                        IsBase64 = null,
                        Cancelled = p.Cancelled ?? false
                    })
                    .FirstOrDefaultAsync()!;
            }
            catch
            {
                return null!;
            }
        }

        public async Task<CommonResponse> CreateProductAsync(ProductUpdateInput input)
        {
            if (input == null)
            {
                return Fail("Product name is required.");
            }

            try
            {
                var normalized = ProductHelper.NormalizeInput(input);
                if (normalized.Name.Length == 0)
                {
                    return Fail("Product name is required.");
                }

                if (normalized.Price <= 0)
                {
                    return Fail("Price must be greater than zero.");
                }

                if (normalized.MRP.HasValue && normalized.MRP < normalized.Price)
                {
                    return Fail("MRP must be >= Price.");
                }

                if (!await ValidCategoryAsync(normalized.FK_Category))
                {
                    return Fail("Invalid Category.");
                }

                if (!await ValidSubCategoryAsync(normalized.FK_SubCategory, normalized.FK_Category))
                {
                    return Fail("Invalid SubCategory.");
                }

                if (await DuplicateProductNameAsync(
                        normalized.Name,
                        normalized.FK_Category,
                        normalized.FK_SubCategory,
                        excludeProductId: 0))
                {
                    return Fail("Duplicate product exists.");
                }

                var entity = new ProductEntity
                {
                    Name = normalized.Name,
                    Description = normalized.Description,
                    Price = normalized.Price,
                    MRP = normalized.MRP,
                    CategoryId = normalized.FK_Category,
                    SubCategoryId = normalized.FK_SubCategory,
                    BrandId = normalized.FK_Brand,
                    Rating = normalized.Rating,
                    Gender = normalized.Gender,
                    StatusId = normalized.FK_Status,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = null,
                    Cancelled = false,
                    CancelledOn = null,
                    CancelledReason = null
                };

                _dbContext.Products.Add(entity);
                await _dbContext.SaveChangesAsync();

                return Ok(entity.ProductId, "Product created successfully.");
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
                var normalized = ProductHelper.NormalizeInput(input);
                if (normalized.Name.Length == 0)
                {
                    return Fail("Product name is required.");
                }

                if (normalized.Price <= 0)
                {
                    return Fail("Price must be greater than zero.");
                }

                if (normalized.MRP.HasValue && normalized.MRP < normalized.Price)
                {
                    return Fail("MRP must be >= Price.");
                }

                if (!await ValidCategoryAsync(normalized.FK_Category))
                {
                    return Fail("Invalid Category.");
                }

                if (!await ValidSubCategoryAsync(normalized.FK_SubCategory, normalized.FK_Category))
                {
                    return Fail("Invalid SubCategory.");
                }

                var entity = await _dbContext.Products
                    .FirstOrDefaultAsync(p => p.ProductId == input.ID_Product);

                if (entity == null)
                {
                    return Fail("Invalid Product ID.");
                }

                if (entity.Cancelled == true)
                {
                    return Fail("Product is deleted.");
                }

                if (await DuplicateProductNameAsync(
                        normalized.Name,
                        normalized.FK_Category,
                        normalized.FK_SubCategory,
                        excludeProductId: input.ID_Product))
                {
                    return Fail("Duplicate product exists.");
                }

                entity.Name = normalized.Name;
                entity.Description = normalized.Description;
                entity.Price = normalized.Price;
                entity.MRP = normalized.MRP;
                entity.CategoryId = normalized.FK_Category;
                entity.SubCategoryId = normalized.FK_SubCategory;
                entity.BrandId = normalized.FK_Brand;
                entity.Rating = normalized.Rating;
                entity.Gender = normalized.Gender;
                entity.StatusId = normalized.FK_Status;
                entity.UpdatedAt = DateTime.Now;

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
                return Fail("Invalid Product ID.");
            }

            try
            {
                var productId = input.ID_Product;
                if (productId <= 0)
                {
                    return Fail("Invalid Product ID.");
                }

                var entity = await _dbContext.Products
                    .FirstOrDefaultAsync(p => p.ProductId == productId);

                if (entity == null)
                {
                    return Fail("Invalid Product ID.");
                }

                if (entity.Cancelled == true)
                {
                    return Fail("Product already deleted.");
                }

                entity.Cancelled = true;
                entity.CancelledOn = DateTime.Now;
                entity.CancelledReason = StringHelper.NormalizeOptionalString(input.CancelledReason);

                await _dbContext.SaveChangesAsync();

                return Ok(productId, "Product deleted successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while deleting product: {ex.Message}");
            }
        }

        private Task<bool> ValidCategoryAsync(int categoryId)
        {
            return _dbContext.Categories.AnyAsync(c =>
                c.IdCategory == categoryId &&
                !c.Cancelled);
        }

        private Task<bool> ValidSubCategoryAsync(int subCategoryId, int categoryId)
        {
            return _dbContext.SubCategories.AnyAsync(s =>
                s.ID_SubCategory == subCategoryId &&
                s.FK_Category == categoryId &&
                s.Cancelled != true);
        }

        private Task<bool> DuplicateProductNameAsync(
            string name,
            int categoryId,
            int subCategoryId,
            int excludeProductId)
        {
            return _dbContext.Products.AnyAsync(p =>
                p.Cancelled != true &&
                p.ProductId != excludeProductId &&
                p.Name == name &&
                p.CategoryId == categoryId &&
                p.SubCategoryId == subCategoryId);
        }

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
