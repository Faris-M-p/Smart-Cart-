using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

/// <summary>Lookup / dropdown fill queries (EF Core + LINQ).</summary>
public interface IFilterRepository
{
    Task<IReadOnlyList<FillProduct>> GetProductsAsync(
        FilterOptions filter,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FillCategory>> GetCategoriesAsync(
        FilterOptions filter,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FillSubCategory>> GetSubCategoriesAsync(
        FilterOptions filter,
        CancellationToken cancellationToken = default);
}
