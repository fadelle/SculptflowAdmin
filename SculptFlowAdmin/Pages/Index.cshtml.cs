using SculptFlowAdmin.Business.Contracts.Services.Overview;
using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Dtos.Overview;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages;

public class IndexModel : AdminPageModel
{
    private readonly IOverviewAdminService _overview;

    public IndexModel(IOverviewAdminService overview) => _overview = overview;

    public SystemTotals Totals { get; private set; } = null!;
    public List<DailyCount> Daily { get; private set; } = new();
    public List<ProblemRow> Problems { get; private set; } = new();
    public List<ClinicRow> RecentClinics { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        Totals = await _overview.TotalsAsync(ct);
        Daily = await _overview.DailyMessagesAsync(14, ct);
        Problems = await _overview.ProblemsAsync(ct);
        RecentClinics = await _overview.RecentClinicsAsync(8, ct);
    }
}
