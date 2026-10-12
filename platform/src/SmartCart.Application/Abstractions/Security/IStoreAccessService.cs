namespace SmartCart.Application.Abstractions.Security;

public interface IStoreAccessService
{
    Task<int> ResolveStoreIdAsync(int employeeId, int? requestedStoreId, CancellationToken cancellationToken = default);
    Task AssertStoreAccessAsync(int employeeId, int storeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<int>> GetAuthorizedStoreIdsAsync(int employeeId, CancellationToken cancellationToken = default);
}
