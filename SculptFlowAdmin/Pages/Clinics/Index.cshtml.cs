using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Clinics;
using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Clinics;

public class IndexModel : AdminPageModel
{
    private readonly IClinicAdminService _clinics;

    public IndexModel(IClinicAdminService clinics) => _clinics = clinics;

    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true)] public string? Active { get; set; }
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public PagedResult<ClinicRow> Result { get; private set; } = null!;

    public async Task OnGetAsync(CancellationToken ct)
    {
        bool? active = Active switch { "true" => true, "false" => false, _ => null };
        Result = await _clinics.ListAsync(Q, active, PageNumber, ct);
    }
}
