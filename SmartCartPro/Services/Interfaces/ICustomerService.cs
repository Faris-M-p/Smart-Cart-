using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface ICustomerService
{
    Task<OutputCustomerSelect?> GetCustomerAsync(InputCustomerSelect input, CancellationToken ct = default);
    Task<OutputCustomerList> ListCustomersAsync(InputCustomerList input, CancellationToken ct = default);
    Task<OutputCustomerCreate> CreateCustomerAsync(InputCustomerCreate input, CancellationToken ct = default);
    Task<OutputCustomerUpdate> UpdateCustomerAsync(InputCustomerUpdate input, CancellationToken ct = default);
    Task<OutputCustomerSelect?> FindByPhoneAsync(string phone, CancellationToken ct = default);
}
