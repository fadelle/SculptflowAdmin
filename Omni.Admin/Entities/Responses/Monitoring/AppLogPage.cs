using Omni.Admin.Entities.Dtos.Monitoring;

namespace Omni.Admin.Entities.Responses.Monitoring;

/// <summary>GET /api/platform-admin/logs (copy of the main app's PlatformAdmin AppLogPage): rows newest first.</summary>
public record AppLogPage(
    DateTimeOffset From,
    DateTimeOffset To,
    int Page,
    int PageSize,
    int Total,
    List<AppLogRow> Items);