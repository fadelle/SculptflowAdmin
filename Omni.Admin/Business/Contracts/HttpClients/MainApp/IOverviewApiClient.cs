using Omni.Admin.Entities.Dtos.Clients;
using Omni.Admin.Entities.Dtos.Overview;
using Omni.Admin.Entities.Responses.Paging;

namespace Omni.Admin.Business.Contracts.HttpClients.MainApp;

public interface IOverviewApiClient
{
    Task<SystemTotals> TotalsAsync(CancellationToken ct);

    Task<List<DailyCount>> DailyMessagesAsync(int days, CancellationToken ct);

    Task<List<ProblemRow>> ProblemsAsync(CancellationToken ct);

    Task<List<ClientRow>> RecentClientsAsync(int take, CancellationToken ct);

    Task<PagedResult<EventRow>> EventsAsync(Guid? clientId, string? eventType, int page, CancellationToken ct);

    /// <summary>The main app's own record of every write to its platform-admin API (QA t6-admin-23).</summary>
    Task<PagedResult<PlatformAdminWriteRow>> AdminWritesAsync(Guid? clientId, int page, CancellationToken ct);

    Task<List<string>> EventTypesAsync(CancellationToken ct);
}
