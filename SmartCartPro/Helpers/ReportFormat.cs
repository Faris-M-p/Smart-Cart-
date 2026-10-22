using System.Globalization;

namespace SmartCartPro.Helpers;

public static class ReportFormat
{
    private static readonly CultureInfo Indian = CultureInfo.GetCultureInfo("en-IN");

    public static string Money(decimal value) => "₹" + value.ToString("N2", Indian);

    public static string Qty(decimal value) => value.ToString("0.###", Indian);

    public static string Count(int value) => value.ToString("N0", Indian);

    public static string Pct(decimal value) => value.ToString("N2", Indian) + "%";

    public static string When(DateTime local) => local.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture);

    public static string Day(DateTime local) => local.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

    public static string Day(DateOnly day) => day.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

    public static string Text(string? value) => string.IsNullOrWhiteSpace(value) ? "—" : value.Trim();
}
