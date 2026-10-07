namespace SculptFlowAdmin.Entities.Responses.PlatformAdmin;

/// <summary>(Copy of the main app's Entities/Dtos/PlatformAdmin shape.) One WhatsApp health event of a connection.</summary>
public record HealthEventRow(Guid Id, string EventType, string? Severity, string? Status, string? Code, string? Message,
    DateTimeOffset OccurredAt);
