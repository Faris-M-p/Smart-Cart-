using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class FilterService : IFilterService
{
    private readonly IFilterRepository _filterRepository;

    public FilterService(IFilterRepository filterRepository)
    {
        _filterRepository = filterRepository;
    }

    public async Task<FilterDataResponse> GetFilterDataAsync(
        FilterOptions filter,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        if (filter.Mode <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(filter.Mode),
                filter.Mode,
                "Mode is required. Use 1=Products, 2=Categories, 3=SubCategories.");
        }

        var response = new FilterDataResponse();

        switch (filter.Mode)
        {
            case 1:
                response.Products = await _filterRepository.GetProductsAsync(filter, cancellationToken);
                break;

            case 2:
                response.Categories = await _filterRepository.GetCategoriesAsync(filter, cancellationToken);
                break;

            case 3:
                filter.CategoryId = ParseCategoryCriteria(filter.Criteria);
                response.SubCategories = await _filterRepository.GetSubCategoriesAsync(filter, cancellationToken);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(filter.Mode),
                    filter.Mode,
                    "Unsupported filter mode. Use 1=Products, 2=Categories, 3=SubCategories.");
        }

        return response;
    }

    /// <summary>
    /// Parses client Criteria into CategoryId for the repository.
    /// Null/empty → all subcategories; invalid value → ArgumentException.
    /// </summary>
    private static int? ParseCategoryCriteria(string? criteria)
    {
        if (string.IsNullOrWhiteSpace(criteria))
        {
            return null;
        }

        if (!int.TryParse(criteria.Trim(), out var categoryId) || categoryId <= 0)
        {
            throw new ArgumentException(
                "Criteria must be a positive category id when provided.",
                nameof(criteria));
        }

        return categoryId;
    }
}
