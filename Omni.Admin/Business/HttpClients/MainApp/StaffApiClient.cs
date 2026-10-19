using Microsoft.Extensions.Options;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Common.Configs;
using Omni.Admin.Entities.Dtos.Staff;
using Omni.Admin.Entities.Requests.Content;
using Omni.Admin.Entities.Requests.PlatformAdmin;
using Omni.Admin.Entities.Requests.Staff;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Business.HttpClients.MainApp;

/// <summary>Client for the main app's platform-admin staff API (/api/platform-admin/staff).</summary>
public class StaffApiClient : MainAppApiClient, IStaffApiClient
{
    public const string Prefix = "api/platform-admin/staff";

    public StaffApiClient(HttpClient http, IOptions<MainAppApiOptions> options, IHttpContextAccessor context, ILogger<StaffApiClient> logger)
        : base(http, options, context, logger, Prefix, "Staff")
    {
    }

    public Task<PagedResult<StaffRow>> ListAsync(Guid? clinicId, string? search, int page, CancellationToken ct) =>
        GetRequiredAsync<PagedResult<StaffRow>>(Query(("clinicId", clinicId), ("search", search), ("page", page)), ct);

    public Task<StaffRow?> GetAsync(string userId, CancellationToken ct) => GetAsync<StaffRow>($"/{Esc(userId)}", ct);

    public Task<PlatformAdminChange> SetMembershipActiveAsync(string userId, bool active, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/{Esc(userId)}/membership-active", new ActiveBody(active), null, false, ct);

    public Task<PlatformAdminChange> SetLoginLockedAsync(string userId, bool locked, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/{Esc(userId)}/locked", new FlagBody(locked), null, false, ct);

    public Task<PlatformAdminChange> SetPasswordAsync(string userId, string password, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/{Esc(userId)}/password", new PasswordBody(password), null, false, ct);

    public Task<PlatformAdminChange> SetEmailConfirmedAsync(string userId, bool confirmed, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/{Esc(userId)}/email-confirmed", new FlagBody(confirmed), null, false, ct);
}
