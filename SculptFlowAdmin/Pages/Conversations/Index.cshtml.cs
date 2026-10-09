using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Leads;
using SculptFlowAdmin.Common.Enums;
using SculptFlowAdmin.Entities.Dtos.Leads;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Conversations;

[ScopeCapability(ScopeCapability.Clinic)]
public class IndexModel : AdminPageModel
{
    private readonly ILeadAdminService _leads;
    private readonly ScopeContext _scope;

    public IndexModel(ILeadAdminService leads, ScopeContext scope)
    {
        _leads = leads;
        _scope = scope;
    }

    [BindProperty(SupportsGet = true)] public Guid? ClinicId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Channel { get; set; }
    [BindProperty(SupportsGet = true)] public string? Mode { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public PagedResult<ConversationRow> Result { get; private set; } = null!;

    public async Task OnGetAsync(CancellationToken ct)
    {
        ClinicId ??= _scope.FilterGuid; // the header's clinic scope when the URL names none
        Result = await _leads.ListConversationsAsync(ClinicId, null, Channel, Mode, Status, PageNumber, ct);
    }
}
