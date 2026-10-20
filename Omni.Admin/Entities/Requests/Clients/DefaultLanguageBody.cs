namespace Omni.Admin.Entities.Requests.Clients;

/// <summary>(Copy of the main app's Entities/Requests/PlatformAdmin shape, PUT /api/platform-admin/clients/{id}/default-language.)
/// The language ("en", "ar" or "fr") the client's dashboard opens in for users who haven't picked their own.</summary>
public record DefaultLanguageBody(string? Language);
