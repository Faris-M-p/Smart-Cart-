using Ecommerce.Helpers.Common;
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
