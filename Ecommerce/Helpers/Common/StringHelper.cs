using System.Text.Json;
using Ecommerce.Models;

namespace Ecommerce.Helpers.Common
{
    /// <summary>
    /// Shared string normalization and JSON filter parsing for repositories.
    /// </summary>
    public static class StringHelper
    {
        public static string? NormalizeOptionalString(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim();
        }

        /// <summary>Parses admin filter JSON arrays of <c>{"ID_Value":n}</c> (see <see cref="CommonInputArray"/>).</summary>
        public static List<int> ParseFilterIds(string? json)
        {
            if (string.IsNullOrWhiteSpace(json) ||
                string.Equals(json.Trim(), "[]", StringComparison.Ordinal))
            {
                return new List<int>();
            }

            try
            {
                var parsed = JsonSerializer.Deserialize<List<CommonModel.CommonInputArray>>(json);
                if (parsed == null)
                {
                    return new List<int>();
                }

                return parsed
                    .Select(x => x.ID)
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();
            }
            catch (JsonException)
            {
                return new List<int>();
            }
        }

        public static string? TruncateOptional(string? value, int maxLength)
        {
            var n = (value ?? string.Empty).Trim();
            if (n.Length == 0)
            {
                return null;
            }

            return n.Length <= maxLength ? n : n.Substring(0, maxLength);
        }
    }
}
