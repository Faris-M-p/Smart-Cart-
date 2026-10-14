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
                c.id_category AS CategoryId,
                c.categoryname AS CategoryName,
                COUNT(p.id_product)::int AS ProductCount
            FROM categories c
            LEFT JOIN products p
                ON p.fk_category = c.id_category
               AND p.isactive = TRUE
               AND p.cancelled = FALSE
            WHERE c.isactive = TRUE
              AND c.cancelled = FALSE
            GROUP BY c.id_category, c.categoryname
            ORDER BY c.categoryname;
            """;

        await using var connection = (NpgsqlConnection)CreateConnection();
        await connection.OpenAsync(cancellationToken);
        var rows = await connection.QueryAsync<ProductReportRow>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));
        return rows.ToList();
    }
}
