using Microsoft.AspNetCore.Identity;
using SculptFlowAdmin.Entities.Dtos.Staff;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Entities.Responses.Paging;

namespace SculptFlowAdmin.Persistence.Contracts.Staff;

public interface IStaffAdminRepository
{
    Task<PagedResult<StaffRow>> ListAsync(Guid? clinicId, string? search, int page, CancellationToken ct = default);

    Task<StaffRow?> GetAsync(string userId, CancellationToken ct = default);

    /// <summary>Tracked: the user's most recently updated membership.</summary>
    Task<ClinicUser?> GetLatestMembershipAsync(string userId, CancellationToken ct = default);

    Task<bool> HasOtherActiveMembershipAsync(string userId, Guid exceptMembershipId, CancellationToken ct = default);

    /// <summary>Tracked Identity user.</summary>
    Task<IdentityUser?> GetUserAsync(string userId, CancellationToken ct = default);

    /// <summary>The clinic of the user's membership (an active one first).</summary>
    Task<Guid?> ClinicOfAsync(string userId, CancellationToken ct = default);
}
