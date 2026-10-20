namespace Omni.Admin.Entities.Requests.Admins;

/// <summary>Body for the main app's POST /api/platform-admin/admins.</summary>
public record CreateAdminRequest(string? Email, string? FullName, string? Password);
