using Omni.Admin.Entities.Dtos.Admins;

namespace Omni.Admin.Business.Contracts.Services.Auth;

/// <summary>Portal sign-in through the main app (the portal has no users table). The cookie hooks stay static on
/// AdminAuthService.</summary>
public interface IAdminAuthService
{
    /// <summary>Checks the email and password with the main app. Returns the admin, or one generic message for every refusal
    /// (no hint whether the email exists or is locked), or an "unavailable" message when the main app can't answer.</summary>
    Task<AdminSignInResult> SignInAsync(string? email, string? password, CancellationToken ct = default);

    /// <summary>The admin while still an active admin, else null. A "yes" is remembered for <c>AdminAuthService.RecheckAfter</c>.
    /// Throws MainAppApiException when the main app can't answer.</summary>
    Task<AdminIdentity?> GetActiveAdminAsync(string id, CancellationToken ct = default);

    /// <summary>Drops the remembered answer for this admin, so the next request asks the main app again.</summary>
    void Forget(string id);
}
