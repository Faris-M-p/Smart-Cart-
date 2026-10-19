using SmartCartPro.Models;

namespace SmartCartPro.Services.Interfaces;

public interface ICatalogService
{
    // Category
    Task<OutputCategorySelect?> GetCategoryAsync(InputCategorySelect input, CancellationToken ct = default);
    Task<OutputCategoryList> ListCategoriesAsync(InputCategoryList input, CancellationToken ct = default);
    Task<OutputCategoryCreate> CreateCategoryAsync(InputCategoryCreate input, CancellationToken ct = default);
    Task<OutputCategoryUpdate> UpdateCategoryAsync(InputCategoryUpdate input, CancellationToken ct = default);

    // SubCategory
    Task<OutputSubCategorySelect?> GetSubCategoryAsync(InputSubCategorySelect input, CancellationToken ct = default);
    Task<OutputSubCategoryList> ListSubCategoriesAsync(InputSubCategoryList input, CancellationToken ct = default);
    Task<OutputSubCategoryCreate> CreateSubCategoryAsync(InputSubCategoryCreate input, CancellationToken ct = default);
    Task<OutputSubCategoryUpdate> UpdateSubCategoryAsync(InputSubCategoryUpdate input, CancellationToken ct = default);

    // Brand
    Task<OutputBrandSelect?> GetBrandAsync(InputBrandSelect input, CancellationToken ct = default);
    Task<OutputBrandList> ListBrandsAsync(InputBrandList input, CancellationToken ct = default);
    Task<OutputBrandCreate> CreateBrandAsync(InputBrandCreate input, CancellationToken ct = default);
    Task<OutputBrandUpdate> UpdateBrandAsync(InputBrandUpdate input, CancellationToken ct = default);

    // Unit
    Task<OutputUnitSelect?> GetUnitAsync(InputUnitSelect input, CancellationToken ct = default);
    Task<OutputUnitList> ListUnitsAsync(InputUnitList input, CancellationToken ct = default);
    Task<OutputUnitCreate> CreateUnitAsync(InputUnitCreate input, CancellationToken ct = default);
    Task<OutputUnitUpdate> UpdateUnitAsync(InputUnitUpdate input, CancellationToken ct = default);

    // Tax
    Task<OutputTaxSelect?> GetTaxAsync(InputTaxSelect input, CancellationToken ct = default);
    Task<OutputTaxList> ListTaxesAsync(InputTaxList input, CancellationToken ct = default);
    Task<OutputTaxCreate> CreateTaxAsync(InputTaxCreate input, CancellationToken ct = default);
    Task<OutputTaxUpdate> UpdateTaxAsync(InputTaxUpdate input, CancellationToken ct = default);

    // Lookups
    Task<IReadOnlyList<OutputCategorySelect>> GetCategoryLookupAsync(CancellationToken ct = default);
    Task<IReadOnlyList<OutputSubCategorySelect>> GetSubCategoryLookupAsync(int fkCategory, CancellationToken ct = default);
    Task<IReadOnlyList<OutputBrandSelect>> GetBrandLookupAsync(CancellationToken ct = default);
    Task<IReadOnlyList<OutputUnitSelect>> GetUnitLookupAsync(CancellationToken ct = default);
    Task<IReadOnlyList<OutputTaxSelect>> GetTaxLookupAsync(CancellationToken ct = default);
}
