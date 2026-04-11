using Ecommerce.Models.Enums;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.Admin.ProductVariantModel;

namespace Ecommerce.Helpers.ProductVariants
{
    public static class ProductVariantHelper
    {
        public static object BuildProductVariantListSpParameters(ProductVariantListInput input)
        {
            var pageIndex = Math.Max(1, input.PageIndex);
            var pageSize = Math.Max(1, input.PageSize);
            var sortMode = string.IsNullOrWhiteSpace(input.SortMode) ? "ASC" : input.SortMode.Trim();
            var sortColumn = MapProductVariantSortColumnForSp(input.SortColumn);

            return new
            {
                FK_Product = input.FK_Product,
                SearchText = input.SearchText ?? string.Empty,
                FilterVariantIDs = input.FilterVariantIDs ?? string.Empty,
                FilterVariantValueIDs = input.FilterVariantValueIDs ?? string.Empty,
                PageIndex = pageIndex,
                PageSize = pageSize,
                SortColumn = sortColumn,
                SortMode = sortMode
            };
        }

        public static string MapProductVariantSortColumnForSp(int sortColumn)
        {
            if (!Enum.IsDefined(typeof(ProductVariantSortColumn), sortColumn))
            {
                return "ID_ProductVariant";
            }

            return (ProductVariantSortColumn)sortColumn switch
            {
                ProductVariantSortColumn.PriceAdjustment => "PriceAdjustment",
                ProductVariantSortColumn.CreatedOn => "CreatedOn",
                ProductVariantSortColumn.StockAvailable => "StockAvailable",
                ProductVariantSortColumn.Id => "ID_ProductVariant",
                _ => "ID_ProductVariant"
            };
        }

        public static TableOutput<ProductVariant> EmptyTableOutput(ProductVariantListInput? input)
        {
            var pi = Math.Max(1, input?.PageIndex ?? 1);
            var ps = Math.Max(1, input?.PageSize ?? 20);
            return new TableOutput<ProductVariant>
            {
                TableData = new List<ProductVariant>(),
                TableSettings = new TableOutput_Settings { PageIndex = pi, PageSize = ps, TotalCount = 0 }
            };
        }
    }
}
