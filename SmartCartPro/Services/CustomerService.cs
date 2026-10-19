using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repo;

    public CustomerService(ICustomerRepository repo) => _repo = repo;

    public Task<OutputCustomerSelect?> GetCustomerAsync(InputCustomerSelect input, CancellationToken ct = default) => _repo.GetCustomerAsync(input, ct);
    public Task<OutputCustomerList> ListCustomersAsync(InputCustomerList input, CancellationToken ct = default) => _repo.ListCustomersAsync(input, ct);
    public Task<OutputCustomerCreate> CreateCustomerAsync(InputCustomerCreate input, CancellationToken ct = default) => _repo.CreateCustomerAsync(input, ct);
    public Task<OutputCustomerUpdate> UpdateCustomerAsync(InputCustomerUpdate input, CancellationToken ct = default) => _repo.UpdateCustomerAsync(input, ct);
    public Task<OutputCustomerSelect?> FindByPhoneAsync(string phone, CancellationToken ct = default) => _repo.FindByPhoneAsync(phone, ct);
}
