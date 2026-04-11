namespace Ecommerce.Helpers.Common
{
    /// <summary>
    /// Shared string normalization for repositories and services.
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
    }
}
