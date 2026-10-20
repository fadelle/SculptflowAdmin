using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Contracts.Services.Overview;
using Omni.Admin.Entities.Dtos.Clients;
using Omni.Admin.Entities.Dtos.Overview;
using Omni.Admin.Entities.Models;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Persistence.Contracts.Admins;

namespace Omni.Admin.Business.Services.Overview;

public class OverviewAdminService : IOverviewAdminService
{
    private readonly IOverviewApiClient _api;
    private readonly IAdminAuditRepository _audit;

    public OverviewAdminService(IOverviewApiClient api, IAdminAuditRepository audit)
    {
        _api = api;
        _audit = audit;
    }

    public Task<SystemTotals> TotalsAsync(CancellationToken ct = default) => _api.TotalsAsync(ct);

    public Task<List<DailyCount>> DailyMessagesAsync(int days = 14, CancellationToken ct = default) => _api.DailyMessagesAsync(days, ct);

    public Task<List<ProblemRow>> ProblemsAsync(CancellationToken ct = default) => _api.ProblemsAsync(ct);

    public Task<List<ClientRow>> RecentClientsAsync(int take, CancellationToken ct = default) => _api.RecentClientsAsync(take, ct);

    public Task<PagedResult<EventRow>> EventsAsync(Guid? clientId, string? eventType, int page, CancellationToken ct = default) =>
        _api.EventsAsync(clientId, eventType, page, ct);

    public Task<List<string>> EventTypesAsync(CancellationToken ct = default) => _api.EventTypesAsync(ct);

    public Task<PagedResult<AdminAuditEntry>> AuditAsync(Guid? clientId, string? search, int page, CancellationToken ct = default) =>
        _audit.ListAsync(clientId, search, page, ct);
}
