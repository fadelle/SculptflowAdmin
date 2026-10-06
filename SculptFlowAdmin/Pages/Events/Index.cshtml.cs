using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Clinics;
using SculptFlowAdmin.Business.Contracts.Services.Overview;
using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Dtos.Overview;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Events;

public class IndexModel : AdminPageModel
{
    private readonly IOverviewAdminService _overview;
    private readonly IClinicAdminService _clinics;

    public IndexModel(IOverviewAdminService overview, IClinicAdminService clinics)
    {
        _overview = overview;
        _clinics = clinics;
    }

    [BindProperty(SupportsGet = true)] public Guid? ClinicId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Type { get; set; }
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public PagedResult<EventRow> Result { get; private set; } = null!;
    public List<ClinicOption> Clinics { get; private set; } = new();
    public List<string> Types { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        Clinics = await _clinics.OptionsAsync(ct);
        Types = await _overview.EventTypesAsync(ct);
        Result = await _overview.EventsAsync(ClinicId, Type, PageNumber, ct);
    }
}
