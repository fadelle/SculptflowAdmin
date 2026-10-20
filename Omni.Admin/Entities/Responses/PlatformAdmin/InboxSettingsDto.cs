namespace Omni.Admin.Entities.Responses.PlatformAdmin;

/// <summary>(Copy of the main app's Entities/Dtos/Inbox shape, GET /api/platform-admin/clients/{id}/inbox-settings.) A
/// client's Inbox settings: PlanIncludesAiBot is the plan's ai_assistant entitlement; the AI bot switches are the client's own
/// and only take effect while the plan includes the bot. AiBotAnswers* are the effective results.</summary>
public record InboxSettingsDto(bool PlanIncludesAiBot, bool AiBotNewConversations, bool AiBotReopenedConversations,
    bool AiBotAnswersNew, bool AiBotAnswersReopened);
