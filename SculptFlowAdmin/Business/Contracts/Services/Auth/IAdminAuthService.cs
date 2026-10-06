using SculptFlowAdmin.Entities.Models;

namespace SculptFlowAdmin.Business.Contracts.Services.Auth;

/// <summary>Portal sign-in and admin accounts. Constants and the cookie hooks stay static on AdminAuthService.</summary>
public interface IAdminAuthService
{
    /// <summary>Checks an admin's email and password, applying the failed-login lockout. Returns the user, or a message
    /// for the sign-in form.</summary>
    Task<(AdminUser? User, string? Error)> ValidateAsync(string email, string password, CancellationToken ct = default);

    Task<AdminUser> CreateAsync(string email, string fullName, string password, CancellationToken ct = default);

    Task SetPasswordAsync(Guid id, string password, CancellationToken ct = default);

    Task<List<AdminUser>> ListAsync(CancellationToken ct = default);

    Task SetActiveAsync(Guid id, bool active, Guid? actingAdminId, CancellationToken ct = default);
}
