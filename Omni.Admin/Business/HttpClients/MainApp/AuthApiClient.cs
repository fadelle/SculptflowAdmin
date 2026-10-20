using Microsoft.Extensions.Options;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Common.Configs;
using Omni.Admin.Entities.Dtos.Admins;
using Omni.Admin.Entities.Requests.Admins;

namespace Omni.Admin.Business.HttpClients.MainApp;

/// <summary>Client for the main app's admin sign-in API (/api/platform-admin/auth): the portal has no users table, so the
/// main app checks the password, lockout, role and active flag.</summary>
public class AuthApiClient : MainAppApiClient, IAuthApiClient
{
    public const string Prefix = "api/platform-admin/auth";

    public AuthApiClient(HttpClient http, IOptions<MainAppApiOptions> options, IHttpContextAccessor context, ILogger<AuthApiClient> logger)
        : base(http, options, context, logger, Prefix, "Sign-in")
    {
    }

    public Task<AdminIdentity?> LoginAsync(string email, string password, CancellationToken ct) =>
        PostOrRefusedAsync<AdminIdentity>("/login", new AdminLoginRequest(email, password), ct);

    public Task<AdminIdentity?> GetActiveAdminAsync(string id, CancellationToken ct) =>
        GetAsync<AdminIdentity>($"/admins/{Esc(id)}", ct);
}
