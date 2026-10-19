using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class DocumentNumberService : IDocumentNumberService
{
    private readonly IDocumentNumberRepository _repo;

    public DocumentNumberService(IDocumentNumberRepository repo) => _repo = repo;

    public Task<string> NextAsync(int storeId, string sequenceType, CancellationToken ct = default) =>
        _repo.NextAsync(storeId, sequenceType, ct);
}
