using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Helpers.Logging;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Ecommerce.DataAccess
{
    public class EFDbCommandInterceptor : DbCommandInterceptor
    {
        public override DbDataReader ReaderExecuted(
            DbCommand command, 
            CommandExecutedEventData eventData, 
            DbDataReader result)
        {
            LogCommand(command, eventData, null);
            return base.ReaderExecuted(command, eventData, result);
        }

        public override async ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command, 
            CommandExecutedEventData eventData, 
            DbDataReader result, 
            CancellationToken cancellationToken = default)
        {
            LogCommand(command, eventData, null);
            return await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override int NonQueryExecuted(
            DbCommand command, 
            CommandExecutedEventData eventData, 
            int result)
        {
            LogCommand(command, eventData, null);
            return base.NonQueryExecuted(command, eventData, result);
        }

        public override async ValueTask<int> NonQueryExecutedAsync(
            DbCommand command, 
            CommandExecutedEventData eventData, 
            int result, 
            CancellationToken cancellationToken = default)
        {
            LogCommand(command, eventData, null);
            return await base.NonQueryExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override object ScalarExecuted(
            DbCommand command, 
            CommandExecutedEventData eventData, 
            object result)
        {
            LogCommand(command, eventData, null);
            return base.ScalarExecuted(command, eventData, result);
        }

        public override async ValueTask<object> ScalarExecutedAsync(
            DbCommand command, 
            CommandExecutedEventData eventData, 
            object result, 
            CancellationToken cancellationToken = default)
        {
            LogCommand(command, eventData, null);
            return await base.ScalarExecutedAsync(command, eventData, result, cancellationToken);
        }

        public override void CommandFailed(
            DbCommand command, 
            CommandErrorEventData eventData)
        {
            LogCommand(command, null, eventData);
            base.CommandFailed(command, eventData);
        }

        public override Task CommandFailedAsync(
            DbCommand command, 
            CommandErrorEventData eventData, 
            CancellationToken cancellationToken = default)
        {
            LogCommand(command, null, eventData);
            return base.CommandFailedAsync(command, eventData, cancellationToken);
        }

        private static void LogCommand(
            DbCommand command, 
            CommandExecutedEventData executedData, 
            CommandErrorEventData errorData)
        {
            long durationMs = 0;
            Exception ex = null;

            if (executedData != null)
            {
                durationMs = (long)executedData.Duration.TotalMilliseconds;
            }
            else if (errorData != null)
            {
                durationMs = (long)errorData.Duration.TotalMilliseconds;
                ex = errorData.Exception;
            }

            DatabaseTraceLogger.LogCommand(command, durationMs, ex);
        }
    }
}
