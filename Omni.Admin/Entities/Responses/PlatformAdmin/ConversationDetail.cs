namespace Omni.Admin.Entities.Responses.PlatformAdmin;

/// <summary>(Copy of the main app's Entities/Dtos/PlatformAdmin shape.) One conversation for the admin portal.</summary>
public record ConversationDetail(Guid Id, Guid ClientId, Guid ContactId, ConversationContact? Contact, string Channel, string Status,
    string Mode, DateTimeOffset? LastMessageAt, DateTimeOffset? ServiceWindowExpiresAt, bool IsServiceWindowOpen,
    DateTimeOffset CreatedAt);
