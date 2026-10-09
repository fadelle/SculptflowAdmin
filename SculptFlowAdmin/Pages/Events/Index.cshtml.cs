using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Overview;
using SculptFlowAdmin.Common.Enums;
using SculptFlowAdmin.Entities.Dtos.Overview;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Events;

[ScopeCapability(ScopeCapability.Clinic)]
public class IndexModel : AdminPageModel
{
    private readonly IOverviewAdminService _overview;
    private readonly ScopeContext _scope;

    public IndexModel(IOverviewAdminService overview, ScopeContext scope)
    {
        _overview = overview;
        _scope = scope;
    }

    [BindProperty(SupportsGet = true)] public Guid? ClinicId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Type { get; set; }
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public PagedResult<EventRow> Result { get; private set; } = null!;
    public List<string> Types { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        ClinicId ??= _scope.FilterGuid; // the header's clinic scope when the URL names none
        Types = await _overview.EventTypesAsync(ct);
        Result = await _overview.EventsAsync(ClinicId, Type, PageNumber, ct);
    }
}
