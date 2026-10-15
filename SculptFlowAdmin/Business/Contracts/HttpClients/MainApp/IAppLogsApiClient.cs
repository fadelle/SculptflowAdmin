using SculptFlowAdmin.Entities.Responses.Monitoring;

namespace SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;

public interface IAppLogsApiClient
{
    Task<AppLogGroupReport> GroupsAsync(int hours, string level, Guid? clinicId, string? search, string sort, int take, CancellationToken ct);

    Task<AppLogPage> ListAsync(int hours, string level, Guid? clinicId, string? search, string? fingerprint, string? traceId, int page,
        int pageSize, CancellationToken ct);

    /// <summary>Null when the row doesn't exist (or was purged).</summary>
    Task<AppLogDetail?> GetAsync(long id, CancellationToken ct);

    Task<AppLogSummary> SummaryAsync(int hours, CancellationToken ct);
}