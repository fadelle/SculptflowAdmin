using Microsoft.Extensions.Options;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Common.Configs;
using Omni.Admin.Entities.Responses.Monitoring;

namespace Omni.Admin.Business.HttpClients.MainApp;

/// <summary>Client for the main app's platform-admin API timings (/api/platform-admin/api-timings). Read-only.</summary>
public class ApiTimingsApiClient : MainAppApiClient, IApiTimingsApiClient
{
    public const string Prefix = "api/platform-admin/api-timings";

    public ApiTimingsApiClient(HttpClient http, IOptions<MainAppApiOptions> options, IHttpContextAccessor context, ILogger<ApiTimingsApiClient> logger)
        : base(http, options, context, logger, Prefix, "API timings")
    {
    }

    public Task<ApiTimingReport> ReportAsync(int hours, string sort, int take, CancellationToken ct) =>
        GetRequiredAsync<ApiTimingReport>(Query(("hours", hours), ("sort", sort), ("take", take)), ct);
}
