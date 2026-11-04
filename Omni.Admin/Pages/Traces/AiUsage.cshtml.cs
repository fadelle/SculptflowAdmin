using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Monitoring;
using Omni.Admin.Common.Enums;
using Omni.Admin.Entities.Responses.Monitoring;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Traces;

/// <summary>Read-only (QA t7-10): what each client's AI bot used (runs, model calls, tokens), what that likely cost the platform,
/// and what the client was charged for AI replies.</summary>
[ScopeCapability(ScopeCapability.Client)]
public class AiUsageModel : MainAppPageModel
{
    public static readonly IReadOnlyList<(int Days, string Label)> Periods = [(1, "Last day"), (7, "Last 7 days"), (30, "Last 30 days"), (90, "Last 90 days")];

    private readonly IMessageTracesAdminService _traces;
    private readonly ScopeContext _scope;

    public AiUsageModel(IMessageTracesAdminService traces, ScopeContext scope)
    {
        _traces = traces;
        _scope = scope;
    }

    [BindProperty(SupportsGet = true)] public int? Days { get; set; }
    [BindProperty(SupportsGet = true)] public Guid? ClientId { get; set; }
    public AiUsageReport? Result { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        Days = Periods.Any(p => p.Days == Days) ? Days : 7;
        ClientId ??= _scope.FilterGuid;
        await LoadAsync(async () => Result = await _traces.AiUsageAsync(Days!.Value, ClientId, ct));
    }
}
