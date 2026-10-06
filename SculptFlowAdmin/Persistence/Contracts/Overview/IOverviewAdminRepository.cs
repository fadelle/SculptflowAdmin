using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Dtos.Overview;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Entities.Responses.Paging;

namespace SculptFlowAdmin.Persistence.Contracts.Overview;

public interface IOverviewAdminRepository
{
    Task<SystemTotals> TotalsAsync(CancellationToken ct = default);

    /// <summary>Messages per UTC day for the last <paramref name="days"/> days, split by who sent them.</summary>
    Task<List<DailyCount>> DailyMessagesAsync(int days = 14, CancellationToken ct = default);

    /// <summary>Everything that probably needs an admin: broken channels and calendars, failing sends, stuck campaigns.</summary>
    Task<List<ProblemRow>> ProblemsAsync(CancellationToken ct = default);

    Task<List<ClinicRow>> RecentClinicsAsync(int take, CancellationToken ct = default);

    Task<PagedResult<EventRow>> EventsAsync(Guid? clinicId, string? eventType, int page, CancellationToken ct = default);

    Task<List<string>> EventTypesAsync(CancellationToken ct = default);

    Task<PagedResult<AdminAuditEntry>> AuditAsync(Guid? clinicId, string? search, int page, CancellationToken ct = default);
}
