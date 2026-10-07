namespace SculptFlowAdmin.Entities.Responses.PlatformAdmin;

/// <summary>(Copy of the main app's Entities/Dtos/PlatformAdmin shape.) One event_logs row of a lead.</summary>
public record LeadEventRow(Guid Id, Guid? LeadId, Guid? ConversationId, Guid? AppointmentId, string EventType, string? Source,
    string Metadata, DateTimeOffset CreatedAt);
