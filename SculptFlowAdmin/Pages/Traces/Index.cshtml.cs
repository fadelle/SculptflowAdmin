using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Monitoring;
using SculptFlowAdmin.Common.Enums;
using SculptFlowAdmin.Common.Statics;
using SculptFlowAdmin.Entities.Dtos.Monitoring;
using SculptFlowAdmin.Entities.Responses.Monitoring;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Traces;

/// <summary>Read-only: recent message traces (one per message story), with a search by trace, message or provider id.</summary>
[ScopeCapability(ScopeCapability.Clinic)]
public class IndexModel : MainAppPageModel
{
    private readonly IMessageTracesAdminService _traces;
    private readonly ScopeContext _scope;

    public IndexModel(IMessageTracesAdminService traces, ScopeContext scope)
    {
        _traces = traces;
        _scope = scope;
    }

    [BindProperty(SupportsGet = true)] public int? Hours { get; set; }
    [BindProperty(SupportsGet = true)] public bool Failed { get; set; }
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true)] public Guid? ClinicId { get; set; }
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public MessageTracePage? Result { get; private set; }
    public string? NotFoundQuery { get; private set; }

    public PagedResult<TraceSummaryRow> Paged => Result is null
        ? new PagedResult<TraceSummaryRow>([], 1, MessageTraceOptions.PageSize, 0)
        : new PagedResult<TraceSummaryRow>(Result.Items, Result.Page, Result.PageSize, Result.Total);

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        Hours = MessageTraceOptions.Periods.Any(p => p.Hours == Hours) ? Hours : MessageTraceOptions.DefaultHours;
        ClinicId ??= _scope.FilterGuid;
        if (Clean(Q) is { } query)
        {
            string? traceId = null;
            if (!await LoadAsync(async () => traceId = await _traces.ResolveAsync(query, ct))) return Page();
            if (traceId is not null) return Redirect("/Traces/Details?traceId=" + Uri.EscapeDataString(traceId));
            NotFoundQuery = query;
        }
        await LoadAsync(async () => Result = await _traces.ListAsync(Hours!.Value, ClinicId, Failed, Math.Max(1, PageNumber), ct));
        return Page();
    }
}