namespace Omni.Admin.Entities.Responses.Monitoring;

/// <summary>GET /api/platform-admin/logs/summary (copy of the main app's PlatformAdmin AppLogSummary): occurrences per
/// level in the last Hours hours, for the dashboard tile.</summary>
public record AppLogSummary(
    int Hours,
    DateTimeOffset From,
    bool Enabled,
    long Warnings,
    long Errors,
    long Criticals,
    DateTimeOffset? LastErrorAt);