using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Dtos.Overview;
using SculptFlowAdmin.Entities.Responses.Paging;

namespace SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;

public interface IOverviewApiClient
{
    Task<SystemTotals> TotalsAsync(CancellationToken ct);

    Task<List<DailyCount>> DailyMessagesAsync(int days, CancellationToken ct);

    Task<List<ProblemRow>> ProblemsAsync(CancellationToken ct);

    Task<List<ClinicRow>> RecentClinicsAsync(int take, CancellationToken ct);

    Task<PagedResult<EventRow>> EventsAsync(Guid? clinicId, string? eventType, int page, CancellationToken ct);

    Task<List<string>> EventTypesAsync(CancellationToken ct);
}
