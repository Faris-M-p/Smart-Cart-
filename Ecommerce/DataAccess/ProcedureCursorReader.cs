using System.Data;
using Dapper;
using Npgsql;

namespace Ecommerce.Interface
{
    /// <summary>
    /// Reads FETCH ALL results for each cursor opened by a multi-refcursor procedure, in declaration order.
    /// </summary>
    public sealed class ProcedureCursorReader
    {
        private readonly NpgsqlConnection _connection;
        private readonly NpgsqlTransaction _transaction;
        private readonly string[] _cursorNames;
        private int _index;

        internal ProcedureCursorReader(
            NpgsqlConnection connection,
            NpgsqlTransaction transaction,
            string[] cursorNames)
        {
            _connection = connection;
            _transaction = transaction;
            _cursorNames = cursorNames;
        }

        public async Task<List<U>> ReadAsync<U>()
        {
            if (_index >= _cursorNames.Length)
            {
                throw new InvalidOperationException("No more procedure cursors to read.");
            }

            var cursorName = _cursorNames[_index++];
            return (await _connection.QueryAsync<U>(
                $"FETCH ALL FROM \"{cursorName}\"",
                transaction: _transaction)).ToList();
        }

        public async Task<U?> ReadFirstOrDefaultAsync<U>()
        {
            var rows = await ReadAsync<U>();
            return rows.FirstOrDefault();
        }
    }
}
