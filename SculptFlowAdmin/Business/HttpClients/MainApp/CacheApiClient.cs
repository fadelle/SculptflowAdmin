using Microsoft.Extensions.Options;
using SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;
using SculptFlowAdmin.Common.Configs;
using SculptFlowAdmin.Entities.Responses.Caching;

namespace SculptFlowAdmin.Business.HttpClients.MainApp;

/// <summary>
/// Client for the main app's platform-admin cache API (/api/platform-admin/cache). Keys contain ':' so they go in the
/// query string. Pages go through <c>CacheAdminService</c>, which also audits each removal.
/// </summary>
public class CacheApiClient : MainAppApiClient, ICacheApiClient
{
    public const string Prefix = "api/platform-admin/cache";

    public CacheApiClient(HttpClient http, IOptions<MainAppApiOptions> options, IHttpContextAccessor context, ILogger<CacheApiClient> logger)
        : base(http, options, context, logger, Prefix, "Cache")
    {
    }

    public Task<List<CacheKeyResponse>> ListAsync(string? prefix, CancellationToken ct) =>
        GetRequiredAsync<List<CacheKeyResponse>>(string.IsNullOrWhiteSpace(prefix) ? "/keys" : $"/keys?prefix={Esc(prefix)}", ct);

    public Task<CacheEntryResponse?> GetAsync(string key, CancellationToken ct) =>
        GetAsync<CacheEntryResponse>($"/key?key={Esc(key)}", ct);

    public Task RemoveAsync(string key, CancellationToken ct) =>
        WriteAsync(HttpMethod.Delete, $"/key?key={Esc(key)}", null, null, false, ct);

    // Always answers (0 when nothing matched), so a 404 can only mean the API is off.
    public Task<CacheClearResponse> RemoveByPrefixAsync(string prefix, CancellationToken ct) =>
        WriteForAsync<CacheClearResponse>(HttpMethod.Delete, $"/keys?prefix={Esc(prefix)}", null, null, true, ct);

    public Task<CacheClearResponse> ClearAsync(CancellationToken ct) =>
        WriteForAsync<CacheClearResponse>(HttpMethod.Delete, "", null, null, true, ct);
}
