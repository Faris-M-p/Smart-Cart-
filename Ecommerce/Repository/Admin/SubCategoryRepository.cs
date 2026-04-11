using Ecommerce.DataAccess;
using Ecommerce.Helpers.Common;
using Ecommerce.Helpers.SubCategories;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static Ecommerce.Models.Admin.SubCategoryModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Repository.Admin
{
    public class SubCategoryRepository : ISubCategoryInterface
    {
        private readonly EcommerceDbContext _dbContext;

        public SubCategoryRepository(EcommerceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<SubCategory?> GetSubCategoryByIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            return await (
                from s in _dbContext.SubCategories.AsNoTracking()
                where s.ID_SubCategory == id
                join c in _dbContext.Categories.AsNoTracking() on s.FK_Category equals c.IdCategory into cg
                from c in cg.DefaultIfEmpty()
                select new SubCategory
                {
                    ID_SubCategory = s.ID_SubCategory,
                    SubCategoryName = s.Name,
                    FK_Category = s.FK_Category,
                    CategoryName = c != null ? c.Name : string.Empty,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    Cancelled = s.Cancelled ?? false,
                    CancelledOn = s.CancelledOn,
                    CancelledReason = s.CancelledReason
                }).FirstOrDefaultAsync();
        }

        public async Task<TableOutput<SubCategory>> GetSubCategoryListAsync(SubCategoryListInput input)
        {
            if (input == null)
            {
                return SubCategoryHelper.EmptyTableOutput(null);
            }

            try
            {
                var normalized = SubCategoryHelper.NormalizeInput(input);
                var filteredQuery = SubCategoryHelper.ApplyFilters(
                    _dbContext.SubCategories.AsNoTracking(),
                    normalized);

                var totalCount = await filteredQuery.LongCountAsync();

                var sortedQuery = SubCategoryHelper.ApplySorting(
                    filteredQuery,
                    normalized.SortColumn,
                    normalized.SortMode);

                var joinedQuery =
                    from s in sortedQuery
                    join c in _dbContext.Categories.AsNoTracking() on s.FK_Category equals c.IdCategory into cg
                    from c in cg.DefaultIfEmpty()
                    select new { s, c };

                var pagedQuery = joinedQuery
                    .Skip((normalized.PageIndex - 1) * normalized.PageSize)
                    .Take(normalized.PageSize);

                var rows = await pagedQuery
                    .Select(x => new SubCategory
                    {
                        ID_SubCategory = x.s.ID_SubCategory,
                        SubCategoryName = x.s.Name,
                        FK_Category = x.s.FK_Category,
                        CategoryName = x.c != null ? x.c.Name : string.Empty,
                        Description = x.s.Description,
                        IsActive = x.s.IsActive,
                        Cancelled = x.s.Cancelled ?? false,
                        CancelledOn = x.s.CancelledOn,
                        CancelledReason = x.s.CancelledReason
                    })
                    .ToListAsync();

                return new TableOutput<SubCategory>
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
                return SubCategoryHelper.EmptyTableOutput(input);
            }
        }

        public async Task<CommonResponse> CreateSubCategoryAsync(SubCategoryUpdateInput input)
        {
            if (input == null)
            {
                return Fail("Please enter SubCategory name.");
            }

            try
            {
                var normalized = SubCategoryHelper.NormalizeInput(input);
                if (normalized.Name.Length == 0)
                {
                    return Fail("Please enter SubCategory name.");
                }

                if (!await CategoryExistsActiveAsync(normalized.FK_Category))
                {
                    return Fail("Invalid or deleted Category.");
                }

                if (await SubCategoryNameExistsAsync(normalized.Name, normalized.FK_Category, excludeId: 0))
                {
                    return Fail($"SubCategory \"{normalized.Name}\" already exists in this category.");
                }

                var entity = new SubCategoryEntity
                {
                    Name = normalized.Name,
                    FK_Category = normalized.FK_Category,
                    Description = normalized.Description,
                    IsActive = normalized.IsActive,
                    Cancelled = false,
                    CancelledOn = null,
                    CancelledReason = null
                };

                _dbContext.SubCategories.Add(entity);
                await _dbContext.SaveChangesAsync();

                return Ok(entity.ID_SubCategory, "SubCategory created successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while creating subcategory: {ex.Message}");
            }
        }

        public async Task<CommonResponse> UpdateSubCategoryAsync(SubCategoryUpdateInput input)
        {
            if (input == null)
            {
                return Fail("Invalid request.");
            }

            try
            {
                var normalized = SubCategoryHelper.NormalizeInput(input);
                if (normalized.Name.Length == 0)
                {
                    return Fail("Please enter SubCategory name.");
                }

                if (input.SubCategoryID <= 0)
                {
                    return Fail("Invalid SubCategory ID.");
                }

                if (!await CategoryExistsActiveAsync(normalized.FK_Category))
                {
                    return Fail("Invalid or deleted Category.");
                }

                var entity = await _dbContext.SubCategories.FirstOrDefaultAsync(s => s.ID_SubCategory == input.SubCategoryID);
                if (entity == null)
                {
                    return Fail("Invalid SubCategory ID.");
                }

                if (entity.Cancelled == true)
                {
                    return Fail("This SubCategory is deleted and cannot be edited.");
                }

                if (await SubCategoryNameExistsAsync(normalized.Name, normalized.FK_Category, excludeId: input.SubCategoryID))
                {
                    return Fail($"SubCategory \"{normalized.Name}\" already exists in this category.");
                }

                entity.Name = normalized.Name;
                entity.FK_Category = normalized.FK_Category;
                entity.Description = normalized.Description;
                entity.IsActive = normalized.IsActive;

                await _dbContext.SaveChangesAsync();

                return Ok(input.SubCategoryID, "SubCategory updated successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while updating subcategory: {ex.Message}");
            }
        }

        public async Task<CommonResponse> DeleteSubCategoryAsync(SubCategoryDeleteInput input)
        {
            if (input == null)
            {
                return Fail("Invalid SubCategory ID.");
            }

            try
            {
                var id = input.SubCategoryID;
                if (id <= 0)
                {
                    return Fail("Invalid SubCategory ID.");
                }

                var entity = await _dbContext.SubCategories.FirstOrDefaultAsync(s => s.ID_SubCategory == id);
                if (entity == null)
                {
                    return Fail("Invalid SubCategory ID.");
                }

                if (entity.Cancelled == true)
                {
                    return Fail("This subcategory is already deleted.");
                }

                if (await HasActiveProductsForSubCategoryAsync(id))
                {
                    return Fail("Cannot delete subcategory while products are assigned to it.");
                }

                entity.Cancelled = true;
                entity.CancelledOn = DateTime.Now;
                entity.CancelledReason = StringHelper.NormalizeOptionalString(input.CancelledReason);

                await _dbContext.SaveChangesAsync();

                return Ok(id, "SubCategory deleted successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while deleting subcategory: {ex.Message}");
            }
        }

        private Task<bool> CategoryExistsActiveAsync(int categoryId) =>
            _dbContext.Categories.AnyAsync(c =>
                c.IdCategory == categoryId && !c.Cancelled && c.IsActive);

        private Task<bool> HasActiveProductsForSubCategoryAsync(int subCategoryId) =>
            _dbContext.Products.AnyAsync(p =>
                p.SubCategoryId == subCategoryId && p.Cancelled != true);

        private async Task<bool> SubCategoryNameExistsAsync(string trimmedName, int categoryId, int excludeId)
        {
            var key = trimmedName.ToLowerInvariant();
            return await _dbContext.SubCategories.AnyAsync(s =>
                s.Cancelled != true &&
                s.FK_Category == categoryId &&
                s.ID_SubCategory != excludeId &&
                (s.Name ?? string.Empty).ToLower() == key);
        }

        private static CommonResponse Ok(long code, string msg) =>
            new() { ResponseCode = code, StatusCode = true, ResponseMsg = msg };

        private static CommonResponse Fail(string msg) =>
            new() { ResponseCode = -1, StatusCode = false, ResponseMsg = msg };
    }
}
