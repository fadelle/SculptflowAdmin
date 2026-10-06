using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Clinics;
using SculptFlowAdmin.Business.Contracts.Services.Overview;
using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Audit;

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
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public PagedResult<AdminAuditEntry> Result { get; private set; } = null!;
    public List<ClinicOption> Clinics { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        Clinics = await _clinics.OptionsAsync(ct);
        Result = await _overview.AuditAsync(ClinicId, Q, PageNumber, ct);
    }
}
