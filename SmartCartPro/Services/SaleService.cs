using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class SaleService : ISaleService
{
    private readonly ISaleRepository _repo;

    public SaleService(ISaleRepository repo) => _repo = repo;

    public Task<OutputSaleSelect?> GetSaleAsync(InputSaleSelect input, CancellationToken ct = default) => _repo.GetSaleAsync(input, ct);
    public Task<OutputSaleList> ListSalesAsync(InputSaleList input, CancellationToken ct = default) => _repo.ListSalesAsync(input, ct);
    public Task<OutputSaleCreate> CreateSaleAsync(InputSaleCreate input, CancellationToken ct = default) => _repo.CreateSaleAsync(input, ct);
    public Task<OutputSaleCancel> CancelSaleAsync(InputSaleCancel input, CancellationToken ct = default) => _repo.CancelSaleAsync(input, ct);
    public Task<OutputSaleReturn> ReturnSaleAsync(InputSaleReturn input, CancellationToken ct = default) => _repo.ReturnSaleAsync(input, ct);
    public Task<OutputCashSessionOpen> OpenSessionAsync(InputCashSessionOpen input, CancellationToken ct = default) => _repo.OpenSessionAsync(input, ct);
    public Task<OutputCashSessionClose> CloseSessionAsync(InputCashSessionClose input, CancellationToken ct = default) => _repo.CloseSessionAsync(input, ct);
    public Task<OutputCashSessionSelect?> GetOpenSessionAsync(InputCashSessionSelect input, CancellationToken ct = default) => _repo.GetOpenSessionAsync(input, ct);
    public Task<OutputCashSummary> GetCashSummaryAsync(InputCashSummary input, CancellationToken ct = default) => _repo.GetCashSummaryAsync(input, ct);
}
