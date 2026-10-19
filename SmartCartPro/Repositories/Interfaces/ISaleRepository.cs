using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface ISaleRepository
{
    Task<OutputSaleSelect?> GetSaleAsync(InputSaleSelect input, CancellationToken ct = default);
    Task<OutputSaleList> ListSalesAsync(InputSaleList input, CancellationToken ct = default);

    /// <summary>
    /// Creates a sale, deducts stock (sale movement), records payments, assigns invoice number.
    /// Fully transactional. Idempotency via ClientRef if provided.
    /// </summary>
    Task<OutputSaleCreate> CreateSaleAsync(InputSaleCreate input, CancellationToken ct = default);

    /// <summary>
    /// Cancels a sale and reverses stock (return_in movement). Requires permission pos.cancel.
    /// </summary>
    Task<OutputSaleCancel> CancelSaleAsync(InputSaleCancel input, CancellationToken ct = default);

    /// <summary>
    /// Processes a partial or full return against an existing invoice. Applies return_in stock movement.
    /// </summary>
    Task<OutputSaleReturn> ReturnSaleAsync(InputSaleReturn input, CancellationToken ct = default);

    // ── Cash session ─────────────────────────────────────────────
    Task<OutputCashSessionOpen> OpenSessionAsync(InputCashSessionOpen input, CancellationToken ct = default);
    Task<OutputCashSessionClose> CloseSessionAsync(InputCashSessionClose input, CancellationToken ct = default);
    Task<OutputCashSessionSelect?> GetOpenSessionAsync(InputCashSessionSelect input, CancellationToken ct = default);
    Task<OutputCashSummary> GetCashSummaryAsync(InputCashSummary input, CancellationToken ct = default);
}
