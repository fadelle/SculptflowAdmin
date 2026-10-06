using SculptFlowAdmin.Persistence.Contexts;
using SculptFlowAdmin.Persistence.Contracts;

namespace SculptFlowAdmin.Persistence.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _db;

    public UnitOfWork(ApplicationDbContext db)
    {
        _db = db;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
}
