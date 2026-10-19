namespace SmartCartPro.Services.Interfaces;

public interface IDocumentNumberService
{
    /// <summary>
    /// Returns the next formatted document number for the given store and sequence type,
    /// incrementing the counter atomically with a row-level lock.
    /// </summary>
    /// <param name="storeId">The store (fk_store) to which the sequence belongs.</param>
    /// <param name="sequenceType">Sequence type identifier, e.g. "SALE", "PURCHASE", "RETURN".</param>
    /// <param name="ct">Cancellation token.</param>
    Task<string> NextAsync(int storeId, string sequenceType, CancellationToken ct = default);
}
