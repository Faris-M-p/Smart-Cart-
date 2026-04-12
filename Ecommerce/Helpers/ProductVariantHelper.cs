using Ecommerce.Helpers.Common;
using Ecommerce.Models.Entities;
using Ecommerce.Models.Enums;
using static Ecommerce.Models.Admin.ProductVariantModel;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Helpers.ProductVariants
{
    public sealed record NormalizedProductVariantListInput(
        string? SearchLower,
        IReadOnlyList<int> FilterVariantIds,
        IReadOnlyList<int> FilterVariantValueIds,
        int PageIndex,
        int PageSize,
        int SortColumn,
        string SortMode);

    public static class ProductVariantHelper
    {
        public static NormalizedProductVariantListInput NormalizeInput(ProductVariantListInput input)
        {
            var pageIndex = Math.Max(1, input.PageIndex);
            var pageSize = Math.Max(1, input.PageSize);
            var raw = input.SearchText?.Trim() ?? string.Empty;
            string? searchLower = raw.Length >= 1 ? raw.ToLowerInvariant() : null;
            return new NormalizedProductVariantListInput(
                searchLower,
                StringHelper.ParseFilterIds(input.FilterVariantIDs),
                StringHelper.ParseFilterIds(input.FilterVariantValueIDs),
                pageIndex,
                pageSize,
                input.SortColumn,
                input.SortMode?.Trim() ?? string.Empty);
        }

        /// <summary>Short label: <c>Black - 9</c> (value names, variant name order).</summary>
        public static string GenerateVariantLabel(IReadOnlyList<(string VariantName, string ValueName)> rows)
        {
            if (rows == null || rows.Count == 0)
            {
                return string.Empty;
            }

            var ordered = rows
                .OrderBy(r => r.VariantName, StringComparer.OrdinalIgnoreCase)
                .Select(r => (r.ValueName ?? string.Empty).Trim())
                .Where(s => s.Length > 0)
                .ToList();

            return ordered.Count == 0 ? string.Empty : string.Join(" - ", ordered);
        }

        /// <summary>Joined text: <c>Size:9, Color:Black</c> (variant name order).</summary>
        public static string GenerateCombination(IReadOnlyList<(string VariantName, string ValueName)> rows)
        {
            if (rows == null || rows.Count == 0)
            {
                return string.Empty;
            }

            var parts = rows
                .OrderBy(r => r.VariantName, StringComparer.OrdinalIgnoreCase)
                .Select(r =>
                {
                    var nv = (r.VariantName ?? string.Empty).Trim();
                    var val = (r.ValueName ?? string.Empty).Trim();
                    return string.IsNullOrEmpty(nv) ? string.Empty : $"{nv}:{val}".Trim();
                })
                .Where(s => s.Length > 1 && !s.EndsWith(':'))
                .ToList();

            return parts.Count == 0 ? string.Empty : string.Join(", ", parts);
        }

        public static bool SameVariantValueSet(IReadOnlyList<int> a, IReadOnlyList<int> b)
        {
            if (a.Count != b.Count)
            {
                return false;
            }

            var sa = a.OrderBy(x => x).ToArray();
            var sb = b.OrderBy(x => x).ToArray();
            for (var i = 0; i < sa.Length; i++)
            {
                if (sa[i] != sb[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>After variant values are resolved from DB: one value per variant type, non-empty.</summary>
        public static string? ValidateCombination(
            IReadOnlyList<(int FkVariant, int FkVariantValue, string VariantName, string ValueName)> resolvedRows)
        {
            if (resolvedRows == null || resolvedRows.Count == 0)
            {
                return "At least one variant value is required.";
            }

            if (resolvedRows.GroupBy(r => r.FkVariant).Any(g => g.Count() > 1))
            {
                return "Each variant type may only have one selected value.";
            }

            return null;
        }

        /// <summary>Validates UI rows before DB resolution: unique variant dimension, unique value id.</summary>
        public static string? ValidateVariantValueForm(IReadOnlyList<ProductVariantValueRowInput> rows)
        {
            if (rows == null || rows.Count == 0)
            {
                return "At least one variant value is required.";
            }

            if (rows.Any(r => r.VariantId <= 0 || r.VariantValueId <= 0))
            {
                return "Each row must have a valid variant and value.";
            }

            if (rows.GroupBy(r => r.VariantId).Any(g => g.Count() > 1))
            {
                return "Each variant type may only appear once in the form.";
            }

            if (rows.GroupBy(r => r.VariantValueId).Any(g => g.Count() > 1))
            {
                return "Duplicate variant value selected.";
            }

            return null;
        }

        public static ProductVariantUpdateInput NormalizeUpdateInput(ProductVariantUpdateInput input)
        {
            ArgumentNullException.ThrowIfNull(input);

            var rows = (input.VariantValues ?? new List<ProductVariantValueRowInput>())
                .Where(r => r.VariantId > 0 && r.VariantValueId > 0)
                .ToList();

            return new ProductVariantUpdateInput
            {
                ID_ProductVariant = input.ID_ProductVariant,
                FK_Product = input.FK_Product,
                SKU = (input.SKU ?? string.Empty).Trim(),
                VariantLabel = (input.VariantLabel ?? string.Empty).Trim(),
                MRP = input.MRP < 0 ? 0 : input.MRP,
                SellingPrice = input.SellingPrice,
                IsActive = input.IsActive,
                IsDefault = input.IsDefault,
                VariantValues = rows,
                EnterBy = input.EnterBy
            };
        }

        public static IQueryable<ProductVariantEntity> ApplyFilters(
            IQueryable<ProductVariantEntity> query,
            int fkProduct,
            NormalizedProductVariantListInput n)
        {
            query = query.Where(pv => pv.FkProduct == fkProduct && !pv.Cancelled);

            if (n.SearchLower != null)
            {
                var s = n.SearchLower;
                query = query.Where(pv =>
                    pv.Sku.ToLower().Contains(s) ||
                    pv.VariantLabel.ToLower().Contains(s));
            }

            return query;
        }

        public static IQueryable<ProductVariantEntity> ApplySorting(
            IQueryable<ProductVariantEntity> query,
            int sortColumn,
            string sortMode)
        {
            var desc = string.Equals(sortMode, "DESC", StringComparison.OrdinalIgnoreCase);

            if (!Enum.IsDefined(typeof(ProductVariantSortColumn), sortColumn))
            {
                return desc
                    ? query.OrderByDescending(pv => pv.IdProductVariant)
                    : query.OrderBy(pv => pv.IdProductVariant);
            }

            var column = (ProductVariantSortColumn)sortColumn;

            switch (column)
            {
                case ProductVariantSortColumn.Sku:
                    return desc
                        ? query.OrderByDescending(pv => pv.Sku)
                        : query.OrderBy(pv => pv.Sku);
                case ProductVariantSortColumn.SellingPrice:
                    return desc
                        ? query.OrderByDescending(pv => pv.SellingPrice)
                        : query.OrderBy(pv => pv.SellingPrice);
                case ProductVariantSortColumn.VariantLabel:
                    return desc
                        ? query.OrderByDescending(pv => pv.VariantLabel)
                        : query.OrderBy(pv => pv.VariantLabel);
                case ProductVariantSortColumn.CreatedAt:
                    return desc
                        ? query.OrderByDescending(pv => pv.CreatedAt)
                        : query.OrderBy(pv => pv.CreatedAt);
                case ProductVariantSortColumn.Id:
                default:
                    return desc
                        ? query.OrderByDescending(pv => pv.IdProductVariant)
                        : query.OrderBy(pv => pv.IdProductVariant);
            }
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
