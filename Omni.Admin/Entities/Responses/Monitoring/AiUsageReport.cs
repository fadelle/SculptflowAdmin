using Omni.Admin.Entities.Dtos.Monitoring;

namespace Omni.Admin.Entities.Responses.Monitoring;

/// <summary>(Copy of the main app's PlatformAdmin AiUsageReport.) AI bot use per client over a period, with the token prices the
/// estimate used and how many days the trace keeps.</summary>
public record AiUsageReport(DateTimeOffset From, DateTimeOffset To, decimal InputTokenPriceUsdPerMillion, decimal OutputTokenPriceUsdPerMillion,
    int TraceRetentionDays, IReadOnlyList<AiUsageRow> Rows);
