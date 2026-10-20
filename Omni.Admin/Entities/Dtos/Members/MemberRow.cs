using Microsoft.AspNetCore.Identity;

namespace Omni.Admin.Entities.Dtos.Members;

public record MemberRow(string UserId, string? Email, string? FullName, Guid? MembershipId, Guid? ClientId, string? ClientName,
    bool MembershipActive, bool LoginLocked, bool EmailConfirmed, int AccessFailedCount, bool HasPassword, bool HasGoogleLogin,
    DateTimeOffset? JoinedAt, string Role, bool UserActive, DateTimeOffset? LastLoginAt);
