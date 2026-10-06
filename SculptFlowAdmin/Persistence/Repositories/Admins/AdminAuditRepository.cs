using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Persistence.Contexts;
using SculptFlowAdmin.Persistence.Contracts.Admins;

namespace SculptFlowAdmin.Persistence.Repositories.Admins;

public class AdminAuditRepository : IAdminAuditRepository
{
    private readonly AdminDbContext _db;

    public AdminAuditRepository(AdminDbContext db)
    {
        _db = db;
    }

    public void Add(AdminAuditEntry entry) => _db.AuditLog.Add(entry);
}
