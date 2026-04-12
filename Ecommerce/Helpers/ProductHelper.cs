using System.Text;
using Ecommerce.Helpers.Common;
using Ecommerce.Models.Entities;
using Ecommerce.Models.Enums;
using static Ecommerce.Models.Admin.ProductModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Helpers.Products
{
    public sealed record NormalizedProductListInput(
        string? SearchLower,
        IReadOnlyList<int> FilterSubCategoryIds,
        IReadOnlyList<int> FilterBrandIds,
        int PageIndex,
        int PageSize,
        int SortColumn,
        string SortMode);

    public sealed record NormalizedProductWriteInput(
        string Name,
        string? SlugOverride,
        string? Description,
        int FK_SubCategory,
        int? FK_Brand,
        bool IsActive);

    public static class ProductHelper
    {
        /// <summary>Lowercase slug: letters/digits kept, spaces to hyphen, collapse hyphens.</summary>
        public static string GenerateSlug(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "product";
            }

            var sb = new StringBuilder();
            var lastHyphen = false;
            foreach (var ch in name.Trim().ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(ch);
                    lastHyphen = false;
                }
                else if (char.IsWhiteSpace(ch) || ch == '-' || ch == '_')
                {
                    if (sb.Length > 0 && !lastHyphen)
                    {
                        sb.Append('-');
                        lastHyphen = true;
                    }
                }
            }

            var s = sb.ToString().Trim('-');
            return string.IsNullOrEmpty(s) ? "product" : s;
        }

        public static NormalizedProductListInput NormalizeInput(ProductListInput input)
        {
            var pageIndex = Math.Max(1, input.PageIndex);
            var pageSize = Math.Max(1, input.PageSize);
            var rawSearch = input.SearchText?.Trim() ?? string.Empty;
            string? searchLower = rawSearch.Length >= 1 ? rawSearch.ToLowerInvariant() : null;
            return new NormalizedProductListInput(
                searchLower,
                StringHelper.ParseFilterIds(input.FilterSubCategoryIDs),
                StringHelper.ParseFilterIds(input.FilterBrandIDs),
                pageIndex,
                pageSize,
                input.SortColumn,
                input.SortMode?.Trim() ?? string.Empty);
        }

        public static NormalizedProductWriteInput NormalizeWriteInput(ProductUpdateInput input)
        {
            var name = (input.Name ?? string.Empty).Trim();
            var slugRaw = (input.Slug ?? string.Empty).Trim();
            string? slugOverride = slugRaw.Length > 0 ? slugRaw : null;
            var description = StringHelper.NormalizeOptionalString(input.Description);
            return new NormalizedProductWriteInput(name, slugOverride, description, input.FK_SubCategory, input.FK_Brand, input.IsActive);
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
                query = query.Where(p => n.FilterSubCategoryIds.Contains(p.FkSubCategory));
            }

            if (n.FilterBrandIds.Count > 0)
            {
                query = query.Where(p => p.FkBrand != null && n.FilterBrandIds.Contains(p.FkBrand.Value));
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
                return desc
                    ? query.OrderByDescending(p => p.IdProduct)
                    : query.OrderBy(p => p.IdProduct);
            }

            var column = (ProductSortColumn)sortColumn;

            switch (column)
            {
                case ProductSortColumn.Name:
                    return desc
                        ? query.OrderByDescending(p => p.Name)
                        : query.OrderBy(p => p.Name);
                case ProductSortColumn.Slug:
                    return desc
                        ? query.OrderByDescending(p => p.Slug)
                        : query.OrderBy(p => p.Slug);
                case ProductSortColumn.CreatedOn:
                    return desc
                        ? query.OrderByDescending(p => p.CreatedAt)
                        : query.OrderBy(p => p.CreatedAt);
                case ProductSortColumn.Id:
                default:
                    return desc
                        ? query.OrderByDescending(p => p.IdProduct)
                        : query.OrderBy(p => p.IdProduct);
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
