using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface ICustomerRepository
{
    Task<OutputCustomerSelect?> GetCustomerAsync(InputCustomerSelect input, CancellationToken ct = default);
    Task<OutputCustomerList> ListCustomersAsync(InputCustomerList input, CancellationToken ct = default);
    Task<OutputCustomerCreate> CreateCustomerAsync(InputCustomerCreate input, CancellationToken ct = default);
    Task<OutputCustomerUpdate> UpdateCustomerAsync(InputCustomerUpdate input, CancellationToken ct = default);
    Task<OutputCustomerSetActive> SetCustomerActiveAsync(InputCustomerSetActive input, CancellationToken ct = default);
    Task<IReadOnlyList<OutputCustomerSearchItem>> SearchCustomersAsync(InputCustomerSearch input, CancellationToken ct = default);

    /// <summary>Used by POS phone-search JSON endpoint.</summary>
    Task<OutputCustomerSelect?> FindByPhoneAsync(string phone, CancellationToken ct = default);
}
