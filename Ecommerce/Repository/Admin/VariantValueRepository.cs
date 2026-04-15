using Ecommerce.DataAccess;
using Ecommerce.Helpers.Common;
using Ecommerce.Helpers.VariantValues;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.VariantValueModel;

namespace Ecommerce.Repository.Admin
{
    public class VariantValueRepository : IVariantValueInterface
    {
        private readonly EcommerceDbContext _dbContext;

        public VariantValueRepository(EcommerceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TableOutput<VariantValue>> GetVariantValueListAsync(VariantValueListInput input)
        {
            if (input == null)
            {
                return VariantValueHelper.EmptyTableOutput(null);
            }

            try
            {
                var normalized = VariantValueHelper.NormalizeInput(input);
                var filteredQuery = VariantValueHelper.ApplyFilters(
                    _dbContext.VariantValues.AsNoTracking(),
                    normalized);

                var totalCount = await filteredQuery.LongCountAsync();

                var sortedQuery = VariantValueHelper.ApplySorting(
                    filteredQuery,
                    normalized.SortColumn,
                    normalized.SortMode);

                var pagedEntityQuery = sortedQuery
                    .Skip((normalized.PageIndex - 1) * normalized.PageSize)
                    .Take(normalized.PageSize);

                var rows = await pagedEntityQuery
                    .Select(vv => new VariantValue
                    {
                        VariantValueID = vv.ID_VariantValue,
                        FK_Variant = vv.FK_Variant,
                        Name = vv.Name,
                        Description = vv.Description,
                        DisplayOrder = vv.DisplayOrder
                    })
                    .ToListAsync();

                return new TableOutput<VariantValue>
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
                return VariantValueHelper.EmptyTableOutput(input);
            }
        }

        public async Task<List<VariantValue>> GetByVariantIdAsync(int variantId)
        {
            if (variantId <= 0)
            {
                return new List<VariantValue>();
            }

            return await _dbContext.VariantValues
                .AsNoTracking()
                .Where(vv => vv.FK_Variant == variantId && vv.Cancelled != true)
                .OrderBy(vv => vv.DisplayOrder)
                .ThenBy(vv => vv.Name)
                .Select(vv => new VariantValue
                {
                    VariantValueID = vv.ID_VariantValue,
                    FK_Variant = vv.FK_Variant,
                    Name = vv.Name,
                    Description = vv.Description,
                    DisplayOrder = vv.DisplayOrder
                })
                .ToListAsync();
        }

        public async Task<VariantValue?> GetVariantValueByIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            return await (
                from vv in _dbContext.VariantValues.AsNoTracking()
                join v in _dbContext.Variants.AsNoTracking() on vv.FK_Variant equals v.ID_Variant
                where vv.ID_VariantValue == id
                select new VariantValue
                {
                    VariantValueID = vv.ID_VariantValue,
                    FK_Variant = vv.FK_Variant,
                    Name = vv.Name,
                    Description = vv.Description,
                    DisplayOrder = vv.DisplayOrder,
                    VariantName = v.Name
                }).FirstOrDefaultAsync();
        }

        public async Task<CommonResponse> CreateVariantValueAsync(VariantValueCreateInput input)
        {
            if (input == null)
            {
                return Fail("Invalid request.");
            }

            try
            {
                var n = VariantValueHelper.NormalizeCreateInput(input);
                if (n.Name.Length == 0)
                {
                    return Fail("Please enter name.");
                }

                if (n.FkVariant <= 0)
                {
                    return Fail("Invalid variant.");
                }

                if (!await VariantExistsActiveAsync(n.FkVariant))
                {
                    return Fail("Invalid or deleted variant.");
                }

                if (await NameExistsInVariantAsync(n.FkVariant, n.Name, excludeId: 0))
                {
                    return Fail($"Value \"{n.Name}\" already exists for this variant.");
                }

                var entity = new VariantValueEntity
                {
                    FK_Variant = n.FkVariant,
                    Name = n.Name,
                    Description = n.Description,
                    DisplayOrder = n.DisplayOrder,
                    Cancelled = false,
                    CancelledOn = null
                };

                _dbContext.VariantValues.Add(entity);
                await _dbContext.SaveChangesAsync();

                return Ok(entity.ID_VariantValue, "Variant value created successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while creating variant value: {ex.Message}");
            }
        }

        public async Task<CommonResponse> UpdateVariantValueAsync(VariantValueUpdateInput input)
        {
            if (input == null)
            {
                return Fail("Invalid request.");
            }

            try
            {
                if (input.VariantValueID <= 0)
                {
                    return Fail("Invalid variant value ID.");
                }

                var n = VariantValueHelper.NormalizeUpdateInput(input);
                if (n.Name.Length == 0)
                {
                    return Fail("Please enter name.");
                }

                var entity = await _dbContext.VariantValues.FirstOrDefaultAsync(vv => vv.ID_VariantValue == n.Id);
                if (entity == null)
                {
                    return Fail("Invalid variant value ID.");
                }

                if (entity.Cancelled)
                {
                    return Fail("This variant value is deleted and cannot be edited.");
                }

                if (await NameExistsInVariantAsync(entity.FK_Variant, n.Name, excludeId: n.Id))
                {
                    return Fail($"Value \"{n.Name}\" already exists for this variant.");
                }

                entity.Name = n.Name;
                entity.Description = n.Description;
                entity.DisplayOrder = n.DisplayOrder;

                await _dbContext.SaveChangesAsync();

                return Ok(n.Id, "Variant value updated successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while updating variant value: {ex.Message}");
            }
        }

        public async Task<CommonResponse> DeleteVariantValueAsync(VariantValueDeleteInput input)
        {
            if (input == null)
            {
                return Fail("Invalid variant value ID.");
            }

            try
            {
                if (input.VariantValueID <= 0)
                {
                    return Fail("Invalid variant value ID.");
                }

                var entity = await _dbContext.VariantValues.FirstOrDefaultAsync(vv => vv.ID_VariantValue == input.VariantValueID);
                if (entity == null)
                {
                    return Fail("Invalid variant value ID.");
                }

                if (entity.Cancelled)
                {
                    return Fail("This variant value is already deleted.");
                }

                var inUse = await _dbContext.ProductVariantAttributes.AsNoTracking()
                    .AnyAsync(pva => pva.FK_VariantValue == input.VariantValueID);

                if (inUse)
                {
                    return Fail("Cannot delete variant value while it is used by a product variant.");
                }

                entity.Cancelled = true;
                entity.CancelledOn = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                return Ok(input.VariantValueID, "Variant value deleted successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while deleting variant value: {ex.Message}");
            }
        }

        private Task<bool> VariantExistsActiveAsync(int variantId) =>
            _dbContext.Variants.AnyAsync(v => v.ID_Variant == variantId && v.Cancelled != true);

        private async Task<bool> NameExistsInVariantAsync(int fkVariant, string trimmedName, int excludeId)
        {
            var key = trimmedName.ToLowerInvariant();
            return await _dbContext.VariantValues.AnyAsync(vv =>
                vv.FK_Variant == fkVariant &&
                vv.Cancelled != true &&
                vv.ID_VariantValue != excludeId &&
                vv.Name.ToLower() == key);
        }

        private static CommonResponse Ok(long code, string msg) =>
            new() { ResponseCode = code, StatusCode = true, ResponseMsg = msg };

        private static CommonResponse Fail(string msg) =>
            new() { ResponseCode = -1, StatusCode = false, ResponseMsg = msg };
    }
}
