using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo) => _repo = repo;

    public Task<OutputProductSelect?> GetProductAsync(InputProductSelect input, CancellationToken ct = default) => _repo.GetProductAsync(input, ct);
    public Task<OutputProductList> ListProductsAsync(InputProductList input, CancellationToken ct = default) => _repo.ListProductsAsync(input, ct);
    public Task<OutputProductCreate> CreateProductAsync(InputProductCreate input, CancellationToken ct = default) => _repo.CreateProductAsync(input, ct);
    public Task<OutputProductUpdate> UpdateProductAsync(InputProductUpdate input, CancellationToken ct = default) => _repo.UpdateProductAsync(input, ct);

    public Task<OutputVariantSelect?> GetVariantAsync(InputVariantSelect input, CancellationToken ct = default) => _repo.GetVariantAsync(input, ct);
    public Task<OutputVariantList> ListVariantsAsync(InputVariantList input, CancellationToken ct = default) => _repo.ListVariantsAsync(input, ct);
    public Task<OutputVariantCreate> CreateVariantAsync(InputVariantCreate input, CancellationToken ct = default) => _repo.CreateVariantAsync(input, ct);
    public Task<OutputVariantUpdate> UpdateVariantAsync(InputVariantUpdate input, CancellationToken ct = default) => _repo.UpdateVariantAsync(input, ct);

    public Task<OutputProductSelect?> FindByBarcodeAsync(string barcode, CancellationToken ct = default) => _repo.FindByBarcodeAsync(barcode, ct);
}
