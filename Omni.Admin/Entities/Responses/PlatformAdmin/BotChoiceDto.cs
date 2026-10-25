namespace Omni.Admin.Entities.Responses.PlatformAdmin;

/// <summary>(Copy of the main app's Entities/Dtos/Inbox shape.) One of the client's AI bots: status draft, active or inactive.</summary>
public record BotChoiceDto(Guid Id, string Name, string Status);
