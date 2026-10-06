using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Content;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Knowledge;

public class DetailsModel : AdminPageModel
{
    private readonly IContentAdminService _content;

    public DetailsModel(IContentAdminService content) => _content = content;

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    public KnowledgeDocument Doc { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var doc = await _content.GetDocumentAsync(Id, ct);
        if (doc is null) return NotFound();
        Doc = doc;
        return Page();
    }

    public Task<IActionResult> OnPostSetActiveAsync(bool active, CancellationToken ct) =>
        RunAsync(() => _content.SetDocumentActiveAsync(Id, active, ct), active ? "Document activated." : "Document deactivated.", new { id = Id });
}
