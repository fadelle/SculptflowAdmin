namespace Omni.Admin.Entities.Dtos.Contacts;

public record MessageRow(Guid Id, Guid ClientId, string ClientName, Guid ConversationId, string Channel, string Direction,
    string SenderType, string Origin, string? Content, string? DeliveryStatus, string? FailureCode, string? FailureReason,
    DateTimeOffset CreatedAt);
