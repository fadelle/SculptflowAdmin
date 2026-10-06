using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Clinics;
using SculptFlowAdmin.Business.Contracts.Services.Content;
using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Dtos.Content;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Procedures;

public class IndexModel : AdminPageModel
{
    private readonly IContentAdminService _content;
    private readonly IClinicAdminService _clinics;

    public IndexModel(IContentAdminService content, IClinicAdminService clinics)
    {
        _content = content;
        _clinics = clinics;
    }

    [BindProperty(SupportsGet = true)] public Guid? ClinicId { get; set; }
    public List<ProcedureRow> Rows { get; private set; } = new();
    public List<ClinicOption> Clinics { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        Clinics = await _clinics.OptionsAsync(ct);
        Rows = await _content.ListProceduresAsync(ClinicId, ct);
    }

    public Task<IActionResult> OnPostSetActiveAsync(Guid id, bool active, CancellationToken ct) =>
        RunAsync(() => _content.SetProcedureActiveAsync(id, active, ct), active ? "Procedure activated." : "Procedure deactivated.");
}
