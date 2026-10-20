using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Contracts.Managers;
using Omni.Admin.Business.Contracts.Services.Members;
using Omni.Admin.Entities.Dtos.Members;
using Omni.Admin.Entities.Responses.Paging;

namespace Omni.Admin.Business.Services.Members;

public class MemberAdminService : IMemberAdminService
{
    private readonly IMembersApiClient _api;
    private readonly IAdminAudit _audit;

    public MemberAdminService(IMembersApiClient api, IAdminAudit audit)
    {
        _api = api;
        _audit = audit;
    }

    public Task<PagedResult<MemberRow>> ListAsync(Guid? clientId, string? search, int page, CancellationToken ct = default) =>
        _api.ListAsync(clientId, search, page, ct);

    public Task<MemberRow?> GetAsync(string userId, CancellationToken ct = default) => _api.GetAsync(userId, ct);

    public async Task SetMembershipActiveAsync(string userId, bool active, CancellationToken ct = default)
    {
        var change = await _api.SetMembershipActiveAsync(userId, active, ct);
        await _audit.LogAsync(active ? "member.membership_activated" : "member.membership_deactivated", "client_user", userId, change.ClientId, ct: ct);
    }

    public async Task SetLoginLockedAsync(string userId, bool locked, CancellationToken ct = default)
    {
        var change = await _api.SetLoginLockedAsync(userId, locked, ct);
        await _audit.LogAsync(locked ? "member.login_locked" : "member.login_unlocked", "identity_user", userId, change.ClientId, ct: ct);
    }

    public async Task SetPasswordAsync(string userId, string password, CancellationToken ct = default)
    {
        var change = await _api.SetPasswordAsync(userId, password, ct);
        await _audit.LogAsync("member.password_reset", "identity_user", userId, change.ClientId, ct: ct);
    }

    public async Task SetEmailConfirmedAsync(string userId, bool confirmed, CancellationToken ct = default)
    {
        var change = await _api.SetEmailConfirmedAsync(userId, confirmed, ct);
        await _audit.LogAsync(confirmed ? "member.email_confirmed" : "member.email_unconfirmed", "identity_user", userId, change.ClientId, ct: ct);
    }
}
