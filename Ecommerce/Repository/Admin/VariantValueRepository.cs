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

        public async Task<TableOutput<VariantValueListOutput>> GetVariantValueListAsync(VariantValueListInput input)
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
                    .Select(vv => new VariantValueListOutput
                    {
                        ID_VariantValue = vv.IdVariantValue,
                        FK_Variant = vv.FkVariant,
                        ValueName = vv.ValueName,
                        Description = vv.Description ?? string.Empty,
                        ValueIcon = vv.ValueIcon ?? string.Empty,
                        DisplayOrder = vv.DisplayOrder,
                        CreatedOn = vv.CreatedOn,
                        Cancelled = vv.Cancelled,
                        CancelledOn = vv.CancelledOn,
                        CancelledReason = vv.CancelledReason ?? string.Empty
                    })
                    .ToListAsync();

                return new TableOutput<VariantValueListOutput>
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

        public async Task<CommonResponse> UpdateVariantValueAsync(VariantValueUpdateInput input)
        {
            if (input == null)
            {
                return Fail("Invalid request.");
            }

            try
            {
                return input.UserAction switch
                {
                    1 => await InsertVariantValueAsync(input),
                    2 => await SaveVariantValueAsync(input),
                    3 => await SoftDeleteVariantValueAsync(input),
                    _ => Fail("Invalid user action.")
                };
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while saving variant value: {ex.Message}");
            }
        }

        public async Task<VariantValueSelectByIdOutput?> GetVariantValueByIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            try
            {
                return await (
                    from vv in _dbContext.VariantValues.AsNoTracking()
                    join v in _dbContext.Variants.AsNoTracking() on vv.FkVariant equals v.IdVariant
                    where vv.IdVariantValue == id
                    select new VariantValueSelectByIdOutput
                    {
                        ID_VariantValue = vv.IdVariantValue,
                        FK_Variant = vv.FkVariant,
                        ValueName = vv.ValueName,
                        Description = vv.Description ?? string.Empty,
                        ValueIcon = vv.ValueIcon ?? string.Empty,
                        DisplayOrder = vv.DisplayOrder,
                        CreatedOn = vv.CreatedOn,
                        Cancelled = vv.Cancelled,
                        CancelledOn = vv.CancelledOn,
                        CancelledReason = vv.CancelledReason ?? string.Empty,
                        VariantName = v.VariantName
                    }).FirstOrDefaultAsync();
            }
            catch
            {
                return null;
            }
        }

        private async Task<CommonResponse> InsertVariantValueAsync(VariantValueUpdateInput input)
        {
            var n = VariantValueHelper.NormalizeInput(input);
            if (n.Name.Length == 0)
            {
                return Fail("Please enter value name.");
            }

            if (!await VariantExistsActiveAsync(n.FkVariant))
            {
                return Fail("Invalid or deleted variant.");
            }

            if (await ValueNameExistsInVariantAsync(n.FkVariant, n.Name, excludeId: 0))
            {
                return Fail($"Value \"{n.Name}\" already exists for this variant.");
            }

            var entity = new VariantValueEntity
            {
                FkVariant = n.FkVariant,
                ValueName = n.Name,
                Description = n.Description,
                ValueIcon = n.ValueIcon,
                DisplayOrder = n.DisplayOrder,
                CreatedOn = DateTime.Now,
                Cancelled = false,
                CancelledOn = null,
                CancelledReason = null
            };

            _dbContext.VariantValues.Add(entity);
            await _dbContext.SaveChangesAsync();

            return Ok(entity.IdVariantValue, "Variant value created successfully.");
        }

        private async Task<CommonResponse> SaveVariantValueAsync(VariantValueUpdateInput input)
        {
            if (input.ID_VariantValue <= 0)
            {
                return Fail("Invalid variant value ID.");
            }

            var n = VariantValueHelper.NormalizeInput(input);
            if (n.Name.Length == 0)
            {
                return Fail("Please enter value name.");
            }

            if (!await VariantExistsActiveAsync(n.FkVariant))
            {
                return Fail("Invalid or deleted variant.");
            }

            var entity = await _dbContext.VariantValues.FirstOrDefaultAsync(vv => vv.IdVariantValue == input.ID_VariantValue);
            if (entity == null)
            {
                return Fail("Invalid variant value ID.");
            }

            if (entity.Cancelled)
            {
                return Fail("This variant value is deleted and cannot be edited.");
            }

            if (await ValueNameExistsInVariantAsync(n.FkVariant, n.Name, excludeId: input.ID_VariantValue))
            {
                return Fail($"Value \"{n.Name}\" already exists for this variant.");
            }

            entity.FkVariant = n.FkVariant;
            entity.ValueName = n.Name;
            entity.Description = n.Description;
            entity.ValueIcon = n.ValueIcon;
            entity.DisplayOrder = n.DisplayOrder;

            await _dbContext.SaveChangesAsync();

            return Ok(input.ID_VariantValue, "Variant value updated successfully.");
        }

        private async Task<CommonResponse> SoftDeleteVariantValueAsync(VariantValueUpdateInput input)
        {
            if (input.ID_VariantValue <= 0)
            {
                return Fail("Invalid variant value ID.");
            }

            var entity = await _dbContext.VariantValues.FirstOrDefaultAsync(vv => vv.IdVariantValue == input.ID_VariantValue);
            if (entity == null)
            {
                return Fail("Invalid variant value ID.");
            }

            if (entity.Cancelled)
            {
                return Fail("This variant value is already deleted.");
            }

            var inUse = await _dbContext.ProductVariantAttributes.AsNoTracking()
                .AnyAsync(pva => pva.FkVariantValue == input.ID_VariantValue);

            if (inUse)
            {
                return Fail("Cannot delete variant value while it is used by a product variant.");
            }

            entity.Cancelled = true;
            entity.CancelledOn = DateTime.Now;
            entity.CancelledReason = StringHelper.NormalizeOptionalString(input.CancelledReason);

            await _dbContext.SaveChangesAsync();

            return Ok(input.ID_VariantValue, "Variant value deleted successfully.");
        }

        private Task<bool> VariantExistsActiveAsync(int variantId) =>
            _dbContext.Variants.AnyAsync(v => v.IdVariant == variantId && !v.Cancelled);

        private async Task<bool> ValueNameExistsInVariantAsync(int fkVariant, string trimmedName, int excludeId)
        {
            var key = trimmedName.ToLowerInvariant();
            return await _dbContext.VariantValues.AnyAsync(vv =>
                vv.FkVariant == fkVariant &&
                !vv.Cancelled &&
                vv.IdVariantValue != excludeId &&
                vv.ValueName.ToLower() == key);
        }

        private static CommonResponse Ok(long code, string msg) =>
            new() { ResponseCode = code, StatusCode = true, ResponseMsg = msg };

        private static CommonResponse Fail(string msg) =>
            new() { ResponseCode = -1, StatusCode = false, ResponseMsg = msg };
    }
}
