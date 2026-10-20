using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Content;
using Omni.Admin.Common.Enums;
using Omni.Admin.Entities.Dtos.Content;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Services;

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
    public List<ServiceRow> Rows { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        ClientId ??= _scope.FilterGuid; // the header's client scope when the URL names none
        Rows = await _content.ListServicesAsync(ClientId, ct);
    }

    public Task<IActionResult> OnPostSetActiveAsync(Guid id, bool active, CancellationToken ct) =>
        RunAsync(() => _content.SetServiceActiveAsync(id, active, ct), active ? "Service activated." : "Service deactivated.");
}
