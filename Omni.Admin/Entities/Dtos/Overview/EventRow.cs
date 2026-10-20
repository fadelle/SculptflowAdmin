namespace Omni.Admin.Entities.Dtos.Overview;

public record EventRow(Guid Id, Guid ClientId, string ClientName, string EventType, string? Source, Guid? ContactId,
    Guid? ConversationId, Guid? BookingId, string Metadata, DateTimeOffset CreatedAt);
