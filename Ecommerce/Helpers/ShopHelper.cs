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
        decimal? PriceFrom,
        decimal? PriceTo,
        int PageIndex,
        int PageSize,
        int SortColumn,
        string SortMode);

    public sealed class ShopProductListItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int SubCategoryId { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public decimal? MinPrice { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public static class ShopHelper
    {
        public const int DefaultPageSize = 10;
        public const int MaxPageSize = 50;

        public static List<int> ParseShopIds(string? value)
        {
            var fromJson = StringHelper.ParseFilterIds(value);
            if (fromJson.Count > 0)
            {
                return fromJson;
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                return new List<int>();
            }

            return value
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => int.TryParse(s, out var id) ? id : 0)
                .Where(id => id > 0)
                .Distinct()
                .ToList();
        }

        public static NormalizedShopListInput NormalizeInput(InputProduct input)
        {
            var pageIndex = Math.Max(1, input.PageIndex);
            var pageSize = input.PageSize <= 0 ? DefaultPageSize : Math.Min(MaxPageSize, input.PageSize);
            var raw = input.SearchName?.Trim() ?? string.Empty;
            string? searchLower = raw.Length >= 1 ? raw.ToLowerInvariant() : null;

            decimal? priceFrom = input.PriceFrom.HasValue && input.PriceFrom.Value > 0 ? input.PriceFrom : null;
            decimal? priceTo = input.PriceTo.HasValue && input.PriceTo.Value > 0 ? input.PriceTo : null;
            if (priceFrom.HasValue && priceTo.HasValue && priceFrom.Value > priceTo.Value)
            {
                (priceFrom, priceTo) = (priceTo, priceFrom);
            }

            return new NormalizedShopListInput(
                searchLower,
                ParseShopIds(input.SubCategoryIds),
                ParseShopIds(input.BrandIds),
                priceFrom,
                priceTo,
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
                query = query.Where(p => p.Name.ToLower().Contains(s) || p.Slug.ToLower().Contains(s));
            }

            if (n.SubCategoryIds.Count > 0)
            {
                query = query.Where(p => n.SubCategoryIds.Contains(p.FK_SubCategory));
            }

            if (n.BrandIds.Count > 0)
            {
                query = query.Where(p => p.FK_Brand != null && n.BrandIds.Contains(p.FK_Brand.Value));
            }

            return query;
        }

        public static IQueryable<ShopProductListItem> ApplyPriceFilter(
            IQueryable<ShopProductListItem> query,
            NormalizedShopListInput n)
        {
            if (n.PriceFrom.HasValue)
            {
                var from = n.PriceFrom.Value;
                query = query.Where(x => x.MinPrice != null && x.MinPrice >= from);
            }

            if (n.PriceTo.HasValue)
            {
                var to = n.PriceTo.Value;
                query = query.Where(x => x.MinPrice != null && x.MinPrice <= to);
            }

            return query;
        }

        public static IQueryable<ShopProductListItem> ApplySorting(
            IQueryable<ShopProductListItem> query,
            int sortColumn,
            string sortMode)
        {
            var desc = string.Equals(sortMode, "DESC", StringComparison.OrdinalIgnoreCase);

            if (!Enum.IsDefined(typeof(ShopProductSortColumn), sortColumn))
            {
                return query.OrderByDescending(p => p.ProductId);
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
                        ? query.OrderByDescending(p => p.MinPrice ?? 0).ThenByDescending(p => p.ProductId)
                        : query.OrderBy(p => p.MinPrice ?? 0).ThenBy(p => p.ProductId);
                case ShopProductSortColumn.SubCategoryId:
                    return desc
                        ? query.OrderByDescending(p => p.SubCategoryId)
                        : query.OrderBy(p => p.SubCategoryId);
                case ShopProductSortColumn.CreatedAt:
                    return desc
                        ? query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.ProductId)
                        : query.OrderBy(p => p.CreatedAt).ThenBy(p => p.ProductId);
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
            var ps = input?.PageSize > 0
                ? Math.Min(MaxPageSize, input.PageSize)
                : DefaultPageSize;
            return new TableOutput<Product>
            {
                TableData = new List<Product>(),
                TableSettings = new TableOutput_Settings { PageIndex = pi, PageSize = ps, TotalCount = 0 }
            };
        }
    }
}
