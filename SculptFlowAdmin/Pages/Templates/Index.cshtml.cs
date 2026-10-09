using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Content;
using SculptFlowAdmin.Common.Enums;
using SculptFlowAdmin.Entities.Dtos.Content;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Templates;

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
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    public List<TemplateRow> Rows { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        ClinicId ??= _scope.FilterGuid; // the header's clinic scope when the URL names none
        Rows = await _content.ListTemplatesAsync(ClinicId, Status, ct);
    }
}
