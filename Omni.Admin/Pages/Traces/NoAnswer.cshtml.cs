using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Monitoring;
using Omni.Admin.Common.Enums;
using Omni.Admin.Common.Statics;
using Omni.Admin.Entities.Dtos.Monitoring;
using Omni.Admin.Entities.Responses.Monitoring;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Traces;

/// <summary>Read-only (QA t7-10): conversations whose last message is the customer's and got no answer in time, with what that
/// message's trace came to (skipped and why, failed, no reply...), so nobody is left waiting unseen.</summary>
[ScopeCapability(ScopeCapability.Client)]
public class NoAnswerModel : MainAppPageModel
{
    /// <summary>How long a customer message may wait before it counts as unanswered.</summary>
    public static readonly IReadOnlyList<(int Minutes, string Label)> Waits = [(5, "5 minutes"), (15, "15 minutes"), (60, "1 hour"), (240, "4 hours"), (1440, "1 day")];

    private readonly IMessageTracesAdminService _traces;
    private readonly ScopeContext _scope;

    public NoAnswerModel(IMessageTracesAdminService traces, ScopeContext scope)
    {
        _traces = traces;
        _scope = scope;
    }

    [BindProperty(SupportsGet = true)] public int? Hours { get; set; }
    [BindProperty(SupportsGet = true)] public int? Minutes { get; set; }
    [BindProperty(SupportsGet = true)] public Guid? ClientId { get; set; }
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public UnansweredPage? Result { get; private set; }

    public PagedResult<UnansweredRow> Paged => Result is null
        ? new PagedResult<UnansweredRow>([], 1, MessageTraceOptions.PageSize, 0)
        : new PagedResult<UnansweredRow>(Result.Items, Result.Page, Result.PageSize, Result.Total);

    public async Task OnGetAsync(CancellationToken ct)
    {
        Hours = MessageTraceOptions.Periods.Any(p => p.Hours == Hours) ? Hours : MessageTraceOptions.DefaultHours;
        Minutes = Waits.Any(w => w.Minutes == Minutes) ? Minutes : 15;
        ClientId ??= _scope.FilterGuid;
        await LoadAsync(async () => Result = await _traces.UnansweredAsync(Hours!.Value, Minutes!.Value, ClientId, Math.Max(1, PageNumber), ct));
    }
}
