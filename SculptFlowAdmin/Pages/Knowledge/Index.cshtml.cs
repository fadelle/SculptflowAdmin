using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Clinics;
using SculptFlowAdmin.Business.Contracts.Services.Content;
using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Dtos.Content;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Knowledge;

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
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true)] public string? Active { get; set; }
    [BindProperty(SupportsGet = true)] public string Tab { get; set; } = "documents";
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public PagedResult<KnowledgeDocRow> Docs { get; private set; } = null!;
    public List<WebsiteSourceRow> Websites { get; private set; } = new();
    public List<ClinicOption> Clinics { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        Clinics = await _clinics.OptionsAsync(ct);
        bool? active = Active switch { "true" => true, "false" => false, _ => null };
        Docs = await _content.ListDocumentsAsync(ClinicId, Q, active, PageNumber, ct);
        Websites = await _content.ListWebsitesAsync(ClinicId, ct);
    }

    public Task<IActionResult> OnPostSetActiveAsync(Guid id, bool active, CancellationToken ct) =>
        RunAsync(() => _content.SetDocumentActiveAsync(id, active, ct), active ? "Document activated." : "Document deactivated.");
}
