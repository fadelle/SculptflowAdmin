using Omni.Admin.Entities.Dtos.Members;
using Omni.Admin.Entities.Responses.Members;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Business.Contracts.HttpClients.MainApp;

public interface IMembersApiClient
{
    Task<PagedResult<MemberRow>> ListAsync(Guid? clientId, string? search, int page, CancellationToken ct);

    Task<MemberRow?> GetAsync(string userId, CancellationToken ct);

    Task<PlatformAdminChange> SetMembershipActiveAsync(string userId, bool active, CancellationToken ct);

    Task<PlatformAdminChange> SetLoginLockedAsync(string userId, bool locked, CancellationToken ct);

    Task<PlatformAdminChange> SetPasswordAsync(string userId, string password, CancellationToken ct);

    Task<PlatformAdminChange> SetEmailConfirmedAsync(string userId, bool confirmed, CancellationToken ct);

    /// <summary>The main app emails the user a one-time password reset link (502 when the email can't be sent).</summary>
    Task<PlatformAdminChange> SendPasswordResetEmailAsync(string userId, CancellationToken ct);

    /// <summary>A client's invites that were neither accepted nor cancelled.</summary>
    Task<IReadOnlyList<InviteResponse>> ListInvitesAsync(Guid clientId, CancellationToken ct);

    Task<PlatformAdminChange> RevokeInviteAsync(Guid inviteId, CancellationToken ct);
}
