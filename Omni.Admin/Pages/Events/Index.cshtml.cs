using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Overview;
using Omni.Admin.Common.Enums;
using Omni.Admin.Entities.Dtos.Overview;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Events;

[ScopeCapability(ScopeCapability.Client)]
public class IndexModel : AdminPageModel
{
    private readonly IOverviewAdminService _overview;
    private readonly ScopeContext _scope;

    public IndexModel(IOverviewAdminService overview, ScopeContext scope)
    {
        _overview = overview;
        _scope = scope;
    }

    [BindProperty(SupportsGet = true)] public Guid? ClientId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Type { get; set; }
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public PagedResult<EventRow> Result { get; private set; } = null!;
    public List<string> Types { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        ClientId ??= _scope.FilterGuid; // the header's client scope when the URL names none
        Types = await _overview.EventTypesAsync(ct);
        Result = await _overview.EventsAsync(ClientId, Type, PageNumber, ct);
    }
}
