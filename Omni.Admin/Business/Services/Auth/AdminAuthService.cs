using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Contracts.Services.Auth;
using Omni.Admin.Common.Exceptions;
using Omni.Admin.Entities.Dtos.Admins;

namespace Omni.Admin.Business.Services.Auth;

/// <summary>
/// Admin sign-in. Admins are users of the main app (identity.users, role admin): the main app checks the password, the
/// lockout, the role and the active flag (POST /api/platform-admin/auth/login), and this portal issues its own cookie
/// (omni.admin) with the admin's id, name and email. Every request asks the main app again whether that admin is still an
/// active admin (GET .../auth/admins/{id}, remembered for <see cref="RecheckAfter"/>), so a deactivated or demoted admin is
/// signed out on their next click. A client or member account can never sign in here.
/// </summary>
public class AdminAuthService : IAdminAuthService
{
    /// <summary>The one message for every refused sign-in (the main app's wording; it answers one 401 for all of them).</summary>
    public const string SignInFailed = "Wrong email or password, or the account is locked for a few minutes.";

    public const string SignInUnavailable = "Sign-in isn't available right now: the portal couldn't use the main app's admin API. " +
        "Try again in a moment; if it keeps failing, check the portal's MainApp settings.";

    /// <summary>How long the main app's "still an active admin" answer is trusted before it is asked again.</summary>
    public static readonly TimeSpan RecheckAfter = TimeSpan.FromMinutes(1);

    private readonly IAuthApiClient _api;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AdminAuthService> _logger;

    public AdminAuthService(IAuthApiClient api, IMemoryCache cache, ILogger<AdminAuthService> logger)
    {
        _api = api;
        _cache = cache;
        _logger = logger;
    }

    public async Task<AdminSignInResult> SignInAsync(string? email, string? password, CancellationToken ct = default)
    {
        email = (email ?? string.Empty).Trim();
        if (email.Length == 0 || string.IsNullOrEmpty(password)) return new(null, SignInFailed);
        try
        {
            var admin = await _api.LoginAsync(email, password, ct);
            if (admin is null) return new(null, SignInFailed);
            _cache.Set(CacheKey(admin.Id), admin, RecheckAfter);
            return new(admin, null);
        }
        catch (MainAppApiException ex)
        {
            _logger.LogWarning("Admin sign-in couldn't use the main app: {Reason}", ex.Message);
            return new(null, SignInUnavailable);
        }
    }

    public async Task<AdminIdentity?> GetActiveAdminAsync(string id, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(CacheKey(id), out AdminIdentity? known) && known is not null) return known;
        var admin = await _api.GetActiveAdminAsync(id, ct);
        // Only a "yes" is remembered: a refused admin is signed out, and a later sign-in starts fresh.
        if (admin is not null) _cache.Set(CacheKey(id), admin, RecheckAfter);
        return admin;
    }

    public void Forget(string id) => _cache.Remove(CacheKey(id));

    public static ClaimsPrincipal ToPrincipal(AdminIdentity admin)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, admin.Id),
            new Claim(ClaimTypes.Email, admin.Email ?? string.Empty),
            new Claim(ClaimTypes.Name, DisplayName(admin)),
        }, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }

    /// <summary>
    /// Runs on every request with an admin cookie. The main app says no (deactivated, no longer an admin, gone) → the cookie
    /// is rejected and removed. A changed name or email is written into a renewed cookie. When the main app can't answer, the
    /// cookie is kept: every page except the audit log reads through the main app anyway (so it shows the Unavailable page),
    /// and a short outage doesn't sign everyone out.
    /// </summary>
    public static async Task ValidatePrincipalAsync(CookieValidatePrincipalContext context)
    {
        var services = context.HttpContext.RequestServices;
        var id = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        AdminIdentity? admin = null;
        if (!string.IsNullOrEmpty(id))
        {
            try
            {
                admin = await services.GetRequiredService<IAdminAuthService>().GetActiveAdminAsync(id, context.HttpContext.RequestAborted);
            }
            catch (MainAppApiException ex)
            {
                services.GetRequiredService<ILogger<AdminAuthService>>()
                    .LogWarning("Couldn't recheck admin {AdminId} with the main app; the cookie is kept: {Reason}", id, ex.Message);
                return;
            }
        }

        if (admin is null)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return;
        }

        var current = context.Principal!;
        if (current.FindFirstValue(ClaimTypes.Email) != (admin.Email ?? string.Empty)
            || current.FindFirstValue(ClaimTypes.Name) != DisplayName(admin))
        {
            context.ReplacePrincipal(ToPrincipal(admin));
            context.ShouldRenew = true;
        }
    }

    private static string DisplayName(AdminIdentity admin) =>
        string.IsNullOrWhiteSpace(admin.FullName) ? admin.Email ?? string.Empty : admin.FullName;

    private static string CacheKey(string id) => "admin-portal:active-admin:" + id;
}
