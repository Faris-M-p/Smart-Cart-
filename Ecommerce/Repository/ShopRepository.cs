using Ecommerce.Interface;
using static Ecommerce.Models.CommonModel;
using static Ecommerce.Models.ProductModel;

namespace Ecommerce.Repository
{
    
    public class ShopRepository:ShopInterface
    {

        private readonly IDataAccessDapper _dataAccessDapper;

        public ShopRepository(IDataAccessDapper dataAccessDapper)
        {
            _dataAccessDapper = dataAccessDapper;
        }

        public async Task<TableOutput<Product>> GetProductListAsync( InputProduct input)
        {
            try
            {
                // Calling the stored procedure with Dapper
                var output = await _dataAccessDapper.GetMultipleListByStoredProcedure<Product, InputProduct>(
                    storedProcedureName: "GetProducts", // Stored Procedure name
                    parameter: input // Parameters to pass to the stored procedure
                );

                return output;
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
            
                throw new Exception("An error occurred while fetching product data.");
            }
        }

    }
}
