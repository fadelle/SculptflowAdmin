using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Contracts.Managers;
using Omni.Admin.Business.Contracts.Services.Staff;
using Omni.Admin.Entities.Dtos.Staff;
using Omni.Admin.Entities.Responses.Paging;

namespace Omni.Admin.Business.Services.Staff;

public class StaffAdminService : IStaffAdminService
{
    private readonly IStaffApiClient _api;
    private readonly IAdminAudit _audit;

    public StaffAdminService(IStaffApiClient api, IAdminAudit audit)
    {
        _api = api;
        _audit = audit;
    }

    public Task<PagedResult<StaffRow>> ListAsync(Guid? clinicId, string? search, int page, CancellationToken ct = default) =>
        _api.ListAsync(clinicId, search, page, ct);

    public Task<StaffRow?> GetAsync(string userId, CancellationToken ct = default) => _api.GetAsync(userId, ct);

    public async Task SetMembershipActiveAsync(string userId, bool active, CancellationToken ct = default)
    {
        var change = await _api.SetMembershipActiveAsync(userId, active, ct);
        await _audit.LogAsync(active ? "staff.membership_activated" : "staff.membership_deactivated", "clinic_user", userId, change.ClinicId, ct: ct);
    }

    public async Task SetLoginLockedAsync(string userId, bool locked, CancellationToken ct = default)
    {
        var change = await _api.SetLoginLockedAsync(userId, locked, ct);
        await _audit.LogAsync(locked ? "staff.login_locked" : "staff.login_unlocked", "identity_user", userId, change.ClinicId, ct: ct);
    }

    public async Task SetPasswordAsync(string userId, string password, CancellationToken ct = default)
    {
        var change = await _api.SetPasswordAsync(userId, password, ct);
        await _audit.LogAsync("staff.password_reset", "identity_user", userId, change.ClinicId, ct: ct);
    }

    public async Task SetEmailConfirmedAsync(string userId, bool confirmed, CancellationToken ct = default)
    {
        var change = await _api.SetEmailConfirmedAsync(userId, confirmed, ct);
        await _audit.LogAsync(confirmed ? "staff.email_confirmed" : "staff.email_unconfirmed", "identity_user", userId, change.ClinicId, ct: ct);
    }
}
