using Microsoft.EntityFrameworkCore;
using SculptFlowAdmin.Common.Helpers;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Persistence.Contexts;
using SculptFlowAdmin.Persistence.Contracts.Admins;
using SculptFlowAdmin.Persistence.Helpers;

namespace SculptFlowAdmin.Persistence.Repositories.Admins;

public class AdminAuditRepository : IAdminAuditRepository
{
    private readonly AdminDbContext _db;

    public AdminAuditRepository(AdminDbContext db)
    {
        _db = db;
    }

    public void Add(AdminAuditEntry entry) => _db.AuditLog.Add(entry);

    public Task<PagedResult<AdminAuditEntry>> ListAsync(Guid? clinicId, string? search, int page, CancellationToken ct = default)
    {
        var q = _db.AuditLog.AsNoTracking().AsQueryable();
        if (clinicId.HasValue) q = q.Where(a => a.ClinicId == clinicId.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var like = Paging.Like(search);
            q = q.Where(a => EF.Functions.ILike(a.Action, like) || EF.Functions.ILike(a.AdminEmail, like)
                             || (a.EntityId != null && EF.Functions.ILike(a.EntityId, like)));
        }
        return q.OrderByDescending(a => a.CreatedAt).ToPagedAsync(page, ct: ct);
    }
}
