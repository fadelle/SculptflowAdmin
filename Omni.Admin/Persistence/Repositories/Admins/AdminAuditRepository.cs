using Microsoft.EntityFrameworkCore;
using Omni.Admin.Common.Helpers;
using Omni.Admin.Entities.Models;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Persistence.Contexts;
using Omni.Admin.Persistence.Contracts.Admins;
using Omni.Admin.Persistence.Helpers;

namespace Omni.Admin.Persistence.Repositories.Admins;

public class AdminAuditRepository : IAdminAuditRepository
{
    private readonly AdminDbContext _db;

    public AdminAuditRepository(AdminDbContext db)
    {
        _db = db;
    }

    public void Add(AdminAuditEntry entry) => _db.AuditLog.Add(entry);

    public Task<PagedResult<AdminAuditEntry>> ListAsync(Guid? clientId, string? search, int page, CancellationToken ct = default)
    {
        var q = _db.AuditLog.AsNoTracking().AsQueryable();
        if (clientId.HasValue) q = q.Where(a => a.ClientId == clientId.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var like = Paging.Like(search);
            q = q.Where(a => EF.Functions.ILike(a.Action, like) || EF.Functions.ILike(a.AdminEmail, like)
                             || (a.EntityId != null && EF.Functions.ILike(a.EntityId, like)));
        }
        return q.OrderByDescending(a => a.CreatedAt).ToPagedAsync(page, ct: ct);
    }
}
