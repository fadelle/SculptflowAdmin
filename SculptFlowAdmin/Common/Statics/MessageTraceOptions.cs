namespace SculptFlowAdmin.Common.Statics;

/// <summary>The choices on the Message traces pages (the main app accepts 1–8760 hours and up to 200 rows per page).</summary>
public static class MessageTraceOptions
{
    public const int DefaultHours = 24;
    public const int PageSize = 50;

    /// <summary>The dashboard tile's period.</summary>
    public const int SummaryHours = 24;

    public static readonly IReadOnlyList<(int Hours, string Label)> Periods =
    [
        (1, "Last hour"), (6, "Last 6 hours"), (24, "Last 24 hours"), (72, "Last 3 days"), (168, "Last 7 days"), (720, "Last 30 days"),
        (1440, "Last 60 days")
    ];
}