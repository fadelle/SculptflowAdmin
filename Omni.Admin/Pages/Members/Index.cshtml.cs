using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Members;
using Omni.Admin.Common.Enums;
using Omni.Admin.Entities.Dtos.Members;
using Omni.Admin.Entities.Responses.Members;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Members;

[ScopeCapability(ScopeCapability.Client)]
public class IndexModel : AdminPageModel
{
    private readonly IMemberAdminService _members;
    private readonly ScopeContext _scope;

    public IndexModel(IMemberAdminService member, ScopeContext scope)
    {
        _members = member;
        _scope = scope;
    }

    [BindProperty(SupportsGet = true)] public Guid? ClientId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public PagedResult<MemberRow> Result { get; private set; } = null!;
    /// <summary>The client's invites waiting to be accepted; only loaded when one client is shown.</summary>
    public IReadOnlyList<InviteResponse> Invites { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        ClientId ??= _scope.FilterGuid; // the header's client scope when the URL names none
        Result = await _members.ListAsync(ClientId, Q, PageNumber, ct);
        if (ClientId is { } clientId) Invites = await _members.ListInvitesAsync(clientId, ct);
    }

    public Task<IActionResult> OnPostRevokeInviteAsync(Guid inviteId, CancellationToken ct) =>
        RunAsync(() => _members.RevokeInviteAsync(inviteId, ct), "Invite cancelled. Its link no longer works.", new { clientId = ClientId });
}
