using Microsoft.Extensions.Options;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Common.Configs;
using Omni.Admin.Entities.Dtos.Members;
using Omni.Admin.Entities.Requests.Content;
using Omni.Admin.Entities.Requests.PlatformAdmin;
using Omni.Admin.Entities.Requests.Members;
using Omni.Admin.Entities.Responses.Members;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Business.HttpClients.MainApp;

/// <summary>Client for the main app's platform-admin member API (/api/platform-admin/members).</summary>
public class MembersApiClient : MainAppApiClient, IMembersApiClient
{
    public const string Prefix = "api/platform-admin/members";

    public MembersApiClient(HttpClient http, IOptions<MainAppApiOptions> options, IHttpContextAccessor context, ILogger<MembersApiClient> logger)
        : base(http, options, context, logger, Prefix, "Members")
    {
    }

    public Task<PagedResult<MemberRow>> ListAsync(Guid? clientId, string? search, int page, CancellationToken ct) =>
        GetRequiredAsync<PagedResult<MemberRow>>(Query(("clientId", clientId), ("search", search), ("page", page)), ct);

    public Task<MemberRow?> GetAsync(string userId, CancellationToken ct) => GetAsync<MemberRow>($"/{Esc(userId)}", ct);

    public Task<PlatformAdminChange> SetMembershipActiveAsync(string userId, bool active, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/{Esc(userId)}/membership-active", new ActiveBody(active), null, false, ct);

    public Task<PlatformAdminChange> SetLoginLockedAsync(string userId, bool locked, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/{Esc(userId)}/locked", new FlagBody(locked), null, false, ct);

    public Task<PlatformAdminChange> SetPasswordAsync(string userId, string password, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/{Esc(userId)}/password", new PasswordBody(password), null, false, ct);

    public Task<PlatformAdminChange> SetEmailConfirmedAsync(string userId, bool confirmed, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/{Esc(userId)}/email-confirmed", new FlagBody(confirmed), null, false, ct);

    public Task<PlatformAdminChange> SendPasswordResetEmailAsync(string userId, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Post, $"/{Esc(userId)}/password-reset-email", null, null, false, ct);

    public async Task<IReadOnlyList<InviteResponse>> ListInvitesAsync(Guid clientId, CancellationToken ct) =>
        await GetRequiredAsync<List<InviteResponse>>("/invites" + Query(("clientId", clientId)), ct);

    public Task<PlatformAdminChange> RevokeInviteAsync(Guid inviteId, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Delete, $"/invites/{inviteId}", null, null, false, ct);
}
