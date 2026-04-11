using Ecommerce.DataAccess;
using Ecommerce.Helpers.Brands;
using Ecommerce.Helpers.Common;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static Ecommerce.Models.Admin.BrandModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Repository.Admin
{
    public class BrandRepository : IBrandInterface
    {
        private readonly EcommerceDbContext _dbContext;

        public BrandRepository(EcommerceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TableOutput<Brand>> GetBrandListAsync(BrandListInput input)
        {
            if (input == null)
            {
                return BrandHelper.EmptyTableOutput(null);
            }

            try
            {
                var normalized = BrandHelper.NormalizeInput(input);

                var filteredQuery = BrandHelper.ApplyFilters(
                    _dbContext.Brands.AsNoTracking(),
                    normalized);

                var totalCount = await filteredQuery.LongCountAsync();

                var sortedQuery = BrandHelper.ApplySorting(
                    filteredQuery,
                    normalized.SortColumn,
                    normalized.SortMode);

                var pagedEntityQuery = sortedQuery
                    .Skip((normalized.PageIndex - 1) * normalized.PageSize)
                    .Take(normalized.PageSize);

                var rows = await pagedEntityQuery
                    .Select(b => new Brand
                    {
                        BrandID = b.BrandId,
                        BrandName = b.BrandName,
                        Cancelled = b.Cancelled ?? false,
                        CancelledOn = b.CancelledOn,
                        CancelledReason = b.CancelledReason
                    })
                    .ToListAsync();

                return new TableOutput<Brand>
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
                return BrandHelper.EmptyTableOutput(input);
            }
        }

        public async Task<Brand> GetBrandByIdAsync(int id)
        {
            if (id <= 0)
            {
                return null!;
            }

            try
            {
                return await _dbContext.Brands
                    .AsNoTracking()
                    .Where(b => b.BrandId == id)
                    .Select(b => new Brand
                    {
                        BrandID = b.BrandId,
                        BrandName = b.BrandName,
                        Cancelled = b.Cancelled ?? false,
                        CancelledOn = b.CancelledOn,
                        CancelledReason = b.CancelledReason
                    })
                    .FirstOrDefaultAsync()!;
            }
            catch
            {
                return null!;
            }
        }

        public async Task<CommonResponse> CreateBrandAsync(BrandUpdateInput input)
        {
            if (input == null)
            {
                return Fail("Please enter brand name.");
            }

            try
            {
                var normalized = BrandHelper.NormalizeInput(input);
                if (normalized.Name.Length == 0)
                {
                    return Fail("Please enter brand name.");
                }

                if (await BrandNameExistsForActiveAsync(normalized.Name, excludeBrandId: 0))
                {
                    return Fail($"Brand name \"{normalized.Name}\" already exists.");
                }

                var entity = new BrandEntity
                {
                    BrandName = normalized.Name,
                    Cancelled = false,
                    CancelledOn = null,
                    CancelledReason = null
                };

                _dbContext.Brands.Add(entity);
                await _dbContext.SaveChangesAsync();

                return Ok(entity.BrandId, "Brand created successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while creating brand: {ex.Message}");
            }
        }

        public async Task<CommonResponse> UpdateBrandAsync(BrandUpdateInput input)
        {
            if (input == null)
            {
                return Fail("Invalid request.");
            }

            try
            {
                var normalized = BrandHelper.NormalizeInput(input);
                if (normalized.Name.Length == 0)
                {
                    return Fail("Please enter brand name.");
                }

                var entity = await _dbContext.Brands
                    .FirstOrDefaultAsync(b => b.BrandId == input.BrandID);

                if (entity == null)
                {
                    return Fail("Invalid Brand ID.");
                }

                if (entity.Cancelled == true)
                {
                    return Fail("This brand is deleted and cannot be edited.");
                }

                if (await BrandNameExistsForActiveAsync(normalized.Name, excludeBrandId: input.BrandID))
                {
                    return Fail($"Brand name \"{normalized.Name}\" already exists.");
                }

                entity.BrandName = normalized.Name;
                await _dbContext.SaveChangesAsync();

                return Ok(input.BrandID, "Brand updated successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while updating brand: {ex.Message}");
            }
        }

        public async Task<CommonResponse> DeleteBrandAsync(BrandDeleteInput input)
        {
            if (input == null)
            {
                return Fail("Invalid Brand ID.");
            }

            try
            {
                var brandId = input.BrandID;
                if (brandId <= 0)
                {
                    return Fail("Invalid Brand ID.");
                }

                var entity = await _dbContext.Brands
                    .FirstOrDefaultAsync(b => b.BrandId == brandId);

                if (entity == null)
                {
                    return Fail("Invalid Brand ID.");
                }

                if (entity.Cancelled == true)
                {
                    return Fail("This brand is already deleted.");
                }

                if (await HasActiveProductsForBrandAsync(brandId))
                {
                    return Fail("Cannot delete this brand because active products exist.");
                }

                entity.Cancelled = true;
                entity.CancelledOn = DateTime.Now;
                entity.CancelledReason = StringHelper.NormalizeOptionalString(input.CancelledReason);

                await _dbContext.SaveChangesAsync();

                return Ok(brandId, "Brand deleted successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while deleting brand: {ex.Message}");
            }
        }

        private async Task<bool> BrandNameExistsForActiveAsync(string trimmedName, int excludeBrandId)
        {
            var key = trimmedName.ToLowerInvariant();
            return await _dbContext.Brands.AnyAsync(b =>
                b.Cancelled != true &&
                b.BrandId != excludeBrandId &&
                b.BrandName.ToLower() == key);
        }

        private Task<bool> HasActiveProductsForBrandAsync(int brandId)
        {
            return _dbContext.Products.AnyAsync(p =>
                p.BrandId == brandId &&
                p.Cancelled != true);
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
