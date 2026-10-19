using Omni.Admin.Entities.Dtos.Staff;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Business.Contracts.HttpClients.MainApp;

public interface IStaffApiClient
{
    Task<PagedResult<StaffRow>> ListAsync(Guid? clinicId, string? search, int page, CancellationToken ct);

    Task<StaffRow?> GetAsync(string userId, CancellationToken ct);

    Task<PlatformAdminChange> SetMembershipActiveAsync(string userId, bool active, CancellationToken ct);

    Task<PlatformAdminChange> SetLoginLockedAsync(string userId, bool locked, CancellationToken ct);

    Task<PlatformAdminChange> SetPasswordAsync(string userId, string password, CancellationToken ct);

    Task<PlatformAdminChange> SetEmailConfirmedAsync(string userId, bool confirmed, CancellationToken ct);
}
