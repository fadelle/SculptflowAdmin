namespace Omni.Admin.Common.Enums;

/// <summary>What a pipeline stage means, whatever a business type calls it — a copy of the main app's StageCategory
/// (every business type needs at least one stage in each). In pipeline order.</summary>
public static class StageCategory
{
    public const string Open = "open";
    public const string Booked = "booked";
    public const string Attended = "attended";
    public const string NoShow = "no_show";
    public const string Converted = "converted";
    public const string ClosedLost = "closed_lost";
    public const string NeedsHuman = "needs_human";

    public static readonly IReadOnlyList<string> All = [Open, Booked, Attended, NoShow, Converted, ClosedLost, NeedsHuman];
}
