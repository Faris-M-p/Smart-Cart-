using Ecommerce.DataAccess;
using Ecommerce.Helpers.Categories;
using Ecommerce.Helpers.Common;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.CategoryModel;

namespace Ecommerce.Repository.Admin
{
    public class CategoryRepository : ICategoryInterface
    {
        private readonly EcommerceDbContext _dbContext;

        public CategoryRepository(EcommerceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Category>> GetActiveCategoriesAsync()
        {
            return await _dbContext.Categories
                .AsNoTracking()
                .Where(c => c.Cancelled != true)
                .OrderBy(c => c.Name)
                .Select(c => new Category
                {
                    CategoryID = c.IdCategory,
                    CategoryName = c.Name,
                    IsActive = c.IsActive,
                    Cancelled = c.Cancelled
                })
                .ToListAsync();
        }

        public async Task<Category?> GetCategoryByIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            return await _dbContext.Categories
                .AsNoTracking()
                .Where(c => c.IdCategory == id)
                .Select(c => new Category
                {
                    CategoryID = c.IdCategory,
                    CategoryName = c.Name,
                    Description = c.Description,
                    IsActive = c.IsActive,
                    Cancelled = c.Cancelled,
                    CancelledOn = c.CancelledOn,
                    CancelledReason = c.CancelledReason
                })
                .FirstOrDefaultAsync();
        }

        public async Task<TableOutput<Category>> GetCategoryListAsync(CategoryListInput input)
        {
            if (input == null)
            {
                return CategoryHelper.EmptyTableOutput(null);
            }

            try
            {
                var normalized = CategoryHelper.NormalizeInput(input);

                var filteredQuery = CategoryHelper.ApplyFilters(
                    _dbContext.Categories.AsNoTracking(),
                    normalized);

                var totalCount = await filteredQuery.LongCountAsync();

                var sortedQuery = CategoryHelper.ApplySorting(
                    filteredQuery,
                    normalized.SortColumn,
                    normalized.SortMode);

                var pagedEntityQuery = sortedQuery
                    .Skip((normalized.PageIndex - 1) * normalized.PageSize)
                    .Take(normalized.PageSize);

                // ✅ INLINE MAPPING (no helper)
                var rows = await pagedEntityQuery
                    .Select(c => new Category
                    {
                        CategoryID = c.IdCategory,
                        CategoryName = c.Name,
                        Description = c.Description,
                        IsActive = c.IsActive,
                        Cancelled = c.Cancelled,
                        CancelledOn = c.CancelledOn,
                        CancelledReason = c.CancelledReason
                    })
                    .ToListAsync();

                return new TableOutput<Category>
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
                return CategoryHelper.EmptyTableOutput(input);
            }
        }

        public async Task<CommonResponse> CreateCategoryAsync(CategoryUpdateInput input)
        {
            if (input == null)
            {
                return Fail("Please enter category name.");
            }

            try
            {
                var normalized = CategoryHelper.NormalizeInput(input);
                if (normalized.Name.Length == 0)
                {
                    return Fail("Please enter category name.");
                }

                if (await CategoryNameExistsForActiveAsync(normalized.Name, excludeCategoryId: 0))
                {
                    return Fail($"Category name \"{normalized.Name}\" already exists.");
                }

                var entity = new CategoryEntity
                {
                    Name = normalized.Name,
                    Description = normalized.Description,
                    IsActive = normalized.IsActive,
                    Cancelled = false
                };

                _dbContext.Categories.Add(entity);
                await _dbContext.SaveChangesAsync();

                return Ok(entity.IdCategory, "Category created successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while creating category: {ex.Message}");
            }
        }

        public async Task<CommonResponse> UpdateCategoryAsync(CategoryUpdateInput input)
        {
            if (input == null)
            {
                return Fail("Invalid request.");
            }

            try
            {
                var normalized = CategoryHelper.NormalizeInput(input);
                if (normalized.Name.Length == 0)
                {
                    return Fail("Please enter category name.");
                }

                if (input.CategoryID <= 0)
                {
                    return Fail("Invalid Category ID.");
                }

                var entity = await _dbContext.Categories
                    .FirstOrDefaultAsync(c => c.IdCategory == input.CategoryID);

                if (entity == null)
                {
                    return Fail("Invalid Category ID.");
                }

                if (entity.Cancelled)
                {
                    return Fail("This category is deleted and cannot be edited.");
                }

                if (await CategoryNameExistsForActiveAsync(normalized.Name, excludeCategoryId: input.CategoryID))
                {
                    return Fail($"Category name \"{normalized.Name}\" already exists.");
                }

                entity.Name = normalized.Name;
                entity.Description = normalized.Description;
                entity.IsActive = normalized.IsActive;

                await _dbContext.SaveChangesAsync();

                return Ok(input.CategoryID, "Category updated successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while updating category: {ex.Message}");
            }
        }

        public async Task<CommonResponse> DeleteCategoryAsync(CategoryDeleteInput input)
        {
            if (input == null)
            {
                return Fail("Invalid Category ID.");
            }

            try
            {
                var categoryId = input.CategoryID;
                if (categoryId <= 0)
                {
                    return Fail("Invalid Category ID.");
                }

                var entity = await _dbContext.Categories
                    .FirstOrDefaultAsync(c => c.IdCategory == categoryId);

                if (entity == null)
                {
                    return Fail("Invalid Category ID.");
                }

                if (entity.Cancelled)
                {
                    return Fail("This category is already deleted.");
                }

                if (await HasActiveSubCategoriesAsync(categoryId))
                {
                    return Fail("Cannot delete this category because active subcategories exist.");
                }

                entity.Cancelled = true;
                entity.CancelledOn = DateTime.Now;
                entity.CancelledReason = StringHelper.NormalizeOptionalString(input.CancelledReason);

                await _dbContext.SaveChangesAsync();

                return Ok(categoryId, "Category deleted successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while deleting category: {ex.Message}");
            }
        }

        private async Task<bool> CategoryNameExistsForActiveAsync(string trimmedName, int excludeCategoryId)
        {
            var key = trimmedName.ToLowerInvariant();
            return await _dbContext.Categories.AnyAsync(c =>
                !c.Cancelled &&
                c.IdCategory != excludeCategoryId &&
                (c.Name ?? string.Empty).ToLower() == key);
        }

        private Task<bool> HasActiveSubCategoriesAsync(int categoryId)
        {
            return _dbContext.SubCategories.AnyAsync(s =>
                s.FK_Category == categoryId &&
                s.Cancelled != true);
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
