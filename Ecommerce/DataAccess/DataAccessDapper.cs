using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Dapper;
using Ecommerce.Configuration;
using Ecommerce.Interface;
using Microsoft.Extensions.Options;
using Npgsql;
using static Ecommerce.Models.CommonModel;

namespace Ecommerce.DataAccess
{
    /// <summary>
    /// PostgreSQL implementation of <see cref="IDataAccessDapper"/>.
    /// Procedures only (CALL), never functions. OUT params for scalars/status;
    /// INOUT refcursor + FETCH for row sets. Input property names are mapped to
    /// <c>p_snake_case</c> to match Database-Postgres procedure signatures.
    /// </summary>
    public class DataAccessDapper : IDataAccessDapper
    {
        private readonly DatabaseSettings _databaseSettings;

        public DataAccessDapper(IOptions<DatabaseSettings> databaseSettings)
        {
            _databaseSettings = databaseSettings?.Value
                ?? throw new ArgumentException("Connection Settings not found.");
        }

        // ---------------------------------------------------------------
        // Connection
        // ---------------------------------------------------------------

        public IDbConnection CreateConnection()
        {
            if (_databaseSettings is null)
            {
                throw new ArgumentException("Connection Settings not found.");
            }

            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(_databaseSettings);
            bool isValid = Validator.TryValidateObject(
                _databaseSettings,
                validationContext,
                validationResults,
                validateAllProperties: true);

            if (!isValid)
            {
                throw new ArgumentException("Connection Settings not found.");
            }

            string connectionString =
                $"Host={_databaseSettings.ServerName};Database={_databaseSettings.DatabaseName};" +
                $"Username={_databaseSettings.UserId};Password={_databaseSettings.Password};" +
                $"Timeout={_databaseSettings.Timeout};SSL Mode=Prefer;Trust Server Certificate=true;";

            return new NpgsqlConnection(connectionString);
        }

        private async Task<NpgsqlConnection> OpenConnectionAsync()
        {
            var connection = (NpgsqlConnection)CreateConnection();
            await connection.OpenAsync();
            return connection;
        }

        /// <summary>
        /// Builds DynamicParameters from a plain input object's public properties
        /// (mapped to p_snake_case), then adds one InputOutput refcursor parameter
        /// per name in <paramref name="cursorNames"/>.
        /// </summary>
        private static DynamicParameters BuildCursorCallParameters<T>(T? parameter, IEnumerable<string> cursorNames)
        {
            var dbParameters = BuildProcedureParameters(parameter);
            foreach (var cursorName in cursorNames)
            {
                dbParameters.Add(cursorName, cursorName, DbType.String, ParameterDirection.InputOutput);
            }

            return dbParameters;
        }

        private static string BuildCallSql<T>(string procedureName, T? parameter, IEnumerable<string> cursorNames)
        {
            var inputParamNames = GetPropertyNames(parameter);
            var allParams = string.Join(", ", inputParamNames.Concat(cursorNames).Select(n => "@" + n));
            return string.IsNullOrEmpty(allParams)
                ? $"CALL {procedureName}()"
                : $"CALL {procedureName}({allParams})";
        }

        private static string BuildCallSqlInputsOnly<T>(string procedureName, T? parameter, params string[] extraParamNames)
        {
            var names = GetPropertyNames(parameter).Concat(extraParamNames).Select(n => "@" + n);
            var allParams = string.Join(", ", names);
            return string.IsNullOrEmpty(allParams)
                ? $"CALL {procedureName}()"
                : $"CALL {procedureName}({allParams})";
        }

        // ---------------------------------------------------------------
        // 1) STATUS / ID ONLY (OUT params, no rows)
        // ---------------------------------------------------------------

        public async Task<U?> GetStatusByProcedure<U, T>(string procedureName, T parameter, CancellationToken cancellationToken = default)
        {
            await using var connection = await OpenConnectionAsync();

            var dbParameters = BuildProcedureParameters(parameter);
            var callSql = BuildCallSqlInputsOnly(procedureName, parameter);
            var command = new CommandDefinition(callSql, dbParameters, cancellationToken: cancellationToken);
            var output = await connection.QueryAsync<U>(command);
            return output.FirstOrDefault();
        }

        // ---------------------------------------------------------------
        // 2) SINGLE LIST (one INOUT refcursor)
        // ---------------------------------------------------------------

        public async Task<List<U>> GetListByProcedure<U, T>(string procedureName, T parameter, string cursorName = "p_result")
        {
            await using var connection = await OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            var cursorNames = new[] { cursorName };
            var dbParameters = BuildCursorCallParameters(parameter, cursorNames);
            var callSql = BuildCallSql(procedureName, parameter, cursorNames);

            await connection.ExecuteAsync(callSql, dbParameters, transaction);

            var results = (await connection.QueryAsync<U>(
                $"FETCH ALL FROM \"{cursorName}\"",
                transaction: transaction)).ToList();

            await transaction.CommitAsync();
            return results;
        }

        public async Task<U?> GetSingleByProcedure<U, T>(string procedureName, T parameter, string cursorName = "p_result")
        {
            var list = await GetListByProcedure<U, T>(procedureName, parameter, cursorName);
            return list.FirstOrDefault();
        }

        // ---------------------------------------------------------------
        // 3) MULTIPLE LISTS (2–4 INOUT refcursors)
        // ---------------------------------------------------------------

        public async Task<MultipleOutput<T1, T2>> GetMultipleListsByProcedure<T1, T2, P>(
            string procedureName, P parameter, string[] cursorNames)
        {
            if (cursorNames.Length != 2)
            {
                throw new ArgumentException("Expected exactly 2 cursor names for this overload.", nameof(cursorNames));
            }

            await using var connection = await OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            var dbParameters = BuildCursorCallParameters(parameter, cursorNames);
            var callSql = BuildCallSql(procedureName, parameter, cursorNames);
            await connection.ExecuteAsync(callSql, dbParameters, transaction);

            var out1 = (await connection.QueryAsync<T1>($"FETCH ALL FROM \"{cursorNames[0]}\"", transaction: transaction)).ToList();
            var out2 = (await connection.QueryAsync<T2>($"FETCH ALL FROM \"{cursorNames[1]}\"", transaction: transaction)).ToList();

            await transaction.CommitAsync();
            return new MultipleOutput<T1, T2> { TableOut1 = out1, TableOut2 = out2 };
        }

        public async Task<MultipleOutput<T1, T2, T3>> GetMultipleListsByProcedure<T1, T2, T3, P>(
            string procedureName, P parameter, string[] cursorNames)
        {
            if (cursorNames.Length != 3)
            {
                throw new ArgumentException("Expected exactly 3 cursor names for this overload.", nameof(cursorNames));
            }

            await using var connection = await OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            var dbParameters = BuildCursorCallParameters(parameter, cursorNames);
            var callSql = BuildCallSql(procedureName, parameter, cursorNames);
            await connection.ExecuteAsync(callSql, dbParameters, transaction);

            var out1 = (await connection.QueryAsync<T1>($"FETCH ALL FROM \"{cursorNames[0]}\"", transaction: transaction)).ToList();
            var out2 = (await connection.QueryAsync<T2>($"FETCH ALL FROM \"{cursorNames[1]}\"", transaction: transaction)).ToList();
            var out3 = (await connection.QueryAsync<T3>($"FETCH ALL FROM \"{cursorNames[2]}\"", transaction: transaction)).ToList();

            await transaction.CommitAsync();
            return new MultipleOutput<T1, T2, T3> { TableOut1 = out1, TableOut2 = out2, TableOut3 = out3 };
        }

        public async Task<MultipleOutput<T1, T2, T3, T4>> GetMultipleListsByProcedure<T1, T2, T3, T4, P>(
            string procedureName, P parameter, string[] cursorNames)
        {
            if (cursorNames.Length != 4)
            {
                throw new ArgumentException("Expected exactly 4 cursor names for this overload.", nameof(cursorNames));
            }

            await using var connection = await OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            var dbParameters = BuildCursorCallParameters(parameter, cursorNames);
            var callSql = BuildCallSql(procedureName, parameter, cursorNames);
            await connection.ExecuteAsync(callSql, dbParameters, transaction);

            var out1 = (await connection.QueryAsync<T1>($"FETCH ALL FROM \"{cursorNames[0]}\"", transaction: transaction)).ToList();
            var out2 = (await connection.QueryAsync<T2>($"FETCH ALL FROM \"{cursorNames[1]}\"", transaction: transaction)).ToList();
            var out3 = (await connection.QueryAsync<T3>($"FETCH ALL FROM \"{cursorNames[2]}\"", transaction: transaction)).ToList();
            var out4 = (await connection.QueryAsync<T4>($"FETCH ALL FROM \"{cursorNames[3]}\"", transaction: transaction)).ToList();

            await transaction.CommitAsync();
            return new MultipleOutput<T1, T2, T3, T4> { TableOut1 = out1, TableOut2 = out2, TableOut3 = out3, TableOut4 = out4 };
        }

        // ---------------------------------------------------------------
        // 4) ONE INFO ROW + ONE LIST
        // ---------------------------------------------------------------

        public async Task<InfoList<I, L>> GetInfoAndListByProcedure<I, L, P>(
            string procedureName, P parameter, string cursorName = "p_result")
        {
            await using var connection = await OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            var cursorNames = new[] { cursorName };
            var dbParameters = BuildCursorCallParameters(parameter, cursorNames);
            var callSql = BuildCallSql(procedureName, parameter, cursorNames);

            // Info columns come back as OUT params on the same CALL when the
            // procedure declares them; otherwise ListInfo may be default.
            var infoRow = (await connection.QueryAsync<I>(callSql, dbParameters, transaction)).SingleOrDefault();
            var listData = (await connection.QueryAsync<L>(
                $"FETCH ALL FROM \"{cursorName}\"",
                transaction: transaction)).ToList();

            await transaction.CommitAsync();
            return new InfoList<I, L> { ListInfo = infoRow, ListData = listData };
        }

        // ---------------------------------------------------------------
        // 5) DYNAMIC / UNKNOWN NUMBER OF RESULT SETS
        // ---------------------------------------------------------------

        public async Task<Dictionary<string, object>> GetDynamicResultSetsByProcedure<P>(
            string procedureName, P parameter, Dictionary<string, Type> cursorNameToType)
        {
            var result = new Dictionary<string, object>();

            await using var connection = await OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            var cursorNames = cursorNameToType.Keys.ToArray();
            var dbParameters = BuildCursorCallParameters(parameter, cursorNames);
            var callSql = BuildCallSql(procedureName, parameter, cursorNames);
            await connection.ExecuteAsync(callSql, dbParameters, transaction);

            foreach (var (cursorName, expectedType) in cursorNameToType)
            {
                try
                {
                    var rows = (await connection.QueryAsync(
                        $"FETCH ALL FROM \"{cursorName}\"",
                        transaction: transaction)).ToList();

                    var json = JsonSerializer.Serialize(rows);
                    var listType = typeof(List<>).MakeGenericType(expectedType);
                    var typedList = JsonSerializer.Deserialize(json, listType)
                                    ?? Activator.CreateInstance(listType)!;
                    result[cursorName] = typedList;
                }
                catch
                {
                    // Skip a cursor that couldn't be read/mapped (legacy continue-on-error).
                    continue;
                }
            }

            await transaction.CommitAsync();
            return result;
        }

        public async Task<TResult> QueryMultipleByProcedureCursorAsync<TParam, TResult>(
            string procedureName,
            TParam? parameter,
            string[] cursorNames,
            Func<ProcedureCursorReader, Task<TResult>> mapAsync)
        {
            if (cursorNames == null || cursorNames.Length == 0)
            {
                throw new ArgumentException("At least one cursor name is required.", nameof(cursorNames));
            }

            await using var connection = await OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            var dbParameters = BuildCursorCallParameters(parameter, cursorNames);
            var callSql = BuildCallSql(procedureName, parameter, cursorNames);
            await connection.ExecuteAsync(callSql, dbParameters, transaction);

            var reader = new ProcedureCursorReader(connection, transaction, cursorNames);
            var result = await mapAsync(reader);
            await transaction.CommitAsync();
            return result;
        }

        // ---------------------------------------------------------------
        // 6) RAW AD-HOC SQL (no procedure)
        // ---------------------------------------------------------------

        public async Task<IEnumerable<dynamic>> GetDataByQuery(string sqlQuery)
        {
            await using var connection = await OpenConnectionAsync();
            return await connection.QueryAsync(sqlQuery);
        }

        public async Task<IEnumerable<dynamic>> GetDataByQuery(string sqlQuery, object parameter)
        {
            await using var connection = await OpenConnectionAsync();
            return await connection.QueryAsync(sqlQuery, parameter);
        }

        public async Task<U?> GetDataByQuery<U>(string sqlQuery)
        {
            await using var connection = await OpenConnectionAsync();
            var output = await connection.QueryAsync<U>(sqlQuery);
            return output.FirstOrDefault();
        }

        public async Task<U?> GetDataByQuery<U, T>(string sqlQuery, T parameter)
        {
            await using var connection = await OpenConnectionAsync();
            var output = await connection.QueryAsync<U>(sqlQuery, parameter);
            return output.FirstOrDefault();
        }

        public async Task<List<U>> GetListByQuery<U>(string sqlQuery)
        {
            await using var connection = await OpenConnectionAsync();
            var output = await connection.QueryAsync<U>(sqlQuery);
            return output.ToList();
        }

        public async Task<List<U>> GetListByQuery<U, T>(string sqlQuery, T parameter)
        {
            await using var connection = await OpenConnectionAsync();
            var output = await connection.QueryAsync<U>(sqlQuery, parameter);
            return output.ToList();
        }

        // ---------------------------------------------------------------
        // 7) FUTURE / COMMON CASES
        // ---------------------------------------------------------------

        public async Task ExecuteProcedureAsync<T>(string procedureName, T parameter, CancellationToken cancellationToken = default)
        {
            await using var connection = await OpenConnectionAsync();
            var dbParameters = BuildProcedureParameters(parameter);
            var callSql = BuildCallSqlInputsOnly(procedureName, parameter);
            var command = new CommandDefinition(callSql, dbParameters, cancellationToken: cancellationToken);
            await connection.ExecuteAsync(command);
        }

        public async Task<TScalar?> GetScalarByProcedure<TScalar, T>(
            string procedureName,
            T parameter,
            string outParamName = "p_result",
            CancellationToken cancellationToken = default)
        {
            await using var connection = await OpenConnectionAsync();

            var dbParameters = BuildProcedureParameters(parameter);
            dbParameters.Add(
                outParamName,
                dbType: InferDbType(typeof(TScalar)),
                direction: ParameterDirection.Output);

            var callSql = BuildCallSqlInputsOnly(procedureName, parameter, outParamName);
            var command = new CommandDefinition(callSql, dbParameters, cancellationToken: cancellationToken);
            await connection.ExecuteAsync(command);

            return dbParameters.Get<TScalar>(outParamName);
        }

        public async Task<TableOutput<U>> GetPagedListByProcedure<U, T>(
            string procedureName,
            T parameter,
            string cursorName = "p_result",
            CancellationToken cancellationToken = default)
        {
            await using var connection = await OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            var cursorNames = new[] { cursorName };
            var dbParameters = BuildCursorCallParameters(parameter, cursorNames);
            var callSql = BuildCallSql(procedureName, parameter, cursorNames);

            var command = new CommandDefinition(callSql, dbParameters, transaction: transaction, cancellationToken: cancellationToken);
            var settings = (await connection.QueryAsync<TableOutput_Settings>(command)).SingleOrDefault();
            var data = (await connection.QueryAsync<U>(
                $"FETCH ALL FROM \"{cursorName}\"",
                transaction: transaction)).ToList();

            await transaction.CommitAsync();
            return new TableOutput<U> { TableData = data, TableSettings = settings };
        }

        public async Task ExecuteInTransactionAsync(Func<IDbConnection, IDbTransaction, Task> operations)
        {
            await using var connection = await OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                await operations(connection, transaction);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await using var connection = await OpenConnectionAsync();
                var command = new CommandDefinition("SELECT 1", cancellationToken: cancellationToken);
                await connection.ExecuteScalarAsync<int>(command);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<U>> GetListByProcedureWithArrayParam<U, TArrayItem>(
            string procedureName,
            object namedScalarParameters,
            string arrayParamName,
            IEnumerable<TArrayItem> arrayValues,
            string cursorName = "p_result")
        {
            await using var connection = await OpenConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            var dbParameters = BuildProcedureParameters(namedScalarParameters);
            var pgArrayName = arrayParamName.StartsWith("p_", StringComparison.Ordinal)
                ? arrayParamName
                : ToPgParameterName(arrayParamName);
            dbParameters.Add(pgArrayName, arrayValues.ToArray());
            dbParameters.Add(cursorName, cursorName, DbType.String, ParameterDirection.InputOutput);

            var scalarParamNames = GetPropertyNames(namedScalarParameters);
            var callSql =
                $"CALL {procedureName}({string.Join(", ", scalarParamNames.Concat(new[] { pgArrayName, cursorName }).Select(n => "@" + n))})";

            await connection.ExecuteAsync(callSql, dbParameters, transaction);
            var results = (await connection.QueryAsync<U>(
                $"FETCH ALL FROM \"{cursorName}\"",
                transaction: transaction)).ToList();

            await transaction.CommitAsync();
            return results;
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        private static DbType InferDbType(Type type)
        {
            var t = Nullable.GetUnderlyingType(type) ?? type;

            if (t == typeof(int) || t == typeof(int?)) return DbType.Int32;
            if (t == typeof(long) || t == typeof(long?)) return DbType.Int64;
            if (t == typeof(short) || t == typeof(short?)) return DbType.Int16;
            if (t == typeof(bool) || t == typeof(bool?)) return DbType.Boolean;
            if (t == typeof(string)) return DbType.String;
            if (t == typeof(decimal) || t == typeof(decimal?)) return DbType.Decimal;
            if (t == typeof(double) || t == typeof(double?)) return DbType.Double;
            if (t == typeof(float) || t == typeof(float?)) return DbType.Single;
            if (t == typeof(DateTime) || t == typeof(DateTime?)) return DbType.DateTime;
            if (t == typeof(DateTimeOffset) || t == typeof(DateTimeOffset?)) return DbType.DateTimeOffset;
            if (t == typeof(Guid) || t == typeof(Guid?)) return DbType.Guid;
            if (t == typeof(byte[])) return DbType.Binary;

            return DbType.Object;
        }

        private static DynamicParameters BuildProcedureParameters<T>(T? parameter)
        {
            var paramList = new DynamicParameters();
            if (parameter == null)
            {
                return paramList;
            }

            foreach (var prop in parameter.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!prop.CanRead)
                {
                    continue;
                }

                paramList.Add(ToPgParameterName(prop.Name), prop.GetValue(parameter));
            }

            return paramList;
        }

        private static IEnumerable<string> GetPropertyNames<T>(T? parameter)
        {
            if (parameter == null)
            {
                return Array.Empty<string>();
            }

            return parameter.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead)
                .Select(p => ToPgParameterName(p.Name));
        }

        private static string ToPgParameterName(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return propertyName;
            }

            if (propertyName.StartsWith("p_", StringComparison.OrdinalIgnoreCase)
                && propertyName.Contains('_', StringComparison.Ordinal))
            {
                return propertyName.ToLowerInvariant();
            }

            var sb = new StringBuilder();
            sb.Append('p').Append('_');
            for (var i = 0; i < propertyName.Length; i++)
            {
                var c = propertyName[i];
                if (char.IsUpper(c) && i > 0)
                {
                    sb.Append('_');
                }

                sb.Append(char.ToLowerInvariant(c));
            }

            return sb.ToString();
        }
    }
}
