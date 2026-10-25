namespace Omni.Admin.Entities.Responses.PlatformAdmin;

/// <summary>(Copy of the main app's Entities/Dtos/Inbox shape.) One connected account's AI settings: the client's choices
/// (AiEnabled, BotId, the two switches), the assigned bot's name and status, and BlockReason (plan_without_ai_assistant,
/// ai_disabled, no_bot, bot_inactive, ai_bot_limit) when the team answers instead of the bot; BotAnswers is true otherwise.</summary>
public record AccountAiSettingsDto(
    Guid ChannelIntegrationId,
    string Channel,
    string? DisplayName,
    bool AiEnabled,
    Guid? BotId,
    bool AiBotNewConversations,
    bool AiBotReopenedConversations,
    string? BotName,
    string? BotStatus,
    string? BlockReason,
    bool BotAnswers);
