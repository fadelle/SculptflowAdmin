namespace SculptFlowAdmin.Common.Statics;

/// <summary>The choices on the Errors pages (the main app accepts 1–8760 hours, these levels and sorts, up to 500 groups
/// and 200 rows per page).</summary>
public static class AppLogOptions
{
    public const int DefaultHours = 24;
    public const string DefaultLevel = "warning";
    public const string DefaultSort = "recent";
    public const int GroupTake = 200;
    public const int PageSize = 50;

    /// <summary>The dashboard tile's period.</summary>
    public const int SummaryHours = 24;

    public static readonly IReadOnlyList<(int Hours, string Label)> Periods =
    [
        (1, "Last hour"), (6, "Last 6 hours"), (24, "Last 24 hours"), (72, "Last 3 days"), (168, "Last 7 days"), (720, "Last 30 days"),
        (2160, "Last 90 days")
    ];

    public static readonly IReadOnlyList<(string Key, string Label)> Levels =
    [
        ("warning", "Warnings and errors"), ("error", "Errors only"), ("critical", "Critical only")
    ];

    public static readonly IReadOnlyList<(string Key, string Label)> Sorts =
    [
        ("recent", "Most recent"), ("count", "Most frequent")
    ];
}