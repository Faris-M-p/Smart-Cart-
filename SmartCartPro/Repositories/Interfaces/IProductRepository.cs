using SmartCartPro.Models;

namespace SmartCartPro.Repositories.Interfaces;

public interface IProductRepository
{
    // ── Product ───────────────────────────────────────────────────
    Task<OutputProductSelect?> GetProductAsync(InputProductSelect input, CancellationToken ct = default);
    Task<OutputProductList> ListProductsAsync(InputProductList input, CancellationToken ct = default);
    Task<OutputProductCreate> CreateProductAsync(InputProductCreate input, CancellationToken ct = default);
    Task<OutputProductUpdate> UpdateProductAsync(InputProductUpdate input, CancellationToken ct = default);

    // ── Variant ───────────────────────────────────────────────────
    Task<OutputVariantSelect?> GetVariantAsync(InputVariantSelect input, CancellationToken ct = default);
    Task<OutputVariantList> ListVariantsAsync(InputVariantList input, CancellationToken ct = default);
    Task<OutputVariantCreate> CreateVariantAsync(InputVariantCreate input, CancellationToken ct = default);
    Task<OutputVariantUpdate> UpdateVariantAsync(InputVariantUpdate input, CancellationToken ct = default);

    // ── Barcode / SKU lookup (JSON) ───────────────────────────────
    Task<OutputProductSelect?> FindByBarcodeAsync(string barcode, CancellationToken ct = default);
}
