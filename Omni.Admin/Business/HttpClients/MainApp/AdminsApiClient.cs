using Microsoft.Extensions.Options;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Common.Configs;
using Omni.Admin.Entities.Dtos.Admins;
using Omni.Admin.Entities.Requests.Admins;
using Omni.Admin.Entities.Requests.Members;

namespace Omni.Admin.Business.HttpClients.MainApp;

/// <summary>Client for the main app's admins API (/api/platform-admin/admins): admin users live in the main app's
/// identity.users with role admin. Writes answer the admin's row.</summary>
public class AdminsApiClient : MainAppApiClient, IAdminsApiClient
{
    public const string Prefix = "api/platform-admin/admins";

    public AdminsApiClient(HttpClient http, IOptions<MainAppApiOptions> options, IHttpContextAccessor context, ILogger<AdminsApiClient> logger)
        : base(http, options, context, logger, Prefix, "Admins")
    {
    }

    public async Task<IReadOnlyList<AdminRow>> ListAsync(CancellationToken ct) =>
        await GetRequiredAsync<List<AdminRow>>("", ct);

    public Task<AdminRow> CreateAsync(string? email, string? fullName, string? password, CancellationToken ct) =>
        WriteForAsync<AdminRow>(HttpMethod.Post, "", new CreateAdminRequest(email, fullName, password), null, true, ct);

    public Task<AdminRow> SetActiveAsync(string id, bool active, string? actingAdminId, CancellationToken ct) =>
        WriteForAsync<AdminRow>(HttpMethod.Put, $"/{Esc(id)}/active", new AdminActiveBody(active, actingAdminId), null, false, ct);

    public Task<AdminRow> SetPasswordAsync(string id, string? password, CancellationToken ct) =>
        WriteForAsync<AdminRow>(HttpMethod.Put, $"/{Esc(id)}/password", new PasswordBody(password ?? string.Empty), null, false, ct);
}
