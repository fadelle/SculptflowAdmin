namespace Omni.Admin.Entities.Requests.Admins;

/// <summary>Body for the main app's PUT /api/platform-admin/admins/{id}/active. <c>ActingAdminId</c> is the signed-in admin,
/// so the main app can refuse deactivating your own account.</summary>
public record AdminActiveBody(bool Active, string? ActingAdminId);
