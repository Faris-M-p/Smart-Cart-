using Ecommerce.Models;
using System.Data;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Interface
{
    public interface IDataAccessDapper
    {
       
        IDbConnection CreateConnection();

      
        Task<CommonModel.TableOutput<U>> GetMultipleListByStoredProcedure<U, T>(string storedProcedureName, T parameter);

        Task<CommonModel.TableOutput<U>> GetMultipleListByStoredProcedure<U>(string storedProcedureName, object parameter);

        Task<CommonModel.CommonResponse> ExecuteStoredProcedure<T>(string storedProcedureName, T parameter);

        Task<CommonModel.CommonResponse> ExecuteStoredProcedure(string storedProcedureName, object parameter);

    }
}
