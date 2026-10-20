namespace Omni.Admin.Persistence.Contracts;

/// <summary>Saves pending changes to the portal's own table (AdminDbContext: admin_audit_log). Admins are the main app's users.</summary>
public interface IAdminUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
