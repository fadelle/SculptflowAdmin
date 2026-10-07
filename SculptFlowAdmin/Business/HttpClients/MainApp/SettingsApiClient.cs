using Microsoft.Extensions.Options;
using SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;
using SculptFlowAdmin.Common.Configs;
using SculptFlowAdmin.Entities.Requests.Configuration;
using SculptFlowAdmin.Entities.Responses.Configuration;

namespace SculptFlowAdmin.Business.HttpClients.MainApp;

/// <summary>
/// Client for the main app's platform-admin configuration API (/api/platform-admin/settings): the stored values of the
/// main app's settings (config.settings, by section and key). Pages go through <c>SettingsAdminService</c>, which also
/// audits each change.
/// </summary>
public class SettingsApiClient : MainAppApiClient, ISettingsApiClient
{
    public const string Prefix = "api/platform-admin/settings/";

    public SettingsApiClient(HttpClient http, IOptions<MainAppApiOptions> options, IHttpContextAccessor context, ILogger<SettingsApiClient> logger)
        : base(http, options, context, logger, Prefix, "Configuration")
    {
    }

    public Task<List<SettingResponse>> ListAsync(CancellationToken ct) => GetRequiredAsync<List<SettingResponse>>("", ct);

    // PUT creates or replaces, so a 404 can only mean the API is off.
    public Task SetAsync(string section, string key, SetSettingRequest request, CancellationToken ct) =>
        WriteAsync(HttpMethod.Put, $"{Esc(section)}/{Esc(key)}", request, null, true, ct);

    // 404 = nothing was stored (KeyNotFoundException).
    public Task ResetAsync(string section, string key, CancellationToken ct) =>
        WriteAsync(HttpMethod.Delete, $"{Esc(section)}/{Esc(key)}", null, null, false, ct);
}
