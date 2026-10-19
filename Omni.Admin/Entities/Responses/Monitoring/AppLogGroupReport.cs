using Omni.Admin.Entities.Dtos.Monitoring;

namespace Omni.Admin.Entities.Responses.Monitoring;

/// <summary>
/// GET /api/platform-admin/logs/groups (copy of the main app's PlatformAdmin AppLogGroupReport). Totals cover every
/// matching group; Groups is the first <c>take</c> after sorting. The ErrorLog settings say what is being captured.
/// </summary>
public record AppLogGroupReport(
    DateTimeOffset From,
    DateTimeOffset To,
    int Hours,
    string Level,
    string Sort,
    bool Enabled,
    string MinimumLevel,
    int WarningRetentionDays,
    int ErrorRetentionDays,
    long TotalOccurrences,
    int GroupCount,
    List<AppLogGroupRow> Groups);