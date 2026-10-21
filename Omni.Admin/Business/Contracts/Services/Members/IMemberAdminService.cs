using Omni.Admin.Entities.Dtos.Members;
using Omni.Admin.Entities.Responses.Members;
using Omni.Admin.Entities.Responses.Paging;

namespace Omni.Admin.Business.Contracts.Services.Members;

/// <summary>Member accounts, through the main app's member API; every change is audited.</summary>
public interface IMemberAdminService
{
    Task<PagedResult<MemberRow>> ListAsync(Guid? clientId, string? search, int page, CancellationToken ct = default);

    Task<MemberRow?> GetAsync(string userId, CancellationToken ct = default);

    Task SetMembershipActiveAsync(string userId, bool active, CancellationToken ct = default);

    Task SetLoginLockedAsync(string userId, bool locked, CancellationToken ct = default);

    /// <summary>The main app checks it against its password rules.</summary>
    Task SetPasswordAsync(string userId, string password, CancellationToken ct = default);

    Task SetEmailConfirmedAsync(string userId, bool confirmed, CancellationToken ct = default);

    /// <summary>The main app emails a one-time reset link (the person chooses the password themselves).</summary>
    Task SendPasswordResetEmailAsync(string userId, CancellationToken ct = default);

    Task<IReadOnlyList<InviteResponse>> ListInvitesAsync(Guid clientId, CancellationToken ct = default);

    Task RevokeInviteAsync(Guid inviteId, CancellationToken ct = default);
}
