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
                    .Select(x => new Supplier
                    {
                        SupplierID = x.ID_Supplier,
                        Name = x.Name,
                        CompanyName = x.CompanyName,
                        Email = x.Email,
                        Phone = x.Phone,
                        State = x.State,
                        District = x.District,
                        City = x.City,
                        Address = x.Address,
                        Pincode = x.Pincode,
                        Description = x.Description,
                        IsActive = x.IsActive,
                        Cancelled = x.Cancelled
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

        public async Task<Supplier?> GetSupplierByIdAsync(int id)
        {
            if (id <= 0)
            {
                return null;
            }

            return await _dbContext.Suppliers
                .AsNoTracking()
                .Where(s => s.ID_Supplier == id)
                .Select(s => new Supplier
                {
                    SupplierID = s.ID_Supplier,
                    Name = s.Name,
                    CompanyName = s.CompanyName,
                    Email = s.Email,
                    Phone = s.Phone,
                    State = s.State,
                    District = s.District,
                    City = s.City,
                    Address = s.Address,
                    Pincode = s.Pincode,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    Cancelled = s.Cancelled
                })
                .FirstOrDefaultAsync();
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

                if (normalized.State.Length == 0 || normalized.District.Length == 0 || normalized.City.Length == 0)
                {
                    return Fail("Please select state, district, and city.");
                }

                if (await SupplierEmailExistsAsync(normalized.Email, excludeSupplierId: 0))
                {
                    return Fail("A supplier with this email already exists.");
                }

                var entity = new SupplierEntity
                {
                    Name = normalized.Name,
                    CompanyName = normalized.CompanyName,
                    Email = normalized.Email,
                    Phone = normalized.Phone,
                    State = normalized.State,
                    District = normalized.District,
                    City = normalized.City,
                    Address = normalized.Address,
                    Pincode = normalized.Pincode,
                    Description = normalized.Description,
                    IsActive = normalized.IsActive,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = null,
                    Cancelled = false,
                    CancelledOn = null,
                    CancelledReason = null
                };

                _dbContext.Suppliers.Add(entity);
                await _dbContext.SaveChangesAsync();

                return Ok(entity.ID_Supplier, "Supplier created successfully.");
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
                var id = input.SupplierId;
                if (id <= 0)
                {
                    return Fail("Invalid supplier ID.");
                }

                var normalized = SupplierHelper.NormalizeInput(input);
                if (normalized.Name.Length == 0)
                {
                    return Fail("Please enter supplier name.");
                }

                if (normalized.State.Length == 0 || normalized.District.Length == 0 || normalized.City.Length == 0)
                {
                    return Fail("Please select state, district, and city.");
                }

                var entity = await _dbContext.Suppliers.FirstOrDefaultAsync(s => s.ID_Supplier == id);
                if (entity == null)
                {
                    return Fail("Invalid supplier ID.");
                }

                if (entity.Cancelled)
                {
                    return Fail("This supplier is deleted and cannot be edited.");
                }

                if (await SupplierEmailExistsAsync(normalized.Email, excludeSupplierId: id))
                {
                    return Fail("A supplier with this email already exists.");
                }

                entity.Name = normalized.Name;
                entity.CompanyName = normalized.CompanyName;
                entity.Email = normalized.Email;
                entity.Phone = normalized.Phone;
                entity.State = normalized.State;
                entity.District = normalized.District;
                entity.City = normalized.City;
                entity.Address = normalized.Address;
                entity.Pincode = normalized.Pincode;
                entity.Description = normalized.Description;
                entity.IsActive = normalized.IsActive;
                entity.UpdatedAt = DateTime.Now;

                await _dbContext.SaveChangesAsync();

                return Ok(id, "Supplier updated successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while updating supplier: {ex.Message}");
            }
        }

        public async Task<CommonResponse> DeleteSupplierAsync(SupplierDeleteInput input)
        {
            if (input == null)
            {
                return Fail("Invalid supplier ID.");
            }

            try
            {
                var id = input.SupplierId;
                if (id <= 0)
                {
                    return Fail("Invalid supplier ID.");
                }

                var entity = await _dbContext.Suppliers.FirstOrDefaultAsync(s => s.ID_Supplier == id);
                if (entity == null)
                {
                    return Fail("Invalid supplier ID.");
                }

                if (entity.Cancelled)
                {
                    return Fail("This supplier is already deleted.");
                }

                entity.Cancelled = true;
                entity.CancelledOn = DateTime.Now;
                entity.CancelledReason = StringHelper.NormalizeOptionalString(input.CancelledReason);

                await _dbContext.SaveChangesAsync();

                return Ok(id, "Supplier deleted successfully.");
            }
            catch (Exception ex)
            {
                return Fail($"An error occurred while deleting supplier: {ex.Message}");
            }
        }

        private async Task<bool> SupplierEmailExistsAsync(string? email, int excludeSupplierId)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            var key = email.Trim().ToLowerInvariant();
            return await _dbContext.Suppliers.AnyAsync(s =>
                !s.Cancelled &&
                s.ID_Supplier != excludeSupplierId &&
                s.Email != null &&
                s.Email.ToLower() == key);
        }

        private static CommonResponse Ok(long code, string msg) =>
            new() { ResponseCode = code, StatusCode = true, ResponseMsg = msg };

        private static CommonResponse Fail(string msg) =>
            new() { ResponseCode = -1, StatusCode = false, ResponseMsg = msg };
    }
}
