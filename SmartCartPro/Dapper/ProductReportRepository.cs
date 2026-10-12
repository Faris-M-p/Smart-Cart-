using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using SmartCartPro.Dapper.Interfaces;

namespace SmartCartPro.Dapper;

public class ProductReportRepository : IProductReportRepository
{
    private readonly string _connectionString;

    public ProductReportRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    }

    private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

    public async Task<IReadOnlyList<ProductReportRow>> GetActiveProductCountsByCategoryAsync(
        CancellationToken cancellationToken = default)
    {
        // Justified report-style aggregate query via Dapper (no EF migration / no SP generation).
        const string sql = """
            SELECT
                c.id AS CategoryId,
                c.name AS CategoryName,
                COUNT(p.id)::int AS ProductCount
            FROM categories c
            LEFT JOIN products p ON p.category_id = c.id AND p.is_active = TRUE
            WHERE c.is_active = TRUE
            GROUP BY c.id, c.name
            ORDER BY c.name;
            """;

        await using var connection = (NpgsqlConnection)CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var rows = await connection.QueryAsync<ProductReportRow>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToList();
    }
}
