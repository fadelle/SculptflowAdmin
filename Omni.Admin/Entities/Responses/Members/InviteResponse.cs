namespace Omni.Admin.Entities.Responses.Members;

/// <summary>Copy of the main app's InviteResponse (Entities/Responses/Members): a member invite that was neither accepted
/// nor cancelled. IsExpired: its link no longer works. Never includes the link or its token.</summary>
public record InviteResponse(Guid Id, string Email, string? FullName, DateTimeOffset CreatedAt, DateTimeOffset ExpiresAt, bool IsExpired);
