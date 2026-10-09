using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Content;
using SculptFlowAdmin.Common.Enums;
using SculptFlowAdmin.Entities.Dtos.Content;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Knowledge;

[ScopeCapability(ScopeCapability.Clinic)]
public class IndexModel : AdminPageModel
{
    private readonly IContentAdminService _content;
    private readonly ScopeContext _scope;

    public IndexModel(IContentAdminService content, ScopeContext scope)
    {
        _content = content;
        _scope = scope;
    }

    [BindProperty(SupportsGet = true)] public Guid? ClinicId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true)] public string? Active { get; set; }
    [BindProperty(SupportsGet = true)] public string Tab { get; set; } = "documents";
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public PagedResult<KnowledgeDocRow> Docs { get; private set; } = null!;
    public List<WebsiteSourceRow> Websites { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        ClinicId ??= _scope.FilterGuid; // the header's clinic scope when the URL names none
        bool? active = Active switch { "true" => true, "false" => false, _ => null };
        Docs = await _content.ListDocumentsAsync(ClinicId, Q, active, PageNumber, ct);
        Websites = await _content.ListWebsitesAsync(ClinicId, ct);
    }

    public Task<IActionResult> OnPostSetActiveAsync(Guid id, bool active, CancellationToken ct) =>
        RunAsync(() => _content.SetDocumentActiveAsync(id, active, ct), active ? "Document activated." : "Document deactivated.");
}
