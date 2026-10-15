using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Monitoring;
using SculptFlowAdmin.Entities.Responses.Monitoring;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Errors;

/// <summary>Read-only: one saved warning/error in full (stack trace, logged values, links to the request and records).</summary>
public class DetailsModel : AdminPageModel
{
    private readonly IAppLogsAdminService _logs;

    public DetailsModel(IAppLogsAdminService logs) => _logs = logs;

    [BindProperty(SupportsGet = true)] public long Id { get; set; }
    public AppLogDetail Entry { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var entry = await _logs.GetAsync(Id, ct);
        if (entry is null) return NotFound();
        Entry = entry;
        return Page();
    }
}