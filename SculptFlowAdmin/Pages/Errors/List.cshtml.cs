using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Monitoring;
using SculptFlowAdmin.Common.Enums;
using SculptFlowAdmin.Common.Statics;
using SculptFlowAdmin.Entities.Dtos.Monitoring;
using SculptFlowAdmin.Entities.Responses.Monitoring;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Errors;

/// <summary>Read-only: each saved warning/error, newest first, for one problem (fingerprint) or one request (trace id).</summary>
[ScopeCapability(ScopeCapability.Clinic)]
public class ListModel : MainAppPageModel
{
    private readonly IAppLogsAdminService _logs;
    private readonly ScopeContext _scope;

    public ListModel(IAppLogsAdminService logs, ScopeContext scope)
    {
        _logs = logs;
        _scope = scope;
    }

    [BindProperty(SupportsGet = true)] public int? Hours { get; set; }
    [BindProperty(SupportsGet = true)] public string? Level { get; set; }
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true)] public string? Fingerprint { get; set; }
    [BindProperty(SupportsGet = true)] public string? TraceId { get; set; }
    [BindProperty(SupportsGet = true)] public Guid? ClinicId { get; set; }
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public AppLogPage? Result { get; private set; }

    public PagedResult<AppLogRow> Paged => Result is null
        ? new PagedResult<AppLogRow>([], 1, AppLogOptions.PageSize, 0)
        : new PagedResult<AppLogRow>(Result.Items, Result.Page, Result.PageSize, Result.Total);

    public async Task OnGetAsync(CancellationToken ct)
    {
        Hours = AppLogOptions.Periods.Any(p => p.Hours == Hours) ? Hours : AppLogOptions.DefaultHours;
        Level = AppLogOptions.Levels.Any(l => l.Key == Level) ? Level : AppLogOptions.DefaultLevel;
        ClinicId ??= _scope.FilterGuid;
        await LoadAsync(async () => Result = await _logs.ListAsync(Hours!.Value, Level!, ClinicId, Clean(Q), Clean(Fingerprint), Clean(TraceId),
            Math.Max(1, PageNumber), ct));
    }
}