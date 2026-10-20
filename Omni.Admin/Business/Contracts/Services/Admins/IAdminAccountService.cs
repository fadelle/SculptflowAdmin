using Omni.Admin.Entities.Dtos.Admins;

namespace Omni.Admin.Business.Contracts.Services.Admins;

/// <summary>The Admins page: admin users kept by the main app (/api/platform-admin/admins); each change is audited here.</summary>
public interface IAdminAccountService
{
    Task<IReadOnlyList<AdminRow>> ListAsync(CancellationToken ct = default);

    Task<AdminRow> CreateAsync(string? email, string? fullName, string? password, CancellationToken ct = default);

    /// <summary>The main app refuses deactivating <paramref name="actingAdminId"/> itself or the last active admin.</summary>
    Task<AdminRow> SetActiveAsync(string id, bool active, string? actingAdminId, CancellationToken ct = default);

    Task<AdminRow> SetPasswordAsync(string id, string? password, CancellationToken ct = default);
}
