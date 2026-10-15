using SculptFlowAdmin.Entities.Dtos.Monitoring;

namespace SculptFlowAdmin.Entities.Responses.Monitoring;

/// <summary>GET /api/platform-admin/message-traces (copy of the main app's PlatformAdmin MessageTracePage).</summary>
public record MessageTracePage(
    DateTimeOffset From,
    DateTimeOffset To,
    int Page,
    int PageSize,
    int Total,
    bool Enabled,
    int RetentionDays,
    int FailedRetentionDays,
    List<TraceSummaryRow> Items);