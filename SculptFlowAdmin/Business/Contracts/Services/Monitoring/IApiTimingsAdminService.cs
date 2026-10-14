using SculptFlowAdmin.Entities.Responses.Monitoring;

namespace SculptFlowAdmin.Business.Contracts.Services.Monitoring;

/// <summary>The main app's per-route API timings, through its platform-admin API. Read-only, so nothing is audited.</summary>
public interface IApiTimingsAdminService
{
    /// <summary>Routes seen in the last <paramref name="hours"/> hours, sorted by <paramref name="sort"/>
    /// (slowest, p95, max, calls, errors, slow, time), the first <paramref name="take"/>.</summary>
    Task<ApiTimingReport> ReportAsync(int hours, string sort, int take, CancellationToken ct);
}
