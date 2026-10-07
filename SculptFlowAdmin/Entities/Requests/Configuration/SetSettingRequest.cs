namespace SculptFlowAdmin.Entities.Requests.Configuration;

// Copy of the main app's Entities/Requests/Configuration/SetSettingRequest (PUT /api/platform-admin/settings/{section}/{key}).
public record SetSettingRequest(string? Value, string? Note);
