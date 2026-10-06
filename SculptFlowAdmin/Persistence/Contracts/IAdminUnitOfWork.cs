namespace SculptFlowAdmin.Persistence.Contracts;

/// <summary>Saves pending changes to the portal's own tables (AdminDbContext: admin_users, admin_audit_log).</summary>
public interface IAdminUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
