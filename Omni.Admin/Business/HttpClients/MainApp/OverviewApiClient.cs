using Microsoft.Extensions.Options;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Common.Configs;
using Omni.Admin.Entities.Dtos.Clients;
using Omni.Admin.Entities.Dtos.Overview;
using Omni.Admin.Entities.Responses.Paging;

namespace Omni.Admin.Business.HttpClients.MainApp;

/// <summary>Client for the main app's platform-admin overview API (/api/platform-admin/overview): dashboard totals,
/// problems and the event log.</summary>
public class OverviewApiClient : MainAppApiClient, IOverviewApiClient
{
    public const string Prefix = "api/platform-admin/overview";

    public OverviewApiClient(HttpClient http, IOptions<MainAppApiOptions> options, IHttpContextAccessor context, ILogger<OverviewApiClient> logger)
        : base(http, options, context, logger, Prefix, "Overview")
    {
    }

    public Task<SystemTotals> TotalsAsync(CancellationToken ct) => GetRequiredAsync<SystemTotals>("/totals", ct);

    public Task<List<DailyCount>> DailyMessagesAsync(int days, CancellationToken ct) =>
        GetRequiredAsync<List<DailyCount>>("/daily-messages" + Query(("days", days)), ct);

    public Task<List<ProblemRow>> ProblemsAsync(CancellationToken ct) => GetRequiredAsync<List<ProblemRow>>("/problems", ct);

    public Task<List<ClientRow>> RecentClientsAsync(int take, CancellationToken ct) =>
        GetRequiredAsync<List<ClientRow>>("/recent-clients" + Query(("take", take)), ct);

    public Task<PagedResult<EventRow>> EventsAsync(Guid? clientId, string? eventType, int page, CancellationToken ct) =>
        GetRequiredAsync<PagedResult<EventRow>>("/events" + Query(("clientId", clientId), ("eventType", eventType), ("page", page)), ct);

    public Task<List<string>> EventTypesAsync(CancellationToken ct) => GetRequiredAsync<List<string>>("/event-types", ct);
}
