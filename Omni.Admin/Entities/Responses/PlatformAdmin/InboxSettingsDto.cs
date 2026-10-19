namespace Omni.Admin.Entities.Responses.PlatformAdmin;

/// <summary>(Copy of the main app's Entities/Dtos/Inbox shape, GET /api/platform-admin/clinics/{id}/inbox-settings.) A
/// clinic's Inbox settings: PlanIncludesAiBot is the plan's ai_agent entitlement; the AI bot switches are the clinic's own
/// and only take effect while the plan includes the bot. AiBotAnswers* are the effective results.</summary>
public record InboxSettingsDto(bool PlanIncludesAiBot, bool AiBotNewConversations, bool AiBotReopenedConversations,
    bool AiBotAnswersNew, bool AiBotAnswersReopened);
