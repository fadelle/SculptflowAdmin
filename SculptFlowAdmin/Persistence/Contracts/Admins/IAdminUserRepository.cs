using SculptFlowAdmin.Entities.Models;

namespace SculptFlowAdmin.Persistence.Contracts.Admins;

/// <summary>Portal accounts (admin_users). Tracked unless "ReadOnly".</summary>
public interface IAdminUserRepository
{
    /// <summary>By email, case-insensitive (pass it lowercased).</summary>
    Task<AdminUser?> FindByEmailAsync(string lowercaseEmail, CancellationToken ct = default);

    Task<bool> EmailExistsAsync(string lowercaseEmail, CancellationToken ct = default);

    Task<AdminUser?> GetAsync(Guid id, CancellationToken ct = default);

    /// <summary>Read-only, by name.</summary>
    Task<List<AdminUser>> ListReadOnlyAsync(CancellationToken ct = default);

    Task<int> CountActiveAsync(CancellationToken ct = default);

    Task<bool> IsActiveAsync(Guid id, CancellationToken ct = default);

    void Add(AdminUser user);
}
