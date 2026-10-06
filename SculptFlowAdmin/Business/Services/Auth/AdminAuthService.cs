using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using SculptFlowAdmin.Business.Contracts.Services.Auth;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Persistence.Contracts;
using SculptFlowAdmin.Persistence.Contracts.Admins;

namespace SculptFlowAdmin.Business.Services.Auth;

/// <summary>
/// Sign-in for admin users (admin_users table). Deliberately separate from the main app's ASP.NET Identity
/// users: a clinic staff account can never authenticate here, and an admin account is not a clinic user.
/// Passwords use Identity's PasswordHasher (PBKDF2), the same format the main app uses for staff.
/// </summary>
public class AdminAuthService : IAdminAuthService
{
    public const int MaxFailedLogins = 5;
    public static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    public const int MinPasswordLength = 10;

    private readonly IAdminUserRepository _users;
    private readonly IAdminUnitOfWork _adminUnitOfWork;
    private readonly IPasswordHasher<AdminUser> _hasher;

    public AdminAuthService(IAdminUserRepository users, IAdminUnitOfWork adminUnitOfWork, IPasswordHasher<AdminUser> hasher)
    {
        _users = users;
        _adminUnitOfWork = adminUnitOfWork;
        _hasher = hasher;
    }

    public async Task<(AdminUser? User, string? Error)> ValidateAsync(string email, string password, CancellationToken ct = default)
    {
        const string invalid = "Wrong email or password.";
        var normalized = email.Trim().ToLowerInvariant();
        var user = await _users.FindByEmailAsync(normalized, ct);
        if (user is null)
        {
            // Hash anyway so a missing account takes as long as a wrong password.
            _hasher.HashPassword(new AdminUser(), password);
            return (null, invalid);
        }

        var now = DateTimeOffset.UtcNow;
        if (user.LockedUntil > now) return (null, "Too many failed attempts. Try again in a few minutes.");
        if (!user.IsActive) return (null, invalid);

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (result == PasswordVerificationResult.Failed)
        {
            user.FailedLogins++;
            if (user.FailedLogins >= MaxFailedLogins)
            {
                user.LockedUntil = now.Add(LockoutDuration);
                user.FailedLogins = 0;
            }
            await _adminUnitOfWork.SaveChangesAsync(ct);
            return (null, invalid);
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _hasher.HashPassword(user, password);
        }
        user.FailedLogins = 0;
        user.LockedUntil = null;
        user.LastLoginAt = now;
        await _adminUnitOfWork.SaveChangesAsync(ct);
        return (user, null);
    }

    public static string? CheckPassword(string? password) =>
        string.IsNullOrEmpty(password) || password.Length < MinPasswordLength
            ? $"Password must be at least {MinPasswordLength} characters."
            : null;

    public async Task<AdminUser> CreateAsync(string email, string fullName, string password, CancellationToken ct = default)
    {
        email = email.Trim();
        fullName = fullName.Trim();
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@')) throw new ArgumentException("Enter a valid email.");
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Enter a name.");
        var pwError = CheckPassword(password);
        if (pwError is not null) throw new ArgumentException(pwError);

        var lower = email.ToLowerInvariant();
        if (await _users.EmailExistsAsync(lower, ct))
            throw new ArgumentException("An admin with this email already exists.");

        var now = DateTimeOffset.UtcNow;
        var user = new AdminUser { Id = Guid.NewGuid(), Email = email, FullName = fullName, IsActive = true, CreatedAt = now, UpdatedAt = now };
        user.PasswordHash = _hasher.HashPassword(user, password);
        _users.Add(user);
        await _adminUnitOfWork.SaveChangesAsync(ct);
        return user;
    }

    public async Task SetPasswordAsync(Guid id, string password, CancellationToken ct = default)
    {
        var pwError = CheckPassword(password);
        if (pwError is not null) throw new ArgumentException(pwError);
        var user = await _users.GetAsync(id, ct) ?? throw new KeyNotFoundException();
        user.PasswordHash = _hasher.HashPassword(user, password);
        user.FailedLogins = 0;
        user.LockedUntil = null;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _adminUnitOfWork.SaveChangesAsync(ct);
    }

    public Task<List<AdminUser>> ListAsync(CancellationToken ct = default) =>
        _users.ListReadOnlyAsync(ct);

    public async Task SetActiveAsync(Guid id, bool active, Guid? actingAdminId, CancellationToken ct = default)
    {
        if (!active && id == actingAdminId) throw new ArgumentException("You can't deactivate your own account.");
        var user = await _users.GetAsync(id, ct) ?? throw new KeyNotFoundException();
        if (!active && user.IsActive && await _users.CountActiveAsync(ct) <= 1)
            throw new ArgumentException("At least one admin must stay active.");
        user.IsActive = active;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await _adminUnitOfWork.SaveChangesAsync(ct);
    }

    public static ClaimsPrincipal ToPrincipal(AdminUser user)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
        }, CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }

    /// <summary>
    /// Runs on every request with an admin cookie: an admin who was deactivated (or deleted) is signed out
    /// at once instead of keeping access until the cookie expires.
    /// </summary>
    public static async Task ValidatePrincipalAsync(CookieValidatePrincipalContext context)
    {
        var id = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var users = context.HttpContext.RequestServices.GetRequiredService<IAdminUserRepository>();
        var active = Guid.TryParse(id, out var guid) && await users.IsActiveAsync(guid);
        if (!active)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
