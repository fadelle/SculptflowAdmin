namespace SculptFlowAdmin.Entities.Responses.Configuration;

// Copy of the main app's Entities/Responses/Configuration/SettingResponse (GET /api/platform-admin/settings).
// If the shape changes there, change it here (see MAIN_APP_SYNC.md).
public record SettingResponse(
    string Section,
    string Key,
    string Type,
    string Description,
    string DefaultValue,
    string? StoredValue,
    string EffectiveValue,
    decimal? Min,
    decimal? Max,
    string? Note,
    string? UpdatedBy,
    DateTimeOffset? UpdatedAt,
    bool IsSecret);
