using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Monitoring;
using Omni.Admin.Common.Enums;
using Omni.Admin.Common.Statics;
using Omni.Admin.Entities.Responses.Monitoring;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Errors;

/// <summary>Read-only: the main app's saved warnings and errors grouped by problem, through its error log API.</summary>
[ScopeCapability(ScopeCapability.Clinic)]
public class IndexModel : MainAppPageModel
{
    private readonly IAppLogsAdminService _logs;
    private readonly ScopeContext _scope;

    public IndexModel(IAppLogsAdminService logs, ScopeContext scope)
    {
        _logs = logs;
        _scope = scope;
    }

    [BindProperty(SupportsGet = true)] public int? Hours { get; set; }
    [BindProperty(SupportsGet = true)] public string? Level { get; set; }
    [BindProperty(SupportsGet = true)] public string? Sort { get; set; }
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true)] public Guid? ClinicId { get; set; }
    public AppLogGroupReport? Report { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        // Only the offered choices; anything else in the URL falls back to the default.
        Hours = AppLogOptions.Periods.Any(p => p.Hours == Hours) ? Hours : AppLogOptions.DefaultHours;
        Level = AppLogOptions.Levels.Any(l => l.Key == Level) ? Level : AppLogOptions.DefaultLevel;
        Sort = AppLogOptions.Sorts.Any(s => s.Key == Sort) ? Sort : AppLogOptions.DefaultSort;
        ClinicId ??= _scope.FilterGuid; // the header's clinic scope when the URL names none
        await LoadAsync(async () => Report = await _logs.GroupsAsync(Hours!.Value, Level!, ClinicId, Clean(Q), Sort!, ct));
    }
}