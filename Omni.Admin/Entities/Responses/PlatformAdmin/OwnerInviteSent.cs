namespace Omni.Admin.Entities.Responses.PlatformAdmin;

/// <summary>(Copy of the main app's Entities/Dtos/PlatformAdmin shape.) A client's owner invite went out again
/// (POST clients/{id}/owner-invite).</summary>
public record OwnerInviteSent(Guid ClientId, string Email, DateTimeOffset ExpiresAt);
