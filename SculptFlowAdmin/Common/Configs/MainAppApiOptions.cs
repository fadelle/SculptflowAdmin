using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace SculptFlowAdmin.Common.Configs;

/// <summary>"MainApp" config used to reach the main app's platform-admin API.</summary>
public class MainAppApiOptions
{
    /// <summary>Base URL the portal calls server to server. Empty = MainApp:PublicBaseUrl.</summary>
    public string? ApiBaseUrl { get; set; }
    public string? PublicBaseUrl { get; set; }

    /// <summary>SECRET: the main app's PlatformAdmin:ApiKey, sent as X-Platform-Admin-Key. Set via the
    /// MainApp__PlatformAdminApiKey env var or user-secrets, never in appsettings.json.</summary>
    public string? PlatformAdminApiKey { get; set; }

    public string? BaseUrl => (string.IsNullOrWhiteSpace(ApiBaseUrl) ? PublicBaseUrl : ApiBaseUrl)?.Trim().TrimEnd('/');
}
