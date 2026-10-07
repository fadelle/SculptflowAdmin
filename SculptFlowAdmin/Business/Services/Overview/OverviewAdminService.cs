using SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;
using SculptFlowAdmin.Business.Contracts.Services.Overview;
using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Dtos.Overview;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Persistence.Contracts.Admins;

namespace SculptFlowAdmin.Business.Services.Overview;

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

    public Task<List<ClinicRow>> RecentClinicsAsync(int take, CancellationToken ct = default) => _api.RecentClinicsAsync(take, ct);

    public Task<PagedResult<EventRow>> EventsAsync(Guid? clinicId, string? eventType, int page, CancellationToken ct = default) =>
        _api.EventsAsync(clinicId, eventType, page, ct);

    public Task<List<string>> EventTypesAsync(CancellationToken ct = default) => _api.EventTypesAsync(ct);

    public Task<PagedResult<AdminAuditEntry>> AuditAsync(Guid? clinicId, string? search, int page, CancellationToken ct = default) =>
        _audit.ListAsync(clinicId, search, page, ct);
}
