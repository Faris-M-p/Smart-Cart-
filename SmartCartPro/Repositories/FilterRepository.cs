using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Models;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

public class FilterRepository : IFilterRepository
{
    private readonly SmartCartDbContext _db;

    public FilterRepository(SmartCartDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<FillProduct>> GetProductsAsync(
        FilterOptions filter,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Products
            .AsNoTracking()
            .Where(p => !p.Cancelled && p.IsActive);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(p => p.ProductName.Contains(search));
        }

        return await query
            .OrderBy(p => p.ProductName)
            .Select(p => new FillProduct
            {
                Id_Value = p.IdProduct,
                Name = p.ProductName
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FillCategory>> GetCategoriesAsync(
        FilterOptions filter,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Categories
            .AsNoTracking()
            .Where(c => !c.Cancelled && c.IsActive);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(c => c.CategoryName.Contains(search));
        }

        return await query
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.CategoryName)
            .Select(c => new FillCategory
            {
                Id_Value = c.IdCategory,
                Name = c.CategoryName
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FillSubCategory>> GetSubCategoriesAsync(
        FilterOptions filter,
        CancellationToken cancellationToken = default)
    {
        var query = _db.SubCategories
            .AsNoTracking()
            .Where(s => !s.Cancelled && s.IsActive);

        if (filter.CategoryId is not null)
        {
            query = query.Where(s => s.FkCategory == filter.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            query = query.Where(s => s.SubCategoryName.Contains(search));
        }

        return await query
            .OrderBy(s => s.DisplayOrder)
            .ThenBy(s => s.SubCategoryName)
            .Select(s => new FillSubCategory
            {
                Id_Value = s.IdSubcategory,
                Name = s.SubCategoryName
            })
            .ToListAsync(cancellationToken);
    }
}
