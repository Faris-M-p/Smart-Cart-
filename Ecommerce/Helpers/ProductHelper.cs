using Ecommerce.Helpers.Common;
using Ecommerce.Models.Entities;
using Ecommerce.Models.Enums;
using static Ecommerce.Models.Admin.ProductModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Helpers.Products
{
    public sealed record NormalizedProductListInput(
        string? SearchLower,
        IReadOnlyList<int> FilterCategoryIds,
        IReadOnlyList<int> FilterSubCategoryIds,
        IReadOnlyList<int> FilterBrandIds,
        IReadOnlyList<int> FilterStatusIds,
        int PageIndex,
        int PageSize,
        int SortColumn,
        string SortMode);

    public sealed record NormalizedProductWriteInput(
        string Name,
        string? Description,
        decimal Price,
        decimal? MRP,
        int FK_Category,
        int FK_SubCategory,
        int? FK_Brand,
        decimal? Rating,
        string? Gender,
        int FK_Status);

    public static class ProductHelper
    {
        public static NormalizedProductListInput NormalizeInput(ProductListInput input)
        {
            var pageIndex = Math.Max(1, input.PageIndex);
            var pageSize = Math.Max(1, input.PageSize);

            var rawSearch = input.SearchText?.Trim() ?? string.Empty;
            string? searchLower = null;
            if (rawSearch.Length >= 1)
            {
                searchLower = rawSearch.ToLowerInvariant();
            }

            var sortMode = input.SortMode?.Trim() ?? string.Empty;

            return new NormalizedProductListInput(
                searchLower,
                StringHelper.ParseFilterIds(input.FilterCategoryIDs),
                StringHelper.ParseFilterIds(input.FilterSubCategoryIDs),
                StringHelper.ParseFilterIds(input.FilterBrandIDs),
                StringHelper.ParseFilterIds(input.FilterStatusIDs),
                pageIndex,
                pageSize,
                input.SortColumn,
                sortMode);
        }

        public static NormalizedProductWriteInput NormalizeInput(ProductUpdateInput input)
        {
            return new NormalizedProductWriteInput(
                Name: (input.Name ?? string.Empty).Trim(),
                Description: StringHelper.NormalizeOptionalString(input.Description),
                Price: input.Price,
                MRP: input.MRP,
                FK_Category: input.FK_Category,
                FK_SubCategory: input.FK_SubCategory,
                FK_Brand: input.FK_Brand,
                Rating: input.Rating,
                Gender: StringHelper.NormalizeOptionalString(input.Gender),
                FK_Status: input.FK_Status <= 0 ? 1 : input.FK_Status);
        }

        public static IQueryable<ProductEntity> ApplyFilters(
            IQueryable<ProductEntity> query,
            NormalizedProductListInput n)
        {
            query = query.Where(p => p.Cancelled != true);

            if (n.SearchLower != null)
            {
                var s = n.SearchLower;
                query = query.Where(p => p.Name.ToLower().Contains(s));
            }

            if (n.FilterSubCategoryIds.Count > 0)
            {
                query = query.Where(p =>
                    p.SubCategoryId != null &&
                    n.FilterSubCategoryIds.Contains(p.SubCategoryId.Value));
            }
            else if (n.FilterCategoryIds.Count > 0)
            {
                query = query.Where(p =>
                    p.CategoryId != null &&
                    n.FilterCategoryIds.Contains(p.CategoryId.Value));
            }

            if (n.FilterBrandIds.Count > 0)
            {
                query = query.Where(p =>
                    p.BrandId != null &&
                    n.FilterBrandIds.Contains(p.BrandId.Value));
            }

            if (n.FilterStatusIds.Count > 0)
            {
                query = query.Where(p =>
                    p.StatusId != null &&
                    n.FilterStatusIds.Contains(p.StatusId.Value));
            }

            return query;
        }

        public static IQueryable<ProductEntity> ApplySorting(
            IQueryable<ProductEntity> query,
            int sortColumn,
            string sortMode)
        {
            var desc = string.Equals(sortMode, "DESC", StringComparison.OrdinalIgnoreCase);

            if (!Enum.IsDefined(typeof(ProductSortColumn), sortColumn))
            {
                return query.OrderByDescending(p => p.ProductId);
            }

            var column = (ProductSortColumn)sortColumn;

            switch (column)
            {
                case ProductSortColumn.Name:
                    return desc
                        ? query.OrderByDescending(p => p.Name)
                        : query.OrderBy(p => p.Name);
                case ProductSortColumn.Price:
                    return desc
                        ? query.OrderByDescending(p => p.Price)
                        : query.OrderBy(p => p.Price);
                case ProductSortColumn.CreatedOn:
                    return desc
                        ? query.OrderByDescending(p => p.CreatedAt)
                        : query.OrderBy(p => p.CreatedAt);
                case ProductSortColumn.Id:
                default:
                    return desc
                        ? query.OrderByDescending(p => p.ProductId)
                        : query.OrderBy(p => p.ProductId);
            }
        }

        public static TableOutput<Product> EmptyTableOutput(ProductListInput? input)
        {
            var pageIndex = Math.Max(1, input?.PageIndex ?? 1);
            var pageSize = Math.Max(1, input?.PageSize ?? 10);

            return new TableOutput<Product>
            {
                TableData = new List<Product>(),
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
