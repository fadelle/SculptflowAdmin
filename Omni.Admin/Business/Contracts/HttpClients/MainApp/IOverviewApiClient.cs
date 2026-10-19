using Omni.Admin.Entities.Dtos.Clinics;
using Omni.Admin.Entities.Dtos.Overview;
using Omni.Admin.Entities.Responses.Paging;

namespace Omni.Admin.Business.Contracts.HttpClients.MainApp;

public interface IOverviewApiClient
{
    Task<SystemTotals> TotalsAsync(CancellationToken ct);

    Task<List<DailyCount>> DailyMessagesAsync(int days, CancellationToken ct);

    Task<List<ProblemRow>> ProblemsAsync(CancellationToken ct);

    Task<List<ClinicRow>> RecentClinicsAsync(int take, CancellationToken ct);

    Task<PagedResult<EventRow>> EventsAsync(Guid? clinicId, string? eventType, int page, CancellationToken ct);

    Task<List<string>> EventTypesAsync(CancellationToken ct);
}
