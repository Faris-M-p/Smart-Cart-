using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface IProductService
{
    // ── Product ───────────────────────────────────────────────────
    Task<OutputProductSelect?> GetProductAsync(InputProductSelect input, CancellationToken ct = default);
    Task<OutputProductDetail?> GetProductDetailAsync(InputProductDetail input, CancellationToken ct = default);
    Task<OutputProductList> ListProductsAsync(InputProductList input, CancellationToken ct = default);
    Task<OutputProductCreate> CreateProductAsync(InputProductCreate input, CancellationToken ct = default);
    Task<OutputProductUpdate> UpdateProductAsync(InputProductUpdate input, CancellationToken ct = default);
    Task<OutputProductSetActive> SetProductActiveAsync(InputProductSetActive input, CancellationToken ct = default);

    /// <summary>Dropdown data for the product forms (selected inactive rows stay visible so edits do not drop them).</summary>
    Task<OutputProductLookup> GetProductLookupAsync(InputProductLookup input, CancellationToken ct = default);

    // ── Variant ───────────────────────────────────────────────────
    Task<OutputVariantSelect?> GetVariantAsync(InputVariantSelect input, CancellationToken ct = default);

    /// <summary>Returns null when the list is scoped to a product that does not exist.</summary>
    Task<OutputVariantList?> ListVariantsAsync(InputVariantList input, CancellationToken ct = default);
    Task<OutputVariantCreate> CreateVariantAsync(InputVariantCreate input, CancellationToken ct = default);
    Task<OutputVariantUpdate> UpdateVariantAsync(InputVariantUpdate input, CancellationToken ct = default);
    Task<OutputVariantSetActive> SetVariantActiveAsync(InputVariantSetActive input, CancellationToken ct = default);

    // ── Picker lookups (JSON) ─────────────────────────────────────
    Task<IReadOnlyList<OutputProductSearchItem>> SearchProductsAsync(InputProductSearch input, CancellationToken ct = default);
    Task<OutputProductSearchItem?> FindByBarcodeAsync(InputProductBarcode input, CancellationToken ct = default);
}
