using SculptFlowAdmin.Entities.Dtos.Staff;
using SculptFlowAdmin.Entities.Responses.Paging;

namespace SculptFlowAdmin.Business.Contracts.Services.Staff;

public interface IStaffAdminService
{
    Task<PagedResult<StaffRow>> ListAsync(Guid? clinicId, string? search, int page, CancellationToken ct = default);

    Task<StaffRow?> GetAsync(string userId, CancellationToken ct = default);

    Task SetMembershipActiveAsync(string userId, bool active, CancellationToken ct = default);

    Task SetLoginLockedAsync(string userId, bool locked, CancellationToken ct = default);

    Task SetPasswordAsync(string userId, string password, CancellationToken ct = default);

    Task SetEmailConfirmedAsync(string userId, bool confirmed, CancellationToken ct = default);
}
