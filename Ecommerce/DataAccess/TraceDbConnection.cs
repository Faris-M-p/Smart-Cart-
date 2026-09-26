using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Helpers.Logging;

namespace Ecommerce.DataAccess
{
    public class TraceDbConnection : DbConnection
    {
        private readonly DbConnection _inner;

        public TraceDbConnection(DbConnection inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public DbConnection InnerConnection => _inner;

        public override string ConnectionString
        {
            get => _inner.ConnectionString;
            set => _inner.ConnectionString = value;
        }

        public override string Database => _inner.Database;
        public override string DataSource => _inner.DataSource;
        public override string ServerVersion => _inner.ServerVersion;
        public override ConnectionState State => _inner.State;

        public override void ChangeDatabase(string databaseName) => _inner.ChangeDatabase(databaseName);
        public override void Close() => _inner.Close();

        public override async Task CloseAsync()
        {
            await _inner.CloseAsync();
        }

        public override void Open() => _inner.Open();

        public override async Task OpenAsync(CancellationToken cancellationToken)
        {
            await _inner.OpenAsync(cancellationToken);
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return _inner.BeginTransaction(isolationLevel);
        }

        protected override ValueTask<DbTransaction> BeginDbTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken)
        {
            return _inner.BeginTransactionAsync(isolationLevel, cancellationToken);
        }

        protected override DbCommand CreateDbCommand()
        {
            return new TraceDbCommand(_inner.CreateCommand(), this);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class TraceDbCommand : DbCommand
    {
        private readonly DbCommand _inner;
        private readonly TraceDbConnection _connection;

        public TraceDbCommand(DbCommand inner, TraceDbConnection connection)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _connection = connection;
        }

        public override string CommandText
        {
            get => _inner.CommandText;
            set => _inner.CommandText = value;
        }

        public override int CommandTimeout
        {
            get => _inner.CommandTimeout;
            set => _inner.CommandTimeout = value;
        }

        public override CommandType CommandType
        {
            get => _inner.CommandType;
            set => _inner.CommandType = value;
        }

        protected override DbConnection DbConnection
        {
            get => _connection;
            set => _inner.Connection = (value is TraceDbConnection t) ? t.InnerConnection : value;
        }

        protected override DbParameterCollection DbParameterCollection => _inner.Parameters;

        protected override DbTransaction DbTransaction
        {
            get => _inner.Transaction;
            set => _inner.Transaction = value;
        }

        public override bool DesignTimeVisible
        {
            get => _inner.DesignTimeVisible;
            set => _inner.DesignTimeVisible = value;
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get => _inner.UpdatedRowSource;
            set => _inner.UpdatedRowSource = value;
        }

        public override void Cancel() => _inner.Cancel();

        public override int ExecuteNonQuery()
        {
            var sw = Stopwatch.StartNew();
            Exception ex = null;
            try
            {
                return _inner.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                ex = e;
                throw;
            }
            finally
            {
                sw.Stop();
                DatabaseTraceLogger.LogCommand(_inner, sw.ElapsedMilliseconds, ex);
            }
        }

        public override async Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            Exception ex = null;
            try
            {
                return await _inner.ExecuteNonQueryAsync(cancellationToken);
            }
            catch (Exception e)
            {
                ex = e;
                throw;
            }
            finally
            {
                sw.Stop();
                DatabaseTraceLogger.LogCommand(_inner, sw.ElapsedMilliseconds, ex);
            }
        }

        public override object ExecuteScalar()
        {
            var sw = Stopwatch.StartNew();
            Exception ex = null;
            try
            {
                return _inner.ExecuteScalar();
            }
            catch (Exception e)
            {
                ex = e;
                throw;
            }
            finally
            {
                sw.Stop();
                DatabaseTraceLogger.LogCommand(_inner, sw.ElapsedMilliseconds, ex);
            }
        }

        public override async Task<object> ExecuteScalarAsync(CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            Exception ex = null;
            try
            {
                return await _inner.ExecuteScalarAsync(cancellationToken);
            }
            catch (Exception e)
            {
                ex = e;
                throw;
            }
            finally
            {
                sw.Stop();
                DatabaseTraceLogger.LogCommand(_inner, sw.ElapsedMilliseconds, ex);
            }
        }

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            var sw = Stopwatch.StartNew();
            Exception ex = null;
            try
            {
                return _inner.ExecuteReader(behavior);
            }
            catch (Exception e)
            {
                ex = e;
                throw;
            }
            finally
            {
                sw.Stop();
                DatabaseTraceLogger.LogCommand(_inner, sw.ElapsedMilliseconds, ex);
            }
        }

        protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();
            Exception ex = null;
            try
            {
                return await _inner.ExecuteReaderAsync(behavior, cancellationToken);
            }
            catch (Exception e)
            {
                ex = e;
                throw;
            }
            finally
            {
                sw.Stop();
                DatabaseTraceLogger.LogCommand(_inner, sw.ElapsedMilliseconds, ex);
            }
        }

        public override void Prepare() => _inner.Prepare();

        protected override DbParameter CreateDbParameter() => _inner.CreateParameter();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inner.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
