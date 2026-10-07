using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Dtos.Overview;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Entities.Responses.Paging;

namespace SculptFlowAdmin.Business.Contracts.Services.Overview;

/// <summary>Dashboard totals, problems and the event log (main app's overview API), plus this portal's own audit log.</summary>
public interface IOverviewAdminService
{
    Task<SystemTotals> TotalsAsync(CancellationToken ct = default);

    Task<List<DailyCount>> DailyMessagesAsync(int days = 14, CancellationToken ct = default);

    Task<List<ProblemRow>> ProblemsAsync(CancellationToken ct = default);

    Task<List<ClinicRow>> RecentClinicsAsync(int take, CancellationToken ct = default);

    Task<PagedResult<EventRow>> EventsAsync(Guid? clinicId, string? eventType, int page, CancellationToken ct = default);

    Task<List<string>> EventTypesAsync(CancellationToken ct = default);

    Task<PagedResult<AdminAuditEntry>> AuditAsync(Guid? clinicId, string? search, int page, CancellationToken ct = default);
}
