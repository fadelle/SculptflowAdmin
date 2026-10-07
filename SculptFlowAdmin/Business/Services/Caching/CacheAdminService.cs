using SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;
using SculptFlowAdmin.Business.Contracts.Managers;
using SculptFlowAdmin.Business.Contracts.Services.Caching;
using SculptFlowAdmin.Entities.Responses.Caching;

namespace SculptFlowAdmin.Business.Services.Caching;

public class CacheAdminService : ICacheAdminService
{
    private readonly ICacheApiClient _api;
    private readonly IAdminAudit _audit;

    public CacheAdminService(ICacheApiClient api, IAdminAudit audit)
    {
        _api = api;
        _audit = audit;
    }

    public Task<List<CacheKeyResponse>> ListAsync(string? prefix, CancellationToken ct) => _api.ListAsync(prefix?.Trim(), ct);

    public Task<CacheEntryResponse?> GetAsync(string key, CancellationToken ct) => _api.GetAsync(key.Trim(), ct);

    public async Task RemoveAsync(string key, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Pick a key to remove.");
        await _api.RemoveAsync(key.Trim(), ct);
        await _audit.LogAsync("cache.key_removed", "cache_key", key.Trim(), null, null, ct);
    }

    public async Task<int> RemoveByPrefixAsync(string prefix, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(prefix)) throw new ArgumentException("Type a prefix, or use Clear all.");
        var result = await _api.RemoveByPrefixAsync(prefix.Trim(), ct);
        await _audit.LogAsync("cache.prefix_removed", "cache_key", prefix.Trim(), null, new { result.Removed }, ct);
        return result.Removed;
    }

    public async Task<int> ClearAsync(CancellationToken ct)
    {
        var result = await _api.ClearAsync(ct);
        await _audit.LogAsync("cache.cleared", "cache", "all", null, new { result.Removed }, ct);
        return result.Removed;
    }
}
