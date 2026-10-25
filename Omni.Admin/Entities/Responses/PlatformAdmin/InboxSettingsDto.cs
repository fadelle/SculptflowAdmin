namespace Omni.Admin.Entities.Responses.PlatformAdmin;

/// <summary>(Copy of the main app's Entities/Dtos/Inbox shape, GET /api/platform-admin/clients/{id}/inbox-settings.) A
/// client's AI per connected account: PlanIncludesAiBot is the plan's ai_assistant entitlement, ActiveBotLimit the plan's
/// max_ai_bots plus add-ons (null = no limit: how many bots may be active at once); Accounts each connected account's AI
/// settings and outcome; Bots the client's bots to pick from (only an active one can be assigned).</summary>
public record InboxSettingsDto(
    bool PlanIncludesAiBot,
    int? ActiveBotLimit,
    IReadOnlyList<AccountAiSettingsDto> Accounts,
    IReadOnlyList<BotChoiceDto> Bots);
