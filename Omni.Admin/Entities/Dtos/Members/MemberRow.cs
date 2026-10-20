using Microsoft.AspNetCore.Identity;

namespace Omni.Admin.Entities.Dtos.Members;

/// <summary>(Copy of the main app's Entities/Dtos/PlatformAdmin shape.) A client or member user with their (active first)
/// membership. Language is the user's own dashboard language (null: the client's default).</summary>
public record MemberRow(string UserId, string? Email, string? FullName, Guid? MembershipId, Guid? ClientId, string? ClientName,
    bool MembershipActive, bool LoginLocked, bool EmailConfirmed, int AccessFailedCount, bool HasPassword, bool HasGoogleLogin,
    DateTimeOffset? JoinedAt, string Role, bool UserActive, DateTimeOffset? LastLoginAt, string? Language);
