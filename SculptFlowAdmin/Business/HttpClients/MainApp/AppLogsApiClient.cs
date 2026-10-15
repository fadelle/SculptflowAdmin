using Microsoft.Extensions.Options;
using SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;
using SculptFlowAdmin.Common.Configs;
using SculptFlowAdmin.Entities.Responses.Monitoring;

namespace SculptFlowAdmin.Business.HttpClients.MainApp;

/// <summary>Client for the main app's platform-admin error log (/api/platform-admin/logs). Read-only.</summary>
public class AppLogsApiClient : MainAppApiClient, IAppLogsApiClient
{
    public const string Prefix = "api/platform-admin/logs";

    public AppLogsApiClient(HttpClient http, IOptions<MainAppApiOptions> options, IHttpContextAccessor context, ILogger<AppLogsApiClient> logger)
        : base(http, options, context, logger, Prefix, "Error log")
    {
    }

    public Task<AppLogGroupReport> GroupsAsync(int hours, string level, Guid? clinicId, string? search, string sort, int take, CancellationToken ct) =>
        GetRequiredAsync<AppLogGroupReport>("/groups" + Query(("hours", hours), ("level", level), ("clinicId", clinicId), ("search", search),
            ("sort", sort), ("take", take)), ct);

    public Task<AppLogPage> ListAsync(int hours, string level, Guid? clinicId, string? search, string? fingerprint, string? traceId, int page,
        int pageSize, CancellationToken ct) =>
        GetRequiredAsync<AppLogPage>(Query(("hours", hours), ("level", level), ("clinicId", clinicId), ("search", search),
            ("fingerprint", fingerprint), ("traceId", traceId), ("page", page), ("pageSize", pageSize)), ct);

    public Task<AppLogDetail?> GetAsync(long id, CancellationToken ct) =>
        GetAsync<AppLogDetail>("/" + id.ToString(System.Globalization.CultureInfo.InvariantCulture), ct);

    public Task<AppLogSummary> SummaryAsync(int hours, CancellationToken ct) =>
        GetRequiredAsync<AppLogSummary>("/summary" + Query(("hours", hours)), ct);
}