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
        // Six independent tiles: load them together (QA t6-admin-18: the dashboard took 7-8 s one call after another).
        var totals = _overview.TotalsAsync(ct);
        var daily = _overview.DailyMessagesAsync(14, ct);
        var problems = _overview.ProblemsAsync(ct);
        var recent = _overview.RecentClientsAsync(8, ct);
        var errors = _logs.TrySummaryAsync(ct);
        var traces = _traces.TrySummaryAsync(ct);
        await Task.WhenAll(totals, daily, problems, recent, errors, traces);
        Totals = totals.Result;
        Daily = daily.Result;
        Problems = problems.Result;
        RecentClients = recent.Result;
        Errors = errors.Result;
        Traces = traces.Result;
    }
}
