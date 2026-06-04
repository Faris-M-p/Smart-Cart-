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
                        BrandID = b.ID_Brand,
                        BrandName = b.BrandName,
                        Description = b.Description,
                        IsActive = b.IsActive,
                        Cancelled = b.Cancelled,
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
                throw;
            }
        }

        public async Task<Brand?> GetBrandByIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            return await _dbContext.Brands
                .AsNoTracking()
                .Where(b => b.ID_Brand == id)
                .Select(b => new Brand
                {
                    BrandID = b.ID_Brand,
                    BrandName = b.BrandName,
                    Description = b.Description,
                    IsActive = b.IsActive,
                    Cancelled = b.Cancelled,
                    CancelledOn = b.CancelledOn,
                    CancelledReason = b.CancelledReason
                })
                .FirstOrDefaultAsync();
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
                    Description = normalized.Description,
                    IsActive = normalized.IsActive,
                    Cancelled = false
                };

                _dbContext.Brands.Add(entity);
                await _dbContext.SaveChangesAsync();

                return Ok(entity.ID_Brand, "Brand created successfully.");
            }
            catch
            {
                throw;
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

                if (input.BrandID <= 0)
                {
                    return Fail("Invalid Brand ID.");
                }

                var entity = await _dbContext.Brands
                    .FirstOrDefaultAsync(b => b.ID_Brand == input.BrandID);

                if (entity == null)
                {
                    return Fail("Invalid Brand ID.");
                }

                if (entity.Cancelled)
                {
                    return Fail("This brand is deleted and cannot be edited.");
                }

                if (await BrandNameExistsForActiveAsync(normalized.Name, excludeBrandId: input.BrandID))
                {
                    return Fail($"Brand name \"{normalized.Name}\" already exists.");
                }

                entity.BrandName = normalized.Name;
                entity.Description = normalized.Description;
                entity.IsActive = normalized.IsActive;

                await _dbContext.SaveChangesAsync();

                return Ok(input.BrandID, "Brand updated successfully.");
            }
            catch
            {
                throw;
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
                    .FirstOrDefaultAsync(b => b.ID_Brand == brandId);

                if (entity == null)
                {
                    return Fail("Invalid Brand ID.");
                }

                if (entity.Cancelled)
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
            catch
            {
                throw;
            }
        }

        private async Task<bool> BrandNameExistsForActiveAsync(string trimmedName, int excludeBrandId)
        {
            var key = trimmedName.ToLowerInvariant();
            return await _dbContext.Brands.AnyAsync(b =>
                !b.Cancelled &&
                b.ID_Brand != excludeBrandId &&
                (b.BrandName ?? string.Empty).ToLower() == key);
        }

        private Task<bool> HasActiveProductsForBrandAsync(int brandId)
        {
            return _dbContext.Products.AnyAsync(p =>
                p.FK_Brand == brandId &&
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
