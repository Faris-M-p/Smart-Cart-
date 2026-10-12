using SmartCartPro.Entities;
using SmartCartPro.Repositories.Interfaces;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categories;

    public CategoryService(ICategoryRepository categories)
    {
        _categories = categories;
    }

    public Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _categories.GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<Category>> ListAsync(CancellationToken cancellationToken = default) =>
        _categories.ListAsync(cancellationToken);

    public async Task<Category> CreateAsync(Category category, CancellationToken cancellationToken = default)
    {
        await _categories.AddAsync(category, cancellationToken);
        await _categories.SaveChangesAsync(cancellationToken);
        return category;
    }
}
