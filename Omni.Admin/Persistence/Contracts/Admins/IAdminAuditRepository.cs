using Omni.Admin.Entities.Models;
using Omni.Admin.Entities.Responses.Paging;

namespace Omni.Admin.Persistence.Contracts.Admins;

public interface IAdminAuditRepository
{
    void Add(AdminAuditEntry entry);

    /// <summary>Newest first; search matches action, admin email or entity id.</summary>
    Task<PagedResult<AdminAuditEntry>> ListAsync(Guid? clientId, string? search, int page, CancellationToken ct = default);
}
