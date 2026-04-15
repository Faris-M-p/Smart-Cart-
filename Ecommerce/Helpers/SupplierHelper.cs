using Ecommerce.Helpers.Common;
using Ecommerce.Models;
using Ecommerce.Models.Entities;
using Ecommerce.Models.Enums;
using static Ecommerce.Models.Admin.SupplierModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Helpers.Suppliers
{
    public sealed record NormalizedSupplierListInput(
        string? SearchLower,
        IReadOnlyList<int> FilterIds,
        int PageIndex,
        int PageSize,
        int SortColumn,
        string SortMode);

    public sealed record NormalizedSupplierWriteInput(
        string Name,
        string? CompanyName,
        string? Email,
        string? Phone,
        string State,
        string District,
        string City,
        string? Address,
        string? Pincode,
        string? Description,
        bool IsActive);

    public static class SupplierHelper
    {
        public static NormalizedSupplierListInput NormalizeInput(SupplierListInput input)
        {
            var pageIndex = Math.Max(1, input.PageIndex);
            var pageSize = Math.Max(1, input.PageSize);

            var rawSearch = input.SearchText?.Trim() ?? string.Empty;
            string? searchLower = null;
            if (rawSearch.Length >= 1)
            {
                searchLower = rawSearch.ToLowerInvariant();
            }

            var filterIds = StringHelper.ParseFilterIds(input.FilterSupplierIDs);
            var sortColumn = input.SortColumn;
            var sortMode = input.SortMode?.Trim() ?? string.Empty;

            return new NormalizedSupplierListInput(searchLower, filterIds, pageIndex, pageSize, sortColumn, sortMode);
        }

        public static NormalizedSupplierWriteInput NormalizeInput(SupplierUpdateInput input)
        {
            var name = (input.SupplierName ?? string.Empty).Trim();
            var companyName = StringHelper.NormalizeOptionalString(input.CompanyName);
            var email = StringHelper.NormalizeOptionalString(input.Email);
            var phone = StringHelper.TruncateOptional(input.Phone, 20);
            var state = (input.State ?? string.Empty).Trim();
            var district = (input.District ?? string.Empty).Trim();
            var city = (input.City ?? string.Empty).Trim();
            var address = StringHelper.NormalizeOptionalString(input.Address);
            var pincode = StringHelper.TruncateOptional(input.Pincode, 10);
            var description = StringHelper.NormalizeOptionalString(input.Description);

            return new NormalizedSupplierWriteInput(
                name,
                companyName,
                email,
                phone,
                state,
                district,
                city,
                address,
                pincode,
                description,
                input.IsActive);
        }

        public static IQueryable<SupplierEntity> ApplyFilters(
            IQueryable<SupplierEntity> query,
            NormalizedSupplierListInput normalized)
        {
            query = query.Where(x => x.Cancelled != true);

            if (normalized.SearchLower != null)
            {
                var s = normalized.SearchLower;
                query = query.Where(x =>
                    x.Name.ToLower().Contains(s) ||
                    (x.CompanyName != null && x.CompanyName.ToLower().Contains(s)));
            }

            if (normalized.FilterIds.Count > 0)
            {
                query = query.Where(x => normalized.FilterIds.Contains(x.ID_Supplier));
            }

            return query;
        }

        public static IQueryable<SupplierEntity> ApplySorting(
            IQueryable<SupplierEntity> query,
            int sortColumn,
            string sortMode)
        {
            var desc = string.Equals(sortMode, "DESC", StringComparison.OrdinalIgnoreCase);

            if (!Enum.IsDefined(typeof(SupplierSortColumn), sortColumn))
            {
                return query.OrderByDescending(s => s.ID_Supplier);
            }

            var column = (SupplierSortColumn)sortColumn;

            switch (column)
            {
                case SupplierSortColumn.Name:
                    return desc
                        ? query.OrderByDescending(s => s.Name)
                        : query.OrderBy(s => s.Name);
                case SupplierSortColumn.CompanyName:
                    return desc
                        ? query.OrderByDescending(s => s.CompanyName ?? string.Empty)
                        : query.OrderBy(s => s.CompanyName ?? string.Empty);
                case SupplierSortColumn.City:
                    return desc
                        ? query.OrderByDescending(s => s.City)
                        : query.OrderBy(s => s.City);
                case SupplierSortColumn.State:
                    return desc
                        ? query.OrderByDescending(s => s.State)
                        : query.OrderBy(s => s.State);
                case SupplierSortColumn.Id:
                default:
                    return desc
                        ? query.OrderByDescending(s => s.ID_Supplier)
                        : query.OrderBy(s => s.ID_Supplier);
            }
        }

        public static TableOutput<Supplier> EmptyTableOutput(SupplierListInput? input)
        {
            var pageIndex = Math.Max(1, input?.PageIndex ?? 1);
            var pageSize = Math.Max(1, input?.PageSize ?? 10);

            return new TableOutput<Supplier>
            {
                TableData = new List<Supplier>(),
                TableSettings = new TableOutput_Settings
                {
                    PageIndex = pageIndex,
                    PageSize = pageSize,
                    TotalCount = 0
                }
            };
        }
    }
}
