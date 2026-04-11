using Ecommerce.DataAccess;
using Ecommerce.Helpers.Common;
using Ecommerce.Helpers.Suppliers;
using Ecommerce.Interface.Admin;
using Ecommerce.Models.Entities;
using Microsoft.EntityFrameworkCore;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.SupplierModel;

namespace Ecommerce.Repository.Admin
{
    public class SupplierRepository : ISupplierInterface
    {
        private readonly EcommerceDbContext _dbContext;

        public SupplierRepository(EcommerceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TableOutput<Supplier>> GetSupplierListAsync(SupplierListInput input)
        {
            if (input == null)
            {
                return SupplierHelper.EmptyTableOutput(null);
            }

            try
            {
                var normalized = SupplierHelper.NormalizeInput(input);
                var filteredQuery = SupplierHelper.ApplyFilters(
                    _dbContext.Suppliers.AsNoTracking(),
                    normalized);

                var totalCount = await filteredQuery.LongCountAsync();

                var sortedQuery = SupplierHelper.ApplySorting(
                    filteredQuery,
                    normalized.SortColumn,
                    normalized.SortMode);

                var pagedEntityQuery = sortedQuery
                    .Skip((normalized.PageIndex - 1) * normalized.PageSize)
                    .Take(normalized.PageSize);

                var rows = await pagedEntityQuery
                    .Select(s => new Supplier
                    {
                        ID_Supplier = s.SupplierId,
                        SupplierName = s.SupplierName,
                        ContactPerson = null,
                        Phone = s.ContactPhone,
                        Email = s.ContactEmail,
                        GSTNumber = null,
                        Address = s.Address,
                        CreatedOn = s.CreatedAt,
                        Cancelled = s.Cancelled ?? false,
                        CancelledOn = s.CancelledOn,
                        CancelledReason = s.CancelledReason
                    })
                    .ToListAsync();

                return new TableOutput<Supplier>
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
                return SupplierHelper.EmptyTableOutput(input);
            }
        }

        public async Task<Supplier?> GetSupplierByIdAsync(long id)
        {
            if (id <= 0)
            {
                return null;
            }

            try
            {
                return await _dbContext.Suppliers.AsNoTracking()
                    .Where(s => s.SupplierId == id)
                    .Select(s => new Supplier
                    {
                        ID_Supplier = s.SupplierId,
                        SupplierName = s.SupplierName,
                        ContactPerson = null,
                        Phone = s.ContactPhone,
                        Email = s.ContactEmail,
                        GSTNumber = null,
                        Address = s.Address,
                        CreatedOn = s.CreatedAt,
                        Cancelled = s.Cancelled ?? false,
                        CancelledOn = s.CancelledOn,
                        CancelledReason = s.CancelledReason
                    })
                    .FirstOrDefaultAsync();
            }
            catch
            {
                return null;
            }
        }

        public async Task<CommonResponse> CreateSupplierAsync(SupplierUpdateInput input)
        {
            if (input == null)
            {
                return Fail("Invalid request.");
            }

            try
            {
                var normalized = SupplierHelper.NormalizeInput(input);
                if (normalized.Name.Length == 0)
                {
                    return Fail("Please enter supplier name.");
                }

                if (await SupplierNameExistsAsync(normalized.Name, excludeId: 0))
                {
                    return Fail($"Supplier \"{normalized.Name}\" already exists.");
                }

                var entity = new SupplierEntity
                {
                    SupplierName = normalized.Name,
                    ContactEmail = normalized.Email,
                    ContactPhone = normalized.Phone,
                    Address = normalized.Address,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = null,
                    Cancelled = false,
                    CancelledOn = null,
                    CancelledReason = null
                };

                _dbContext.Suppliers.Add(entity);
                await _dbContext.SaveChangesAsync();

                return Ok(entity.SupplierId, "Supplier created successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while creating supplier: {ex.Message}");
            }
        }

        public async Task<CommonResponse> UpdateSupplierAsync(SupplierUpdateInput input)
        {
            if (input == null)
            {
                return Fail("Invalid request.");
            }

            try
            {
                var id = (int)input.ID_Supplier;
                if (id <= 0)
                {
                    return Fail("Invalid supplier ID.");
                }

                var normalized = SupplierHelper.NormalizeInput(input);
                if (normalized.Name.Length == 0)
                {
                    return Fail("Please enter supplier name.");
                }

                var entity = await _dbContext.Suppliers.FirstOrDefaultAsync(s => s.SupplierId == id);
                if (entity == null)
                {
                    return Fail("Invalid supplier ID.");
                }

                if (entity.Cancelled == true)
                {
                    return Fail("This supplier is deleted and cannot be edited.");
                }

                if (await SupplierNameExistsAsync(normalized.Name, excludeId: id))
                {
                    return Fail($"Supplier \"{normalized.Name}\" already exists.");
                }

                entity.SupplierName = normalized.Name;
                entity.ContactEmail = normalized.Email;
                entity.ContactPhone = normalized.Phone;
                entity.Address = normalized.Address;
                entity.UpdatedAt = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                return Ok(id, "Supplier updated successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while updating supplier: {ex.Message}");
            }
        }

        public async Task<CommonResponse> DeleteSupplierAsync(SupplierUpdateInput input)
        {
            if (input == null)
            {
                return Fail("Invalid supplier ID.");
            }

            try
            {
                var id = (int)input.ID_Supplier;
                if (id <= 0)
                {
                    return Fail("Invalid supplier ID.");
                }

                var entity = await _dbContext.Suppliers.FirstOrDefaultAsync(s => s.SupplierId == id);
                if (entity == null)
                {
                    return Fail("Invalid supplier ID.");
                }

                if (entity.Cancelled == true)
                {
                    return Fail("This supplier is already deleted.");
                }

                entity.Cancelled = true;
                entity.CancelledOn = DateTime.Now;
                entity.CancelledReason = StringHelper.NormalizeOptionalString(input.CancelledReason);
                entity.UpdatedAt = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                return Ok(id, "Supplier deleted successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while deleting supplier: {ex.Message}");
            }
        }

        private async Task<bool> SupplierNameExistsAsync(string trimmedName, int excludeId)
        {
            var key = trimmedName.ToLowerInvariant();
            return await _dbContext.Suppliers.AnyAsync(s =>
                s.Cancelled != true &&
                s.SupplierId != excludeId &&
                s.SupplierName.ToLower() == key);
        }

        private static CommonResponse Ok(long code, string msg) =>
            new() { ResponseCode = code, StatusCode = true, ResponseMsg = msg };

        private static CommonResponse Fail(string msg) =>
            new() { ResponseCode = -1, StatusCode = false, ResponseMsg = msg };
    }
}
