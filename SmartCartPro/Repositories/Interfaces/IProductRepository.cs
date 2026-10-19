using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface IProductRepository
{
    // ── Product ───────────────────────────────────────────────────
    Task<OutputProductSelect?> GetProductAsync(InputProductSelect input, CancellationToken ct = default);
    Task<OutputProductList> ListProductsAsync(InputProductList input, CancellationToken ct = default);
    Task<OutputProductCheck> CheckProductAsync(InputProductCheck input, CancellationToken ct = default);
    Task<OutputProductCreate> CreateProductAsync(InputProductCreate input, CancellationToken ct = default);
    Task<OutputProductUpdate> UpdateProductAsync(InputProductUpdate input, CancellationToken ct = default);
    Task<OutputProductSetActive> SetProductActiveAsync(InputProductSetActive input, CancellationToken ct = default);

    // ── Dropdown data / stock summary ─────────────────────────────
    Task<OutputProductLookup> GetProductLookupAsync(InputProductLookup input, CancellationToken ct = default);
    Task<IReadOnlyList<OutputProductStockItem>> ListProductStockAsync(InputProductStockList input, CancellationToken ct = default);

    // ── Variant ───────────────────────────────────────────────────
    Task<OutputVariantSelect?> GetVariantAsync(InputVariantSelect input, CancellationToken ct = default);
    Task<OutputVariantList> ListVariantsAsync(InputVariantList input, CancellationToken ct = default);
    Task<OutputVariantCheck> CheckVariantAsync(InputVariantCheck input, CancellationToken ct = default);
    Task<OutputVariantCreate> CreateVariantAsync(InputVariantCreate input, CancellationToken ct = default);
    Task<OutputVariantUpdate> UpdateVariantAsync(InputVariantUpdate input, CancellationToken ct = default);
    Task<OutputVariantSetActive> SetVariantActiveAsync(InputVariantSetActive input, CancellationToken ct = default);

    // ── Picker lookups (JSON) ─────────────────────────────────────
    Task<IReadOnlyList<OutputProductSearchItem>> SearchProductsAsync(InputProductSearch input, CancellationToken ct = default);
    Task<OutputProductSearchItem?> FindByBarcodeAsync(InputProductBarcode input, CancellationToken ct = default);
}
