namespace SmartCartPro.Dapper.Interfaces;

/// <summary>
/// Optional Dapper access for justified reporting queries only.
/// Default CRUD stays on EF Core repositories.
/// </summary>
public interface IProductReportRepository
{
    Task<IReadOnlyList<ProductReportRow>> GetActiveProductCountsByCategoryAsync(
        CancellationToken cancellationToken = default);
}

public sealed class ProductReportRow
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int ProductCount { get; set; }
}
