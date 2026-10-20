namespace Omni.Admin.Entities.Requests.Admins;

/// <summary>Body for the main app's POST /api/platform-admin/auth/login: what the admin typed on the sign-in page.</summary>
public record AdminLoginRequest(string? Email, string? Password);
