using Omni.Admin.Entities.Responses.Caching;

namespace Omni.Admin.Business.Contracts.Services.Caching;

/// <summary>The main app's cache, through its platform-admin API; every removal is audited.</summary>
public interface ICacheAdminService
{
    Task<List<CacheKeyResponse>> ListAsync(string? prefix, CancellationToken ct);

    Task<CacheEntryResponse?> GetAsync(string key, CancellationToken ct);

    Task RemoveAsync(string key, CancellationToken ct);

    Task<int> RemoveByPrefixAsync(string prefix, CancellationToken ct);

    Task<int> ClearAsync(CancellationToken ct);
}
