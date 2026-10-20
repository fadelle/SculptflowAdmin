using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Content;
using Omni.Admin.Common.Enums;
using Omni.Admin.Entities.Dtos.Content;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Campaigns;

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
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public PagedResult<CampaignRow> Result { get; private set; } = null!;

    public async Task OnGetAsync(CancellationToken ct)
    {
        ClientId ??= _scope.FilterGuid; // the header's client scope when the URL names none
        Result = await _content.ListCampaignsAsync(ClientId, Status, PageNumber, ct);
    }
}
