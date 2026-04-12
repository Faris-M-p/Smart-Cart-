using Ecommerce.Helpers.Common;
using Ecommerce.Models.Entities;
using Ecommerce.Models.Enums;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.ProductModel;

namespace Ecommerce.Helpers.Shop
{
    public sealed record NormalizedShopListInput(
        string? SearchLower,
        IReadOnlyList<int> SubCategoryIds,
        IReadOnlyList<int> BrandIds,
        int PageIndex,
        int PageSize,
        int SortColumn,
        string SortMode);

    public static class ShopHelper
    {
        public static NormalizedShopListInput NormalizeInput(InputProduct input)
        {
            var pageIndex = Math.Max(1, input.PageIndex);
            var pageSize = Math.Max(1, input.PageSize);
            var raw = input.SearchName?.Trim() ?? string.Empty;
            string? searchLower = raw.Length >= 1 ? raw.ToLowerInvariant() : null;

            return new NormalizedShopListInput(
                searchLower,
                StringHelper.ParseFilterIds(input.SubCategoryIds),
                StringHelper.ParseFilterIds(input.BrandIds),
                pageIndex,
                pageSize,
                input.SortColumn,
                input.SortMode?.Trim() ?? "ASC");
        }

        public static IQueryable<ProductEntity> ApplyFilters(
            IQueryable<ProductEntity> query,
            NormalizedShopListInput n)
        {
            query = query.Where(p => p.Cancelled != true && p.IsActive);

            if (n.SearchLower != null)
            {
                var s = n.SearchLower;
                query = query.Where(p => p.Name.ToLower().Contains(s));
            }

            if (n.SubCategoryIds.Count > 0)
            {
                query = query.Where(p => n.SubCategoryIds.Contains(p.FkSubCategory));
            }

            if (n.BrandIds.Count > 0)
            {
                query = query.Where(p => p.FkBrand != null && n.BrandIds.Contains(p.FkBrand.Value));
            }

            return query;
        }

        public static IQueryable<ProductEntity> ApplySorting(
            IQueryable<ProductEntity> query,
            int sortColumn,
            string sortMode)
        {
            var desc = string.Equals(sortMode, "DESC", StringComparison.OrdinalIgnoreCase);

            if (!Enum.IsDefined(typeof(ShopProductSortColumn), sortColumn))
            {
                return desc
                    ? query.OrderByDescending(p => p.IdProduct)
                    : query.OrderBy(p => p.IdProduct);
            }

            var column = (ShopProductSortColumn)sortColumn;

            switch (column)
            {
                case ShopProductSortColumn.Name:
                    return desc
                        ? query.OrderByDescending(p => p.Name)
                        : query.OrderBy(p => p.Name);
                case ShopProductSortColumn.Price:
                    return desc
                        ? query.OrderByDescending(p => p.Name)
                        : query.OrderBy(p => p.Name);
                case ShopProductSortColumn.SubCategoryId:
                    return desc
                        ? query.OrderByDescending(p => p.FkSubCategory)
                        : query.OrderBy(p => p.FkSubCategory);
                case ShopProductSortColumn.ProductId:
                default:
                    return desc
                        ? query.OrderByDescending(p => p.IdProduct)
                        : query.OrderBy(p => p.IdProduct);
            }
        }

        public static List<int> ParseRatingFilters(string? ratings)
        {
            if (string.IsNullOrWhiteSpace(ratings))
            {
                return new List<int>();
            }

            return ratings.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => int.TryParse(s, out var v) ? v : (int?)null)
                .Where(v => v.HasValue)
                .Select(v => v!.Value)
                .Distinct()
                .ToList();
        }

        public static TableOutput<Product> EmptyTableOutput(InputProduct? input)
        {
            var pi = Math.Max(1, input?.PageIndex ?? 1);
            var ps = Math.Max(1, input?.PageSize ?? 10);
            return new TableOutput<Product>
            {
                TableData = new List<Product>(),
                TableSettings = new TableOutput_Settings { PageIndex = pi, PageSize = ps, TotalCount = 0 }
            };
        }
    }
}
