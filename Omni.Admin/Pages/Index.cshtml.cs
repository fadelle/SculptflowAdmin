using Omni.Admin.Business.Contracts.Services.Monitoring;
using Omni.Admin.Business.Contracts.Services.Overview;
using Omni.Admin.Entities.Responses.Monitoring;
using Omni.Admin.Entities.Dtos.Clients;
using Omni.Admin.Entities.Dtos.Overview;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages;

public class IndexModel : AdminPageModel
{
    private readonly IOverviewAdminService _overview;
    private readonly IAppLogsAdminService _logs;

    private readonly IMessageTracesAdminService _traces;

    public IndexModel(IOverviewAdminService overview, IAppLogsAdminService logs, IMessageTracesAdminService traces)
    {
        _overview = overview;
        _logs = logs;
        _traces = traces;
    }

    /// <summary>Message traces in the last 24 hours; null when the main app couldn't answer (the tile says so).</summary>
    public MessageTraceSummary? Traces { get; private set; }

    public SystemTotals Totals { get; private set; } = null!;
    /// <summary>Saved warnings/errors in the last 24 hours; null when the main app couldn't answer (the tile says so).</summary>
    public AppLogSummary? Errors { get; private set; }
    public List<DailyCount> Daily { get; private set; } = new();
    public List<ProblemRow> Problems { get; private set; } = new();
    public List<ClientRow> RecentClients { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        Totals = await _overview.TotalsAsync(ct);
        Daily = await _overview.DailyMessagesAsync(14, ct);
        Problems = await _overview.ProblemsAsync(ct);
        RecentClients = await _overview.RecentClientsAsync(8, ct);
        Errors = await _logs.TrySummaryAsync(ct);
        Traces = await _traces.TrySummaryAsync(ct);
    }
}
