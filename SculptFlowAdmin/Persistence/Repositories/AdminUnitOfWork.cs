using SculptFlowAdmin.Persistence.Contexts;
using SculptFlowAdmin.Persistence.Contracts;

namespace SculptFlowAdmin.Persistence.Repositories;

public class AdminUnitOfWork : IAdminUnitOfWork
{
    private readonly AdminDbContext _db;

    public AdminUnitOfWork(AdminDbContext db)
    {
        _db = db;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
