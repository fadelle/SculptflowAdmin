using SculptFlowAdmin.Entities.Dtos.Staff;
using SculptFlowAdmin.Entities.Responses.Paging;

namespace SculptFlowAdmin.Business.Contracts.Services.Staff;

/// <summary>Staff accounts, through the main app's staff API; every change is audited.</summary>
public interface IStaffAdminService
{
    Task<PagedResult<StaffRow>> ListAsync(Guid? clinicId, string? search, int page, CancellationToken ct = default);

    Task<StaffRow?> GetAsync(string userId, CancellationToken ct = default);

    Task SetMembershipActiveAsync(string userId, bool active, CancellationToken ct = default);

    Task SetLoginLockedAsync(string userId, bool locked, CancellationToken ct = default);

    /// <summary>The main app checks it against its password rules.</summary>
    Task SetPasswordAsync(string userId, string password, CancellationToken ct = default);

    Task SetEmailConfirmedAsync(string userId, bool confirmed, CancellationToken ct = default);
}
