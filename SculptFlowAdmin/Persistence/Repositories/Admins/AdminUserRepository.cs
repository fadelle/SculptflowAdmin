using Microsoft.EntityFrameworkCore;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Persistence.Contexts;
using SculptFlowAdmin.Persistence.Contracts.Admins;

namespace SculptFlowAdmin.Persistence.Repositories.Admins;

public class AdminUserRepository : IAdminUserRepository
{
    private readonly AdminDbContext _db;

    public AdminUserRepository(AdminDbContext db)
    {
        _db = db;
    }

    public Task<AdminUser?> FindByEmailAsync(string lowercaseEmail, CancellationToken ct = default) =>
        _db.AdminUsers.FirstOrDefaultAsync(u => u.Email.ToLower() == lowercaseEmail, ct);

    public Task<bool> EmailExistsAsync(string lowercaseEmail, CancellationToken ct = default) =>
        _db.AdminUsers.AnyAsync(u => u.Email.ToLower() == lowercaseEmail, ct);

    public Task<AdminUser?> GetAsync(Guid id, CancellationToken ct = default) =>
        _db.AdminUsers.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<List<AdminUser>> ListReadOnlyAsync(CancellationToken ct = default) =>
        _db.AdminUsers.AsNoTracking().OrderBy(u => u.FullName).ToListAsync(ct);

    public Task<int> CountActiveAsync(CancellationToken ct = default) => _db.AdminUsers.CountAsync(u => u.IsActive, ct);

    public Task<bool> IsActiveAsync(Guid id, CancellationToken ct = default) =>
        _db.AdminUsers.AnyAsync(u => u.Id == id && u.IsActive, ct);

    public void Add(AdminUser user) => _db.AdminUsers.Add(user);
}
