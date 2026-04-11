using Ecommerce.Helpers.Common;
using Ecommerce.Models.Entities;
using Ecommerce.Models.Enums;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.ProductModel;

namespace Ecommerce.Helpers.Shop
{
    public sealed record NormalizedShopListInput(
        string? SearchLower,
        IReadOnlyList<int> CategoryIds,
        IReadOnlyList<int> SubCategoryIds,
        IReadOnlyList<int> BrandIds,
        IReadOnlyList<int> RatingFilters,
        string? GenderNormalized,
        decimal? PriceFrom,
        decimal? PriceTo,
        int? StatusId,
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
            string? searchLower = raw.Length >= 2 ? raw.ToLowerInvariant() : null;
            var gender = string.IsNullOrWhiteSpace(input.Gender)
                ? null
                : input.Gender.Trim().ToLowerInvariant();

            int? statusId = null;
            if (!string.IsNullOrWhiteSpace(input.Status) && int.TryParse(input.Status.Trim(), out var sid))
            {
                statusId = sid;
            }

            return new NormalizedShopListInput(
                searchLower,
                StringHelper.ParseFilterIds(input.CategoryIds),
                StringHelper.ParseFilterIds(input.SubCategoryIds),
                StringHelper.ParseFilterIds(input.BrandIds),
                ParseRatingFilters(input.Ratings),
                gender,
                input.PriceFrom,
                input.PriceTo,
                statusId,
                pageIndex,
                pageSize,
                input.SortColumn,
                input.SortMode?.Trim() ?? "ASC");
        }

        public static IQueryable<ProductEntity> ApplyFilters(
            IQueryable<ProductEntity> query,
            NormalizedShopListInput n)
        {
            query = query.Where(p => p.Cancelled != true);

            if (n.SearchLower != null)
            {
                var s = n.SearchLower;
                query = query.Where(p => p.Name.ToLower().Contains(s));
            }

            if (n.CategoryIds.Count > 0)
            {
                query = query.Where(p => p.CategoryId != null && n.CategoryIds.Contains(p.CategoryId.Value));
            }

            if (n.SubCategoryIds.Count > 0)
            {
                query = query.Where(p => p.SubCategoryId != null && n.SubCategoryIds.Contains(p.SubCategoryId.Value));
            }

            if (n.BrandIds.Count > 0)
            {
                query = query.Where(p => p.BrandId != null && n.BrandIds.Contains(p.BrandId.Value));
            }

            if (n.RatingFilters.Count > 0)
            {
                query = query.Where(p =>
                    p.Rating != null &&
                    n.RatingFilters.Contains((int)Math.Round((double)p.Rating.Value)));
            }

            if (n.GenderNormalized != null)
            {
                query = query.Where(p =>
                    p.Gender != null &&
                    p.Gender.ToLower() == n.GenderNormalized);
            }

            if (n.PriceFrom.HasValue)
            {
                query = query.Where(p => p.Price >= n.PriceFrom.Value);
            }

            if (n.PriceTo.HasValue)
            {
                query = query.Where(p => p.Price <= n.PriceTo.Value);
            }

            if (n.StatusId.HasValue)
            {
                query = query.Where(p => p.StatusId == n.StatusId.Value);
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
                    ? query.OrderByDescending(p => p.ProductId)
                    : query.OrderBy(p => p.ProductId);
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
                        ? query.OrderByDescending(p => p.Price)
                        : query.OrderBy(p => p.Price);
                case ShopProductSortColumn.CategoryId:
                    return desc
                        ? query.OrderByDescending(p => p.CategoryId)
                        : query.OrderBy(p => p.CategoryId);
                case ShopProductSortColumn.ProductId:
                default:
                    return desc
                        ? query.OrderByDescending(p => p.ProductId)
                        : query.OrderBy(p => p.ProductId);
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
