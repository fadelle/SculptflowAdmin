using Omni.Admin.Persistence.Contexts;
using Omni.Admin.Persistence.Contracts;

namespace Omni.Admin.Persistence.Repositories;

public class AdminUnitOfWork : IAdminUnitOfWork
{
    private readonly AdminDbContext _db;

    public AdminUnitOfWork(AdminDbContext db)
    {
        _db = db;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
