using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Entities.Responses.Paging;

namespace SculptFlowAdmin.Persistence.Contracts.Admins;

public interface IAdminAuditRepository
{
    void Add(AdminAuditEntry entry);

    /// <summary>Newest first; search matches action, admin email or entity id.</summary>
    Task<PagedResult<AdminAuditEntry>> ListAsync(Guid? clinicId, string? search, int page, CancellationToken ct = default);
}
