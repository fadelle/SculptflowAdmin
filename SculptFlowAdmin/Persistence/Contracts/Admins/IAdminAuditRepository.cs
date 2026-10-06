using SculptFlowAdmin.Entities.Models;

namespace SculptFlowAdmin.Persistence.Contracts.Admins;

public interface IAdminAuditRepository
{
    void Add(AdminAuditEntry entry);
}
