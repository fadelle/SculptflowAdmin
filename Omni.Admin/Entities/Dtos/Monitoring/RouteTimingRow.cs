namespace Omni.Admin.Entities.Dtos.Monitoring;

/// <summary>
/// One method + route over the chosen period (copy of the main app's PlatformAdmin RouteTimingRow). AvgMs and MaxMs are
/// exact; P95Ms is the request-weighted average of the per-minute p95s. TotalSeconds = time the server spent on it.
/// </summary>
public record RouteTimingRow(
    string Method,
    string Route,
    long Requests,
    long Errors,
    long ClientErrors,
    long SlowRequests,
    double AvgMs,
    double P95Ms,
    double MaxMs,
    double TotalSeconds,
    DateTimeOffset LastSeenAt);
