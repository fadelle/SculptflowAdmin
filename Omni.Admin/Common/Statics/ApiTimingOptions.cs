namespace Omni.Admin.Common.Statics;

/// <summary>The choices on the API timings page (the main app accepts 1–8760 hours, 1–500 rows and these sort keys).</summary>
public static class ApiTimingOptions
{
    public const int DefaultHours = 24;
    public const string DefaultSort = "slowest";
    public const int DefaultTake = 50;

    public static readonly IReadOnlyList<(int Hours, string Label)> Periods =
    [
        (1, "Last hour"), (6, "Last 6 hours"), (24, "Last 24 hours"), (72, "Last 3 days"), (168, "Last 7 days"), (720, "Last 30 days")
    ];

    public static readonly IReadOnlyList<(string Key, string Label)> Sorts =
    [
        ("slowest", "Slowest (average)"), ("p95", "Slowest (p95)"), ("max", "Slowest single request"), ("calls", "Most called"),
        ("time", "Most total time"), ("errors", "Most errors (5xx)"), ("slow", "Most slow requests")
    ];

    public static readonly IReadOnlyList<int> Takes = [25, 50, 100, 200];
}
