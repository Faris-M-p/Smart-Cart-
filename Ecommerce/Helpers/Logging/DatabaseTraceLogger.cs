using System;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Ecommerce.Helpers.Logging
{
    public static class DatabaseTraceLogger
    {
        private static readonly object _fileLock = new object();
        private static string _logFilePath;

        static DatabaseTraceLogger()
        {
            try
            {
                // Primary log directory under current working directory (project root during runtime)
                string currentDir = Directory.GetCurrentDirectory();
                string logsDir = Path.Combine(currentDir, "Logs");

                if (!Directory.Exists(logsDir))
                {
                    Directory.CreateDirectory(logsDir);
                }

                _logFilePath = Path.Combine(logsDir, "DatabaseTrace.txt");
            }
            catch
            {
                // Fallback to BaseDirectory if CurrentDirectory fails
                string baseDir = AppContext.BaseDirectory;
                string logsDir = Path.Combine(baseDir, "Logs");
                Directory.CreateDirectory(logsDir);
                _logFilePath = Path.Combine(logsDir, "DatabaseTrace.txt");
            }
        }

        public static string LogFilePath => _logFilePath;

        public static void LogCommand(
            DbCommand command, 
            long durationMs, 
            Exception exception = null, 
            string explicitModuleName = null)
        {
            if (command == null) return;

            try
            {
                bool isStoredProc = command.CommandType == CommandType.StoredProcedure;
                string commandText = command.CommandText?.Trim() ?? string.Empty;
                string moduleName = explicitModuleName ?? InferModuleName(commandText, command);
                bool isSuccess = exception == null;

                var sb = new StringBuilder();
                sb.AppendLine("============================================================");
                sb.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}]");
                sb.AppendLine($"MODULE      : {moduleName}");
                sb.AppendLine($"TYPE        : {(isStoredProc ? "STORED PROCEDURE" : "QUERY")}");

                if (isStoredProc)
                {
                    sb.AppendLine($"PROCEDURE   : {commandText}");
                    string paramString = FormatParametersInline(command.Parameters);
                    if (!string.IsNullOrEmpty(paramString))
                    {
                        sb.AppendLine($"PARAMETERS  : {paramString}");
                    }
                }

                sb.AppendLine($"STATUS      : {(isSuccess ? "SUCCESS" : "FAILED")}");
                sb.AppendLine($"DURATION    : {durationMs} ms");

                if (!isSuccess && exception != null)
                {
                    sb.AppendLine($"ERROR       : {exception.Message}");
                }

                sb.AppendLine("------------------------------------------------------------");
                sb.AppendLine("QUERY:");
                
                if (isStoredProc)
                {
                    sb.AppendLine(FormatStoredProcCall(commandText, command.Parameters));
                }
                else
                {
                    sb.AppendLine(commandText);
                    string formattedParams = FormatParametersMultiline(command.Parameters);
                    if (!string.IsNullOrEmpty(formattedParams))
                    {
                        sb.AppendLine("PARAMETERS:");
                        sb.AppendLine(formattedParams);
                    }
                }

                sb.AppendLine("============================================================");

                WriteToFile(sb.ToString());
            }
            catch
            {
                // Silent fail for logging errors to avoid breaking app flow
            }
        }

        public static void LogTrace(
            string queryOrProcedure,
            CommandType commandType,
            object parameters,
            long durationMs,
            Exception exception = null,
            string explicitModuleName = null)
        {
            try
            {
                bool isStoredProc = commandType == CommandType.StoredProcedure;
                string commandText = queryOrProcedure?.Trim() ?? string.Empty;
                string moduleName = explicitModuleName ?? InferModuleName(commandText, null);
                bool isSuccess = exception == null;

                var sb = new StringBuilder();
                sb.AppendLine("============================================================");
                sb.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}]");
                sb.AppendLine($"MODULE      : {moduleName}");
                sb.AppendLine($"TYPE        : {(isStoredProc ? "STORED PROCEDURE" : "QUERY")}");

                if (isStoredProc)
                {
                    sb.AppendLine($"PROCEDURE   : {commandText}");
                    string inlineParams = FormatObjectParametersInline(parameters);
                    if (!string.IsNullOrEmpty(inlineParams))
                    {
                        sb.AppendLine($"PARAMETERS  : {inlineParams}");
                    }
                }

                sb.AppendLine($"STATUS      : {(isSuccess ? "SUCCESS" : "FAILED")}");
                sb.AppendLine($"DURATION    : {durationMs} ms");

                if (!isSuccess && exception != null)
                {
                    sb.AppendLine($"ERROR       : {exception.Message}");
                }

                sb.AppendLine("------------------------------------------------------------");
                sb.AppendLine("QUERY:");

                if (isStoredProc)
                {
                    sb.AppendLine(FormatStoredProcCallFromObject(commandText, parameters));
                }
                else
                {
                    sb.AppendLine(commandText);
                    string multilineParams = FormatObjectParametersMultiline(parameters);
                    if (!string.IsNullOrEmpty(multilineParams))
                    {
                        sb.AppendLine("PARAMETERS:");
                        sb.AppendLine(multilineParams);
                    }
                }

                sb.AppendLine("============================================================");

                WriteToFile(sb.ToString());
            }
            catch
            {
                // Silent fail to guarantee zero application performance impact
            }
        }

        private static void WriteToFile(string content)
        {
            lock (_fileLock)
            {
                try
                {
                    File.AppendAllText(_logFilePath, content + Environment.NewLine, Encoding.UTF8);
                }
                catch
                {
                    // Fallback log location if permission issue occurs
                }
            }
        }

        private static string InferModuleName(string text, DbCommand command)
        {
            if (string.IsNullOrWhiteSpace(text)) return "Database";

            string lower = text.ToLowerInvariant();

            if (lower.Contains("productvariant") || lower.Contains("sku") || lower.Contains("getproductdetails")) return "ProductVariants";
            if (lower.Contains("product") || lower.Contains("getproducts")) return "Products";
            if (lower.Contains("cart") || lower.Contains("addcartitem") || lower.Contains("getbagcounts")) return "Cart";
            if (lower.Contains("wishlist") || lower.Contains("togglewishlist")) return "Wishlist";
            if (lower.Contains("order") || lower.Contains("placeorder") || lower.Contains("getadminorders")) return "Orders";
            if (lower.Contains("category") || lower.Contains("categories")) return "Categories";
            if (lower.Contains("subcategory") || lower.Contains("subcategories")) return "SubCategories";
            if (lower.Contains("brand")) return "Brands";
            if (lower.Contains("supplier")) return "Suppliers";
            if (lower.Contains("purchase")) return "Purchases";
            if (lower.Contains("stock") || lower.Contains("inventory")) return "Inventory";
            if (lower.Contains("userrole") || lower.Contains("permission") || lower.Contains("module")) return "UserRoles";
            if (lower.Contains("employee") || lower.Contains("adminuser")) return "Employees";
            if (lower.Contains("user") || lower.Contains("auth") || lower.Contains("login") || lower.Contains("register")) return "Auth";

            return "Database";
        }

        private static bool IsSensitiveParameter(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            string lower = name.ToLowerInvariant();
            return lower.Contains("password") ||
                   lower.Contains("secret") ||
                   lower.Contains("token") ||
                   lower.Contains("hash") ||
                   lower.Contains("pwd") ||
                   lower.Contains("connectionstring");
        }

        private static string FormatParametersInline(DbParameterCollection parameters)
        {
            if (parameters == null || parameters.Count == 0) return string.Empty;
            var parts = new List<string>();
            foreach (DbParameter p in parameters)
            {
                string name = p.ParameterName.TrimStart('@', ':');
                string valStr = IsSensitiveParameter(name) ? "\"********\"" : FormatValue(p.Value);
                parts.Add($"{name}={valStr}");
            }
            return string.Join(", ", parts);
        }

        private static string FormatParametersMultiline(DbParameterCollection parameters)
        {
            if (parameters == null || parameters.Count == 0) return string.Empty;
            var sb = new StringBuilder();
            foreach (DbParameter p in parameters)
            {
                string name = p.ParameterName.TrimStart('@', ':');
                string valStr = IsSensitiveParameter(name) ? "********" : FormatValue(p.Value);
                sb.AppendLine($"{name}={valStr}");
            }
            return sb.ToString().TrimEnd();
        }

        private static string FormatStoredProcCall(string procName, DbParameterCollection parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return $"EXEC {procName};";
            }

            var args = new List<string>();
            foreach (DbParameter p in parameters)
            {
                string name = p.ParameterName.TrimStart('@', ':');
                string valStr = IsSensitiveParameter(name) ? "'********'" : FormatValueForSql(p.Value);
                args.Add($"@{name}={valStr}");
            }
            return $"EXEC {procName} {string.Join(", ", args)};";
        }

        private static string FormatObjectParametersInline(object parameters)
        {
            if (parameters == null) return string.Empty;
            var props = parameters.GetType().GetProperties();
            if (props.Length == 0) return string.Empty;

            var parts = new List<string>();
            foreach (var prop in props)
            {
                string name = prop.Name;
                object? val = prop.GetValue(parameters);
                string valStr = IsSensitiveParameter(name) ? "\"********\"" : FormatValue(val);
                parts.Add($"{name}={valStr}");
            }
            return string.Join(", ", parts);
        }

        private static string FormatObjectParametersMultiline(object parameters)
        {
            if (parameters == null) return string.Empty;
            var props = parameters.GetType().GetProperties();
            if (props.Length == 0) return string.Empty;

            var sb = new StringBuilder();
            foreach (var prop in props)
            {
                string name = prop.Name;
                object? val = prop.GetValue(parameters);
                string valStr = IsSensitiveParameter(name) ? "********" : FormatValue(val);
                sb.AppendLine($"{name}={valStr}");
            }
            return sb.ToString().TrimEnd();
        }

        private static string FormatStoredProcCallFromObject(string procName, object parameters)
        {
            if (parameters == null) return $"EXEC {procName};";
            var props = parameters.GetType().GetProperties();
            if (props.Length == 0) return $"EXEC {procName};";

            var args = new List<string>();
            foreach (var prop in props)
            {
                string name = prop.Name;
                object? val = prop.GetValue(parameters);
                string valStr = IsSensitiveParameter(name) ? "'********'" : FormatValueForSql(val);
                args.Add($"@{name}={valStr}");
            }
            return $"EXEC {procName} {string.Join(", ", args)};";
        }

        private static string FormatValue(object? val)
        {
            if (val == null || val == DBNull.Value) return "NULL";
            if (val is string s) return $"\"{s}\"";
            if (val is DateTime dt) return $"\"{dt:yyyy-MM-dd HH:mm:ss}\"";
            if (val is bool b) return b ? "true" : "false";
            return val?.ToString() ?? "NULL";
        }

        private static string FormatValueForSql(object? val)
        {
            if (val == null || val == DBNull.Value) return "NULL";
            if (val is string s) return $"'{s.Replace("'", "''")}'";
            if (val is DateTime dt) return $"'{dt:yyyy-MM-dd HH:mm:ss}'";
            if (val is bool b) return b ? "1" : "0";
            return val?.ToString() ?? "NULL";
        }
    }
}
