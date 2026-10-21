using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class DocumentNumberService : IDocumentNumberService
{
    private readonly IDocumentNumberRepository _repo;
    private readonly ISettingService _settings;

    public DocumentNumberService(IDocumentNumberRepository repo, ISettingService settings)
    {
        _repo = repo;
        _settings = settings;
    }

    public async Task<string> NextAsync(int storeId, string sequenceType, CancellationToken ct = default)
    {
        var number = await _repo.NextAsync(storeId, sequenceType, ct);

        if (!string.Equals(sequenceType, "SALE", StringComparison.OrdinalIgnoreCase))
            return number;

        var prefix = (await _settings.GetEffectiveValueAsync("pos.invoice_prefix", storeId, ct))?.Trim();
        if (string.IsNullOrEmpty(prefix))
            return number;

        if (number.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return number;

        return prefix + number;
    }
}
