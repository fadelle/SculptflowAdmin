namespace Omni.Admin.Entities.Requests.Clients;

/// <summary>(Copy of the main app's Entities/Requests/Inbox shape, PUT /api/platform-admin/clients/{id}/inbox-settings/
/// {channelIntegrationId}.) One connected account's AI: on/off, the bot that answers (an active one of the client's; needed
/// when AiEnabled) and whether it answers new and reopened conversations.</summary>
public record UpdateAccountAiRequest(bool AiEnabled, Guid? BotId, bool AiBotNewConversations, bool AiBotReopenedConversations);
