namespace SculptFlowAdmin.Entities.Responses.PlatformAdmin;

/// <summary>(Copy of the main app's Entities/Dtos/PlatformAdmin shape.) One message of a conversation, as the admin transcript shows it.</summary>
public record MessageDetail(Guid Id, string Direction, string SenderType, string MessageType, string? Content,
    string? DeliveryStatus, DateTimeOffset? FailedAt, string? FailureCode, string? FailureReason, string Origin, Guid? CampaignId,
    DateTimeOffset CreatedAt);
