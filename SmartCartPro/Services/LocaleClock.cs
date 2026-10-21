using System.Globalization;

namespace SmartCartPro.Services;

/// <summary>
/// Company locale clock: IANA timezone with UTC fallback, business-day bounds, and report range caps.
/// Used by report and dashboard services; never throws on an unknown zone id.
/// </summary>
internal static class LocaleClock
{
    public const string DefaultTimeZone = "Asia/Kolkata";
    public const string DefaultCurrency = "INR";
    public const int MaxRangeDays = 366;

    public static TimeZoneInfo ResolveTimeZone(string? iana)
    {
        var id = string.IsNullOrWhiteSpace(iana) ? DefaultTimeZone : iana.Trim();
        if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var tz))
            return tz;
        if (TimeZoneInfo.TryFindSystemTimeZoneById(DefaultTimeZone, out tz))
            return tz;
        return TimeZoneInfo.Utc;
    }

    public static string ResolveCurrency(string? code)
    {
        var value = string.IsNullOrWhiteSpace(code) ? DefaultCurrency : code.Trim().ToUpperInvariant();
        return value.Length == 3 ? value : DefaultCurrency;
    }

    public static DateTime ToUtc(DateTime unspecifiedLocal, TimeZoneInfo tz)
    {
        var local = DateTime.SpecifyKind(unspecifiedLocal, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(local, tz);
    }

    public static DateTime ToLocal(DateTime utc, TimeZoneInfo tz)
    {
        var value = utc.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(utc, DateTimeKind.Utc)
            : utc.ToUniversalTime();
        return TimeZoneInfo.ConvertTimeFromUtc(value, tz);
    }

    public static DateOnly LocalToday(TimeZoneInfo tz) =>
        DateOnly.FromDateTime(ToLocal(DateTime.UtcNow, tz));

    public static (DateTime FromUtc, DateTime ToUtcExclusive) DayBounds(DateOnly day, TimeZoneInfo tz)
    {
        var fromUtc = ToUtc(day.ToDateTime(TimeOnly.MinValue), tz);
        var toUtc = ToUtc(day.AddDays(1).ToDateTime(TimeOnly.MinValue), tz);
        return (fromUtc, toUtc);
    }

    public static (DateTime FromUtc, DateTime ToUtcExclusive, DateOnly FromLocal, DateOnly ToLocal)
        ResolveInclusiveRange(DateTime? from, DateTime? to, TimeZoneInfo tz)
    {
        var today = LocalToday(tz);
        var fromDate = from is { } f && f != default ? DateOnly.FromDateTime(f) : new DateOnly(today.Year, today.Month, 1);
        var toDate = to is { } t && t != default ? DateOnly.FromDateTime(t) : today;

        if (toDate < fromDate)
        {
            (fromDate, toDate) = (toDate, fromDate);
        }

        if (toDate.DayNumber - fromDate.DayNumber > MaxRangeDays)
        {
            toDate = fromDate.AddDays(MaxRangeDays);
        }

        var fromUtc = ToUtc(fromDate.ToDateTime(TimeOnly.MinValue), tz);
        var toUtcExclusive = ToUtc(toDate.AddDays(1).ToDateTime(TimeOnly.MinValue), tz);
        return (fromUtc, toUtcExclusive, fromDate, toDate);
    }

    public static string Money(string currency, decimal amount) =>
        currency + " " + amount.ToString("N2", CultureInfo.InvariantCulture);

    public static string Number(decimal amount) =>
        amount.ToString("N2", CultureInfo.InvariantCulture);
}
