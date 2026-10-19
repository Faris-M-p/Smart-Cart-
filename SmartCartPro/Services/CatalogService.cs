using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class CatalogService : ICatalogService
{
    private readonly ICatalogRepository _repo;

    public CatalogService(ICatalogRepository repo) => _repo = repo;

    public Task<OutputCategorySelect?> GetCategoryAsync(InputCategorySelect input, CancellationToken ct = default) => _repo.GetCategoryAsync(input, ct);
    public Task<OutputCategoryList> ListCategoriesAsync(InputCategoryList input, CancellationToken ct = default) => _repo.ListCategoriesAsync(input, ct);
    public Task<OutputCategoryCreate> CreateCategoryAsync(InputCategoryCreate input, CancellationToken ct = default) => _repo.CreateCategoryAsync(input, ct);
    public Task<OutputCategoryUpdate> UpdateCategoryAsync(InputCategoryUpdate input, CancellationToken ct = default) => _repo.UpdateCategoryAsync(input, ct);

    public Task<OutputSubCategorySelect?> GetSubCategoryAsync(InputSubCategorySelect input, CancellationToken ct = default) => _repo.GetSubCategoryAsync(input, ct);
    public async Task<OutputSubCategoryList> ListSubCategoriesAsync(InputSubCategoryList input, CancellationToken ct = default)
    {
        input.Page = Math.Max(1, input.Page);
        input.PageSize = Math.Clamp(input.PageSize <= 0 ? 25 : input.PageSize, 1, 100);
        input.SortBy = string.IsNullOrWhiteSpace(input.SortBy) ? "categoryname" : input.SortBy.Trim().ToLowerInvariant();
        input.Search = string.IsNullOrWhiteSpace(input.Search) ? null : input.Search.Trim();
        if (input.FkCategory is <= 0) input.FkCategory = null;

        var result = await _repo.ListSubCategoriesAsync(input, ct);
        if (result.Items.Count == 0 && result.TotalCount > 0 && input.Page > 1)
        {
            // Requested page is past the end (e.g. last row was just deactivated): fall back to the last real page.
            input.Page = (int)Math.Ceiling(result.TotalCount / (double)input.PageSize);
            result = await _repo.ListSubCategoriesAsync(input, ct);
        }

        result.Filter = input;
        return result;
    }
    public Task<OutputSubCategoryCreate> CreateSubCategoryAsync(InputSubCategoryCreate input, CancellationToken ct = default) => _repo.CreateSubCategoryAsync(input, ct);
    public Task<OutputSubCategoryUpdate> UpdateSubCategoryAsync(InputSubCategoryUpdate input, CancellationToken ct = default) => _repo.UpdateSubCategoryAsync(input, ct);

    public Task<OutputBrandSelect?> GetBrandAsync(InputBrandSelect input, CancellationToken ct = default) => _repo.GetBrandAsync(input, ct);
    public async Task<OutputBrandList> ListBrandsAsync(InputBrandList input, CancellationToken ct = default)
    {
        input.Page = Math.Max(1, input.Page);
        input.PageSize = Math.Clamp(input.PageSize <= 0 ? 25 : input.PageSize, 1, 100);
        input.SortBy = string.IsNullOrWhiteSpace(input.SortBy) ? "brandname" : input.SortBy.Trim().ToLowerInvariant();
        input.Search = string.IsNullOrWhiteSpace(input.Search) ? null : input.Search.Trim();

        var result = await _repo.ListBrandsAsync(input, ct);
        if (result.Items.Count == 0 && result.TotalCount > 0 && input.Page > 1)
        {
            // Requested page is past the end (e.g. last row was just deactivated): fall back to the last real page.
            input.Page = (int)Math.Ceiling(result.TotalCount / (double)input.PageSize);
            result = await _repo.ListBrandsAsync(input, ct);
        }

        result.Filter = input;
        return result;
    }
    public Task<OutputBrandCreate> CreateBrandAsync(InputBrandCreate input, CancellationToken ct = default) => _repo.CreateBrandAsync(input, ct);
    public Task<OutputBrandUpdate> UpdateBrandAsync(InputBrandUpdate input, CancellationToken ct = default) => _repo.UpdateBrandAsync(input, ct);

    public Task<OutputUnitSelect?> GetUnitAsync(InputUnitSelect input, CancellationToken ct = default) => _repo.GetUnitAsync(input, ct);
    public async Task<OutputUnitList> ListUnitsAsync(InputUnitList input, CancellationToken ct = default)
    {
        input.Page = Math.Max(1, input.Page);
        input.PageSize = Math.Clamp(input.PageSize <= 0 ? 25 : input.PageSize, 1, 100);
        input.SortBy = string.IsNullOrWhiteSpace(input.SortBy) ? "unitname" : input.SortBy.Trim().ToLowerInvariant();
        input.Search = string.IsNullOrWhiteSpace(input.Search) ? null : input.Search.Trim();

        var result = await _repo.ListUnitsAsync(input, ct);
        if (result.Items.Count == 0 && result.TotalCount > 0 && input.Page > 1)
        {
            // Requested page is past the end (e.g. last row was just deactivated): fall back to the last real page.
            input.Page = (int)Math.Ceiling(result.TotalCount / (double)input.PageSize);
            result = await _repo.ListUnitsAsync(input, ct);
        }

        result.Filter = input;
        return result;
    }
    public Task<OutputUnitCreate> CreateUnitAsync(InputUnitCreate input, CancellationToken ct = default) => _repo.CreateUnitAsync(input, ct);
    public Task<OutputUnitUpdate> UpdateUnitAsync(InputUnitUpdate input, CancellationToken ct = default) => _repo.UpdateUnitAsync(input, ct);

    public Task<OutputTaxSelect?> GetTaxAsync(InputTaxSelect input, CancellationToken ct = default) => _repo.GetTaxAsync(input, ct);
    public async Task<OutputTaxList> ListTaxesAsync(InputTaxList input, CancellationToken ct = default)
    {
        input.Page = Math.Max(1, input.Page);
        input.PageSize = Math.Clamp(input.PageSize <= 0 ? 25 : input.PageSize, 1, 100);
        input.SortBy = string.IsNullOrWhiteSpace(input.SortBy) ? "taxname" : input.SortBy.Trim().ToLowerInvariant();
        input.Search = string.IsNullOrWhiteSpace(input.Search) ? null : input.Search.Trim();

        var result = await _repo.ListTaxesAsync(input, ct);
        if (result.Items.Count == 0 && result.TotalCount > 0 && input.Page > 1)
        {
            // Requested page is past the end (e.g. last row was just deactivated): fall back to the last real page.
            input.Page = (int)Math.Ceiling(result.TotalCount / (double)input.PageSize);
            result = await _repo.ListTaxesAsync(input, ct);
        }

        result.Filter = input;
        return result;
    }
    public Task<OutputTaxCreate> CreateTaxAsync(InputTaxCreate input, CancellationToken ct = default) => _repo.CreateTaxAsync(input, ct);
    public Task<OutputTaxUpdate> UpdateTaxAsync(InputTaxUpdate input, CancellationToken ct = default) => _repo.UpdateTaxAsync(input, ct);

    public Task<IReadOnlyList<OutputCategorySelect>> GetCategoryLookupAsync(CancellationToken ct = default) => _repo.GetCategoryLookupAsync(ct);
    public Task<IReadOnlyList<OutputCategorySelect>> GetCategoryOptionsAsync(CancellationToken ct = default) => _repo.GetCategoryOptionsAsync(ct);
    public Task<IReadOnlyList<OutputSubCategorySelect>> GetSubCategoryLookupAsync(int fkCategory, CancellationToken ct = default) => _repo.GetSubCategoryLookupAsync(fkCategory, ct);
    public Task<IReadOnlyList<OutputBrandSelect>> GetBrandLookupAsync(CancellationToken ct = default) => _repo.GetBrandLookupAsync(ct);
    public Task<IReadOnlyList<OutputUnitSelect>> GetUnitLookupAsync(CancellationToken ct = default) => _repo.GetUnitLookupAsync(ct);
    public Task<IReadOnlyList<OutputTaxSelect>> GetTaxLookupAsync(CancellationToken ct = default) => _repo.GetTaxLookupAsync(ct);
}
