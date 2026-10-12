using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly SmartCartDbContext _db;

    public ProductRepository(SmartCartDbContext db)
    {
        _db = db;
    }

    public Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        _db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Product>> ListAsync(CancellationToken cancellationToken = default) =>
        await _db.Products.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);

    public Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        _db.Products.Add(product);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Product product, CancellationToken cancellationToken = default)
    {
        _db.Products.Update(product);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Product product, CancellationToken cancellationToken = default)
    {
        _db.Products.Remove(product);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _db.SaveChangesAsync(cancellationToken);
}
