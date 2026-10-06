using SculptFlowAdmin.Business.Contracts.Services.Overview;
using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Dtos.Overview;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Persistence.Contracts.Overview;

namespace SculptFlowAdmin.Business.Services.Overview;

/// <summary>The system-wide dashboard, the problems list, the events log and the admin audit log.</summary>
public class OverviewAdminService : IOverviewAdminService
{
    private readonly IOverviewAdminRepository _repository;

    public OverviewAdminService(IOverviewAdminRepository repository)
    {
        _repository = repository;
    }

    public Task<SystemTotals> TotalsAsync(CancellationToken ct = default) =>
        _repository.TotalsAsync(ct);

    public Task<List<DailyCount>> DailyMessagesAsync(int days = 14, CancellationToken ct = default) =>
        _repository.DailyMessagesAsync(days, ct);

    public Task<List<ProblemRow>> ProblemsAsync(CancellationToken ct = default) =>
        _repository.ProblemsAsync(ct);

    public Task<List<ClinicRow>> RecentClinicsAsync(int take, CancellationToken ct = default) =>
        _repository.RecentClinicsAsync(take, ct);

    public Task<PagedResult<EventRow>> EventsAsync(Guid? clinicId, string? eventType, int page, CancellationToken ct = default) =>
        _repository.EventsAsync(clinicId, eventType, page, ct);

    public Task<List<string>> EventTypesAsync(CancellationToken ct = default) =>
        _repository.EventTypesAsync(ct);

    public Task<PagedResult<AdminAuditEntry>> AuditAsync(Guid? clinicId, string? search, int page, CancellationToken ct = default) =>
        _repository.AuditAsync(clinicId, search, page, ct);

}
