using SculptFlowAdmin.Entities.Dtos.Monitoring;

namespace SculptFlowAdmin.Entities.Responses.Monitoring;

/// <summary>
/// GET /api/platform-admin/api-timings (copy of the main app's PlatformAdmin ApiTimingReport). Totals cover every route;
/// Routes is the first <c>take</c> after sorting. Enabled, SlowRequestMs and RetentionDays are the main app's current
/// Monitoring settings.
/// </summary>
public record ApiTimingReport(
    DateTimeOffset From,
    DateTimeOffset To,
    int Hours,
    string Sort,
    bool Enabled,
    int SlowRequestMs,
    int RetentionDays,
    long TotalRequests,
    long TotalErrors,
    long TotalSlowRequests,
    int RouteCount,
    List<RouteTimingRow> Routes);
