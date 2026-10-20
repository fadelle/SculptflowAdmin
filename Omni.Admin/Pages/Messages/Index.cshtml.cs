using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Contacts;
using Omni.Admin.Common.Enums;
using Omni.Admin.Entities.Dtos.Contacts;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Messages;

[ScopeCapability(ScopeCapability.Client)]
public class IndexModel : AdminPageModel
{
    private readonly IContactAdminService _contacts;
    private readonly ScopeContext _scope;

    public IndexModel(IContactAdminService contacts, ScopeContext scope)
    {
        _contacts = contacts;
        _scope = scope;
    }

    [BindProperty(SupportsGet = true)] public Guid? ClientId { get; set; }
    [BindProperty(SupportsGet = true)] public bool Failed { get; set; }
    [BindProperty(SupportsGet = true)] public string? Sender { get; set; }
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public PagedResult<MessageRow> Result { get; private set; } = null!;

    public async Task OnGetAsync(CancellationToken ct)
    {
        ClientId ??= _scope.FilterGuid; // the header's client scope when the URL names none
        Result = await _contacts.ListMessagesAsync(ClientId, Failed, Sender, Q, PageNumber, ct);
    }
}
