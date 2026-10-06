using Microsoft.AspNetCore.Identity;
using SculptFlowAdmin.Business.Contracts.Managers;
using SculptFlowAdmin.Business.Contracts.Services.Staff;
using SculptFlowAdmin.Common.Exceptions;
using SculptFlowAdmin.Common.Statics;
using SculptFlowAdmin.Entities.Dtos.Staff;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Persistence.Contracts;
using SculptFlowAdmin.Persistence.Contracts.Staff;

namespace SculptFlowAdmin.Business.Services.Staff;

/// <summary>
/// Clinic staff accounts: the main app's ASP.NET Identity users (identity_users) and their clinic_users membership.
/// Two independent switches, both honoured by the main app as it is today:
///   * Membership active — clinic_users.is_active. Inactive = the user still signs in but has no clinic (sees nothing).
///   * Login locked — Identity lockout (lockout_end far in the future). SignInManager refuses a locked account for
///     both password and Google sign-in. The security stamp is rotated too, but the main app's cookie has no
///     SecurityStampValidator today, so an already-open session lasts until its cookie expires. Deactivating the
///     membership is what cuts an open session off from clinic data immediately.
/// </summary>
public class StaffAdminService : IStaffAdminService
{
    public const string FullNameClaimType = StaffClaims.FullName;

    private readonly IStaffAdminRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAdminAudit _audit;
    private readonly IPasswordHasher<IdentityUser> _hasher;

    public StaffAdminService(IStaffAdminRepository repository, IUnitOfWork unitOfWork, IAdminAudit audit, IPasswordHasher<IdentityUser> hasher)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _audit = audit;
        _hasher = hasher;
    }

    public Task<PagedResult<StaffRow>> ListAsync(Guid? clinicId, string? search, int page, CancellationToken ct = default) =>
        _repository.ListAsync(clinicId, search, page, ct);

    public Task<StaffRow?> GetAsync(string userId, CancellationToken ct = default) =>
        _repository.GetAsync(userId, ct);

    public async Task SetMembershipActiveAsync(string userId, bool active, CancellationToken ct = default)
    {
        var m = await _repository.GetLatestMembershipAsync(userId, ct)
                ?? throw new AdminRuleException("This user has no clinic membership.");
        if (m.IsActive == active) return;
        if (active && await _repository.HasOtherActiveMembershipAsync(userId, m.Id, ct))
            throw new AdminRuleException("This user already has another active clinic membership.");
        m.IsActive = active;
        m.UpdatedAt = DateTimeOffset.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.LogAsync(active ? "staff.membership_activated" : "staff.membership_deactivated", "clinic_user", userId, m.ClinicId, ct: ct);
    }

    public async Task SetLoginLockedAsync(string userId, bool locked, CancellationToken ct = default)
    {
        var u = await _repository.GetUserAsync(userId, ct) ?? throw new KeyNotFoundException();
        u.LockoutEnabled = true;
        u.LockoutEnd = locked ? DateTimeOffset.UtcNow.AddYears(100) : null;
        u.AccessFailedCount = 0;
        if (locked) u.SecurityStamp = Guid.NewGuid().ToString("N").ToUpperInvariant();
        u.ConcurrencyStamp = Guid.NewGuid().ToString();
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.LogAsync(locked ? "staff.login_locked" : "staff.login_unlocked", "identity_user", userId,
            await _repository.ClinicOfAsync(userId, ct), ct: ct);
    }

    public async Task SetPasswordAsync(string userId, string password, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8) throw new AdminRuleException("Password must be at least 8 characters.");
        var u = await _repository.GetUserAsync(userId, ct) ?? throw new KeyNotFoundException();
        u.PasswordHash = _hasher.HashPassword(u, password);
        u.SecurityStamp = Guid.NewGuid().ToString("N").ToUpperInvariant();
        u.ConcurrencyStamp = Guid.NewGuid().ToString();
        u.AccessFailedCount = 0;
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.LogAsync("staff.password_reset", "identity_user", userId, await _repository.ClinicOfAsync(userId, ct), ct: ct);
    }

    public async Task SetEmailConfirmedAsync(string userId, bool confirmed, CancellationToken ct = default)
    {
        var u = await _repository.GetUserAsync(userId, ct) ?? throw new KeyNotFoundException();
        u.EmailConfirmed = confirmed;
        u.ConcurrencyStamp = Guid.NewGuid().ToString();
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.LogAsync(confirmed ? "staff.email_confirmed" : "staff.email_unconfirmed", "identity_user", userId,
            await _repository.ClinicOfAsync(userId, ct), ct: ct);
    }

}
