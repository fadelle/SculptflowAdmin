namespace Omni.Admin.Entities.Responses.PlatformAdmin;

/// <summary>(Copy of the main app's Entities/Dtos/PlatformAdmin shape.) One event_logs row of a contact.</summary>
public record ContactEventRow(Guid Id, Guid? ContactId, Guid? ConversationId, Guid? BookingId, string EventType, string? Source,
    string Metadata, DateTimeOffset CreatedAt);
