namespace SmartCartPro.Repositories.Interfaces;

public interface IDocumentNumberRepository
{
    Task<string> NextAsync(int storeId, string sequenceType, CancellationToken ct = default);
}
