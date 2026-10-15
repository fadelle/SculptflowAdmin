using SculptFlowAdmin.Business.Contracts.Services.Monitoring;
using SculptFlowAdmin.Business.Contracts.Services.Overview;
using SculptFlowAdmin.Entities.Responses.Monitoring;
using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Dtos.Overview;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages;

public class IndexModel : AdminPageModel
{
    private readonly IOverviewAdminService _overview;
    private readonly IAppLogsAdminService _logs;

    public IndexModel(IOverviewAdminService overview, IAppLogsAdminService logs)
    {
        _overview = overview;
        _logs = logs;
    }

    public SystemTotals Totals { get; private set; } = null!;
    /// <summary>Saved warnings/errors in the last 24 hours; null when the main app couldn't answer (the tile says so).</summary>
    public AppLogSummary? Errors { get; private set; }
    public List<DailyCount> Daily { get; private set; } = new();
    public List<ProblemRow> Problems { get; private set; } = new();
    public List<ClinicRow> RecentClinics { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        Totals = await _overview.TotalsAsync(ct);
        Daily = await _overview.DailyMessagesAsync(14, ct);
        Problems = await _overview.ProblemsAsync(ct);
        RecentClinics = await _overview.RecentClinicsAsync(8, ct);
        Errors = await _logs.TrySummaryAsync(ct);
    }
}
