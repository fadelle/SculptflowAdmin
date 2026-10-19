using Omni.Admin.Entities.Responses.Caching;

namespace Omni.Admin.Business.Contracts.HttpClients.MainApp;

public interface ICacheApiClient
{
    Task<List<CacheKeyResponse>> ListAsync(string? prefix, CancellationToken ct);

    /// <summary>Null when the key isn't cached.</summary>
    Task<CacheEntryResponse?> GetAsync(string key, CancellationToken ct);

    /// <summary>Throws KeyNotFoundException when the key isn't cached.</summary>
    Task RemoveAsync(string key, CancellationToken ct);

    Task<CacheClearResponse> RemoveByPrefixAsync(string prefix, CancellationToken ct);

    Task<CacheClearResponse> ClearAsync(CancellationToken ct);
}
