using Ecommerce.Interface;
using System.Data;
using static Ecommerce.Models.CommonModel;
using Dapper;
using Microsoft.Data.SqlClient;
using Ecommerce.Models;
using Microsoft.Extensions.Configuration;

namespace Ecommerce.DataAccess
{
    public class DataAccessDapper : IDataAccessDapper
    {
        private readonly IConfiguration _configuration;

        // Hardcoded connection string name
        private readonly string _connectionStringName = "Ecommerse"; // Modify this to match your actual connection string name

        // Constructor to inject IConfiguration for accessing settings
        public DataAccessDapper(IConfiguration configuration)
        {
            _configuration = configuration;
        }
    
        public IDbConnection CreateConnection()
        {
            // Hardcoded connection string with the specified database name
            string connectionString = "Server=LAPTOP-E0RPFDLS\\\\SQLEXPRESS;Database=Ecommerse;Integrated Security=True;";

            // Check if the connection string is empty or null
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Connection string is empty or not defined.");
            }

            // Create and return a new SqlConnection with the hardcoded connection string
            return new SqlConnection(connectionString);
        }
        public IDbConnection GetCatalogDatabase()
        {
            string connectionString = _configuration.GetConnectionString(_connectionStringName);

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException($"Connection string '{_connectionStringName}' not found.");
            }

            return new SqlConnection(connectionString);
        }
        public async Task<TableOutput<U>> GetMultipleListByStoredProcedure<U, T>( string storedProcedureName, T parameter)
        {

            using (var connection = CreateConnection())
            {
                using (var multi = await connection.QueryMultipleAsync(storedProcedureName, parameter, commandType: CommandType.StoredProcedure))
                {
                    var result1 = (await multi.ReadAsync<U>()).ToList();
                    var result2 = (await multi.ReadAsync<TableOutput_Settings>()).SingleOrDefault();

                    var TableOutput = new TableOutput<U>
                    {
                        TableData = result1,
                        TableSettings = result2
                    };
                    return TableOutput;
                    //test
                }
            }



        }
        // Method to execute a stored procedure and return multiple result sets

    }
}
