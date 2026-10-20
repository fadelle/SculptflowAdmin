using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Content;
using Omni.Admin.Common.Enums;
using Omni.Admin.Entities.Dtos.Content;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Knowledge;

[ScopeCapability(ScopeCapability.Client)]
public class IndexModel : AdminPageModel
{
    private readonly IContentAdminService _content;
    private readonly ScopeContext _scope;

    public IndexModel(IContentAdminService content, ScopeContext scope)
    {
        _content = content;
        _scope = scope;
    }

    [BindProperty(SupportsGet = true)] public Guid? ClientId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true)] public string? Active { get; set; }
    [BindProperty(SupportsGet = true)] public string Tab { get; set; } = "documents";
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public PagedResult<KnowledgeDocRow> Docs { get; private set; } = null!;
    public List<WebsiteSourceRow> Websites { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        ClientId ??= _scope.FilterGuid; // the header's client scope when the URL names none
        bool? active = Active switch { "true" => true, "false" => false, _ => null };
        Docs = await _content.ListDocumentsAsync(ClientId, Q, active, PageNumber, ct);
        Websites = await _content.ListWebsitesAsync(ClientId, ct);
    }

    public Task<IActionResult> OnPostSetActiveAsync(Guid id, bool active, CancellationToken ct) =>
        RunAsync(() => _content.SetDocumentActiveAsync(id, active, ct), active ? "Document activated." : "Document deactivated.");
}
