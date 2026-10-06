using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Clinics;
using SculptFlowAdmin.Business.Contracts.Services.Leads;
using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Dtos.Leads;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Leads;

public class IndexModel : AdminPageModel
{
    private readonly ILeadAdminService _leads;
    private readonly IClinicAdminService _clinics;

    public IndexModel(ILeadAdminService leads, IClinicAdminService clinics)
    {
        _leads = leads;
        _clinics = clinics;
    }

    [BindProperty(SupportsGet = true)] public Guid? ClinicId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public PagedResult<LeadRow> Result { get; private set; } = null!;
    public List<ClinicOption> Clinics { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        Clinics = await _clinics.OptionsAsync(ct);
        Result = await _leads.ListLeadsAsync(ClinicId, Status, Q, PageNumber, ct);
    }
}
