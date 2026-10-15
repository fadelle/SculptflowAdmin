using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Monitoring;
using SculptFlowAdmin.Entities.Responses.Monitoring;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Traces;

/// <summary>Read-only: one message's timeline, by trace id or by the message it was created for.</summary>
public class DetailsModel : AdminPageModel
{
    private readonly IMessageTracesAdminService _traces;

    public DetailsModel(IMessageTracesAdminService traces) => _traces = traces;

    [BindProperty(SupportsGet = true)] public string? TraceId { get; set; }
    [BindProperty(SupportsGet = true)] public Guid? MessageId { get; set; }
    public MessageTrace Trace { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var trace = await _traces.GetAsync(TraceId, MessageId, ct);
        if (trace is null) return NotFound();
        Trace = trace;
        return Page();
    }
}