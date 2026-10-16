using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface IFilterService
{
    Task<FilterDataResponse> GetFilterDataAsync(
        FilterOptions filter,
        CancellationToken cancellationToken = default);
}
