namespace Omni.Admin.Entities.Responses.PlatformAdmin;

/// <summary>(Copy of the main app's Entities/Dtos/PlatformAdmin shape.) What opening a client did. The client exists whatever
/// the rest says: PlanStarted false = no plan yet; InviteSent false = the owner email didn't go (send it again).</summary>
public record ClientCreated(
    Guid ClientId,
    string Slug,
    string? PlanCode,
    bool PlanStarted,
    bool InviteSent,
    DateTimeOffset? InviteExpiresAt
);
