namespace Omni.Admin.Entities.Dtos.Admins;

/// <summary>An admin user (the main app's identity.users with role admin) for the Admins page (main app:
/// <c>Entities/Dtos/PlatformAdmin/AdminRow</c>).</summary>
public record AdminRow(string Id, string? Email, string? FullName, bool IsActive, bool LoginLocked, DateTimeOffset? LastLoginAt,
    DateTimeOffset CreatedAt);
