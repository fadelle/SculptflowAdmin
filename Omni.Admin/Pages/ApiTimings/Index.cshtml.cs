using Omni.Admin.Business.Contracts.Services.Monitoring;
using Omni.Admin.Common.Statics;
using Omni.Admin.Entities.Responses.Monitoring;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.ApiTimings;

/// <summary>Read-only: how long the main app's routes take, over a chosen period, through its API timings API.</summary>
public class IndexModel : MainAppPageModel
{
    private readonly IApiTimingsAdminService _timings;

    public IndexModel(IApiTimingsAdminService timings) => _timings = timings;

    public int Hours { get; private set; } = ApiTimingOptions.DefaultHours;
    public string Sort { get; private set; } = ApiTimingOptions.DefaultSort;
    public int Take { get; private set; } = ApiTimingOptions.DefaultTake;
    public ApiTimingReport? Report { get; private set; }

    public async Task OnGetAsync(int? hours, string? sort, int? take, CancellationToken ct)
    {
        // Only the offered choices; anything else in the URL falls back to the default.
        Hours = ApiTimingOptions.Periods.Any(p => p.Hours == hours) ? hours!.Value : ApiTimingOptions.DefaultHours;
        Sort = ApiTimingOptions.Sorts.Any(s => s.Key == sort) ? sort! : ApiTimingOptions.DefaultSort;
        Take = ApiTimingOptions.Takes.Contains(take ?? 0) ? take!.Value : ApiTimingOptions.DefaultTake;
        await LoadAsync(async () => Report = await _timings.ReportAsync(Hours, Sort, Take, ct));
    }
}
