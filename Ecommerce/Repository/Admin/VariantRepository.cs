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

        public async Task<TableOutput<VariantListOutput>> GetVariantListAsync(VariantListInput input)
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
                    .Select(v => new VariantListOutput
                    {
                        ID_Variant = v.IdVariant,
                        VariantName = v.VariantName,
                        Description = v.Description ?? string.Empty,
                        DisplayOrder = v.DisplayOrder,
                        CreatedOn = v.CreatedOn,
                        Cancelled = v.Cancelled,
                        CancelledOn = v.CancelledOn,
                        CancelledReason = v.CancelledReason ?? string.Empty
                    })
                    .ToListAsync();

                return new TableOutput<VariantListOutput>
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
                    VariantName = normalized.Name,
                    Description = normalized.Description,
                    DisplayOrder = normalized.DisplayOrder,
                    CreatedOn = DateTime.Now,
                    Cancelled = false,
                    CancelledOn = null,
                    CancelledReason = null
                };

                _dbContext.Variants.Add(entity);
                await _dbContext.SaveChangesAsync();

                return Ok(entity.IdVariant, "Variant created successfully.");
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

                if (input.ID_Variant <= 0)
                {
                    return Fail("Invalid variant ID.");
                }

                var entity = await _dbContext.Variants
                    .FirstOrDefaultAsync(v => v.IdVariant == input.ID_Variant);

                if (entity == null)
                {
                    return Fail("Invalid variant ID.");
                }

                if (entity.Cancelled)
                {
                    return Fail("This variant is deleted and cannot be edited.");
                }

                if (await VariantNameExistsForActiveAsync(normalized.Name, excludeVariantId: input.ID_Variant))
                {
                    return Fail($"Variant \"{normalized.Name}\" already exists.");
                }

                entity.VariantName = normalized.Name;
                entity.Description = normalized.Description;
                entity.DisplayOrder = normalized.DisplayOrder;

                await _dbContext.SaveChangesAsync();

                return Ok(input.ID_Variant, "Variant updated successfully.");
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
                var variantId = input.ID_Variant;
                if (variantId <= 0)
                {
                    return Fail("Invalid variant ID.");
                }

                var entity = await _dbContext.Variants
                    .FirstOrDefaultAsync(v => v.IdVariant == variantId);

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
                entity.CancelledReason = StringHelper.NormalizeOptionalString(input.CancelledReason);

                await _dbContext.SaveChangesAsync();

                return Ok(variantId, "Variant deleted successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while deleting variant: {ex.Message}");
            }
        }

        public async Task<VariantSelectByIdOutput?> GetVariantByIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            try
            {
                return await _dbContext.Variants.AsNoTracking()
                    .Where(v => v.IdVariant == id)
                    .Select(v => new VariantSelectByIdOutput
                    {
                        ID_Variant = v.IdVariant,
                        VariantName = v.VariantName,
                        Description = v.Description ?? string.Empty,
                        DisplayOrder = v.DisplayOrder,
                        CreatedOn = v.CreatedOn,
                        Cancelled = v.Cancelled,
                        CancelledOn = v.CancelledOn,
                        CancelledReason = v.CancelledReason ?? string.Empty
                    })
                    .FirstOrDefaultAsync();
            }
            catch
            {
                return null;
            }
        }

        private async Task<bool> VariantNameExistsForActiveAsync(string trimmedName, int excludeVariantId)
        {
            var key = trimmedName.ToLowerInvariant();
            return await _dbContext.Variants.AnyAsync(v =>
                !v.Cancelled &&
                v.IdVariant != excludeVariantId &&
                v.VariantName.ToLower() == key);
        }

        private Task<bool> HasActiveVariantValuesAsync(int variantId)
        {
            return _dbContext.VariantValues.AnyAsync(vv =>
                vv.FkVariant == variantId && !vv.Cancelled);
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
