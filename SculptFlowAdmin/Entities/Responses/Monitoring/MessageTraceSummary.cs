namespace SculptFlowAdmin.Entities.Responses.Monitoring;

/// <summary>GET /api/platform-admin/message-traces/summary (copy of the main app's PlatformAdmin MessageTraceSummary).</summary>
public record MessageTraceSummary(int Hours, DateTimeOffset From, bool Enabled, long Traces, long FailedTraces, DateTimeOffset? LastFailedAt);