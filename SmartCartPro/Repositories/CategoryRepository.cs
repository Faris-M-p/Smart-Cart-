using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly SmartCartDbContext _db;

    public CategoryRepository(SmartCartDbContext db)
    {
        _db = db;
    }

    public Task<Category?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _db.Categories.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Category>> ListAsync(CancellationToken cancellationToken = default) =>
        await _db.Categories.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public Task AddAsync(Category category, CancellationToken cancellationToken = default)
    {
        _db.Categories.Add(category);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Category category, CancellationToken cancellationToken = default)
    {
        _db.Categories.Update(category);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Category category, CancellationToken cancellationToken = default)
    {
        _db.Categories.Remove(category);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
