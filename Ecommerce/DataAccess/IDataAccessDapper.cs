using System.Data;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.Interface
{
    /// <summary>
    /// PostgreSQL procedure-calling data access (user/storefront). Never functions; never EF.
    /// </summary>
    public interface IDataAccessDapper
    {
        IDbConnection CreateConnection();

        // 1) Status / ID only (OUT params)
        Task<U?> GetStatusByProcedure<U, T>(string procedureName, T parameter, CancellationToken cancellationToken = default);

        // 2) Single list / single row (one INOUT refcursor)
        Task<List<U>> GetListByProcedure<U, T>(string procedureName, T parameter, string cursorName = "p_result");
        Task<U?> GetSingleByProcedure<U, T>(string procedureName, T parameter, string cursorName = "p_result");

        // 3) Multiple lists (2–4 INOUT refcursors)
        Task<MultipleOutput<T1, T2>> GetMultipleListsByProcedure<T1, T2, P>(string procedureName, P parameter, string[] cursorNames);
        Task<MultipleOutput<T1, T2, T3>> GetMultipleListsByProcedure<T1, T2, T3, P>(string procedureName, P parameter, string[] cursorNames);
        Task<MultipleOutput<T1, T2, T3, T4>> GetMultipleListsByProcedure<T1, T2, T3, T4, P>(string procedureName, P parameter, string[] cursorNames);

        // 4) Info row + list
        Task<InfoList<I, L>> GetInfoAndListByProcedure<I, L, P>(string procedureName, P parameter, string cursorName = "p_result");

        // 5) Dynamic / unknown result sets
        Task<Dictionary<string, object>> GetDynamicResultSetsByProcedure<P>(
            string procedureName, P parameter, Dictionary<string, Type> cursorNameToType);

        /// <summary>
        /// Flexible N-cursor reader for cases with 5+ grids (e.g. product detail). Prefer typed overloads when possible.
        /// </summary>
        Task<TResult> QueryMultipleByProcedureCursorAsync<TParam, TResult>(
            string procedureName,
            TParam? parameter,
            string[] cursorNames,
            Func<ProcedureCursorReader, Task<TResult>> mapAsync);

        // 6) Raw ad-hoc SQL
        Task<IEnumerable<dynamic>> GetDataByQuery(string sqlQuery);
        Task<IEnumerable<dynamic>> GetDataByQuery(string sqlQuery, object parameter);
        Task<U?> GetDataByQuery<U>(string sqlQuery);
        Task<U?> GetDataByQuery<U, T>(string sqlQuery, T parameter);
        Task<List<U>> GetListByQuery<U>(string sqlQuery);
        Task<List<U>> GetListByQuery<U, T>(string sqlQuery, T parameter);

        // 7) Future / common cases
        Task ExecuteProcedureAsync<T>(string procedureName, T parameter, CancellationToken cancellationToken = default);
        Task<TScalar?> GetScalarByProcedure<TScalar, T>(string procedureName, T parameter, string outParamName = "p_result", CancellationToken cancellationToken = default);
        Task<TableOutput<U>> GetPagedListByProcedure<U, T>(string procedureName, T parameter, string cursorName = "p_result", CancellationToken cancellationToken = default);
        Task ExecuteInTransactionAsync(Func<IDbConnection, IDbTransaction, Task> operations);
        Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
        Task<List<U>> GetListByProcedureWithArrayParam<U, TArrayItem>(
            string procedureName,
            object namedScalarParameters,
            string arrayParamName,
            IEnumerable<TArrayItem> arrayValues,
            string cursorName = "p_result");
    }
}
