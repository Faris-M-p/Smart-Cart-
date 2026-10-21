using System.Globalization;

namespace SmartCartPro.Helpers;

/// <summary>
/// Stored timestamps are UTC. Views render them as
/// <c>&lt;time data-sc-utc="@TimeDisplay.Iso(x)" data-sc-format="datetime"&gt;fallback text&lt;/time&gt;</c>
/// and <c>sc-ajax.js</c> shows them in the user's own time zone.
/// </summary>
public static class TimeDisplay
{
    /// <summary>ISO-8601 UTC string (<c>2026-10-19T14:05:00Z</c>) for a stored UTC timestamp.</summary>
    public static string Iso(DateTime utc) =>
        DateTime.SpecifyKind(utc, DateTimeKind.Utc).ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);

    /// <summary>Nullable overload; returns an empty string for null so the attribute is simply ignored by the script.</summary>
    public static string Iso(DateTime? utc) => utc.HasValue ? Iso(utc.Value) : string.Empty;
}
