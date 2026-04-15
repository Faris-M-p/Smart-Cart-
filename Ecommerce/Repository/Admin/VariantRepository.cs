using Ecommerce.DataAccess;
using Ecommerce.Helpers.Common;
using Ecommerce.Helpers.Variants;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.VariantModel;

namespace Ecommerce.Repository.Admin
{
    public class VariantRepository : IVariantInterface
    {
        private readonly EcommerceDbContext _dbContext;

        public VariantRepository(EcommerceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TableOutput<Variant>> GetVariantListAsync(VariantListInput input)
        {
            if (input == null)
            {
                return VariantHelper.EmptyTableOutput(null);
            }

            try
            {
                var normalized = VariantHelper.NormalizeInput(input);

                var filteredQuery = VariantHelper.ApplyFilters(
                    _dbContext.Variants.AsNoTracking(),
                    normalized);

                var totalCount = await filteredQuery.LongCountAsync();

                var sortedQuery = VariantHelper.ApplySorting(
                    filteredQuery,
                    normalized.SortColumn,
                    normalized.SortMode);

                var pagedEntityQuery = sortedQuery
                    .Skip((normalized.PageIndex - 1) * normalized.PageSize)
                    .Take(normalized.PageSize);

                var rows = await pagedEntityQuery
                    .Select(x => new Variant
                    {
                        VariantID = x.ID_Variant,
                        Name = x.Name,
                        Description = x.Description,
                        DisplayOrder = x.DisplayOrder,
                        IsActive = x.IsActive,
                        Cancelled = x.Cancelled
                    })
                    .ToListAsync();

                return new TableOutput<Variant>
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
                return VariantHelper.EmptyTableOutput(input);
            }
        }

        public async Task<Variant?> GetVariantByIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            return await _dbContext.Variants
                .AsNoTracking()
                .Where(v => v.ID_Variant == id)
                .Select(v => new Variant
                {
                    VariantID = v.ID_Variant,
                    Name = v.Name,
                    Description = v.Description,
                    DisplayOrder = v.DisplayOrder,
                    IsActive = v.IsActive,
                    Cancelled = v.Cancelled
                })
                .FirstOrDefaultAsync();
        }

        public async Task<CommonResponse> CreateVariantAsync(VariantUpdateInput input)
        {
            if (input == null)
            {
                return Fail("Invalid request.");
            }

            try
            {
                var normalized = VariantHelper.NormalizeInput(input);
                if (normalized.Name.Length == 0)
                {
                    return Fail("Please enter variant name.");
                }

                if (await VariantNameExistsForActiveAsync(normalized.Name, excludeVariantId: 0))
                {
                    return Fail($"Variant \"{normalized.Name}\" already exists.");
                }

                var entity = new VariantEntity
                {
                    Name = normalized.Name,
                    Description = normalized.Description,
                    DisplayOrder = normalized.DisplayOrder,
                    IsActive = normalized.IsActive,
                    Cancelled = false,
                    CancelledOn = null
                };

                _dbContext.Variants.Add(entity);
                await _dbContext.SaveChangesAsync();

                return Ok(entity.ID_Variant, "Variant created successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while creating variant: {ex.Message}");
            }
        }

        public async Task<CommonResponse> UpdateVariantAsync(VariantUpdateInput input)
        {
            if (input == null)
            {
                return Fail("Invalid request.");
            }

            try
            {
                var normalized = VariantHelper.NormalizeInput(input);
                if (normalized.Name.Length == 0)
                {
                    return Fail("Please enter variant name.");
                }

                if (input.VariantID <= 0)
                {
                    return Fail("Invalid variant ID.");
                }

                var entity = await _dbContext.Variants
                    .FirstOrDefaultAsync(v => v.ID_Variant == input.VariantID);

                if (entity == null)
                {
                    return Fail("Invalid variant ID.");
                }

                if (entity.Cancelled)
                {
                    return Fail("This variant is deleted and cannot be edited.");
                }

                if (await VariantNameExistsForActiveAsync(normalized.Name, excludeVariantId: input.VariantID))
                {
                    return Fail($"Variant \"{normalized.Name}\" already exists.");
                }

                entity.Name = normalized.Name;
                entity.Description = normalized.Description;
                entity.DisplayOrder = normalized.DisplayOrder;
                entity.IsActive = normalized.IsActive;

                await _dbContext.SaveChangesAsync();

                return Ok(input.VariantID, "Variant updated successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while updating variant: {ex.Message}");
            }
        }

        public async Task<CommonResponse> DeleteVariantAsync(VariantDeleteInput input)
        {
            if (input == null)
            {
                return Fail("Invalid variant ID.");
            }

            try
            {
                var variantId = input.VariantID;
                if (variantId <= 0)
                {
                    return Fail("Invalid variant ID.");
                }

                var entity = await _dbContext.Variants
                    .FirstOrDefaultAsync(v => v.ID_Variant == variantId);

                if (entity == null)
                {
                    return Fail("Invalid variant ID.");
                }

                if (entity.Cancelled)
                {
                    return Fail("This variant is already deleted.");
                }

                if (await HasActiveVariantValuesAsync(variantId))
                {
                    return Fail("Cannot delete variant while active variant values exist.");
                }

                entity.Cancelled = true;
                entity.CancelledOn = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                return Ok(variantId, "Variant deleted successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while deleting variant: {ex.Message}");
            }
        }

        private async Task<bool> VariantNameExistsForActiveAsync(string trimmedName, int excludeVariantId)
        {
            var key = trimmedName.ToLowerInvariant();
            return await _dbContext.Variants.AnyAsync(v =>
                !v.Cancelled &&
                v.ID_Variant != excludeVariantId &&
                v.Name.ToLower() == key);
        }

        private Task<bool> HasActiveVariantValuesAsync(int variantId)
        {
            return _dbContext.VariantValues.AnyAsync(vv =>
                vv.FK_Variant == variantId &&
                vv.Cancelled != true);
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
