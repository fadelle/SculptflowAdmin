using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Leads;
using Omni.Admin.Common.Enums;
using Omni.Admin.Entities.Dtos.Leads;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Messages;

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
    [BindProperty(SupportsGet = true)] public bool Failed { get; set; }
    [BindProperty(SupportsGet = true)] public string? Sender { get; set; }
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public PagedResult<MessageRow> Result { get; private set; } = null!;

    public async Task OnGetAsync(CancellationToken ct)
    {
        ClinicId ??= _scope.FilterGuid; // the header's clinic scope when the URL names none
        Result = await _leads.ListMessagesAsync(ClinicId, Failed, Sender, Q, PageNumber, ct);
    }
}
