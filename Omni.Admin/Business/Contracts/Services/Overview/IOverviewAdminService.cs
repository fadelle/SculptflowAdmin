using Omni.Admin.Entities.Dtos.Clients;
using Omni.Admin.Entities.Dtos.Overview;
using Omni.Admin.Entities.Models;
using Omni.Admin.Entities.Responses.Paging;

namespace Omni.Admin.Business.Contracts.Services.Overview;

/// <summary>Dashboard totals, problems and the event log (main app's overview API), plus this portal's own audit log.</summary>
public interface IOverviewAdminService
{
    Task<SystemTotals> TotalsAsync(CancellationToken ct = default);

    Task<List<DailyCount>> DailyMessagesAsync(int days = 14, CancellationToken ct = default);

    Task<List<ProblemRow>> ProblemsAsync(CancellationToken ct = default);

    Task<List<ClientRow>> RecentClientsAsync(int take, CancellationToken ct = default);

    Task<PagedResult<EventRow>> EventsAsync(Guid? clientId, string? eventType, int page, CancellationToken ct = default);

    Task<List<string>> EventTypesAsync(CancellationToken ct = default);

    Task<PagedResult<AdminAuditEntry>> AuditAsync(Guid? clientId, string? search, int page, CancellationToken ct = default);
}
