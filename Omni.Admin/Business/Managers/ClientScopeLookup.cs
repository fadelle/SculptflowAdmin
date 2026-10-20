using Microsoft.Extensions.Caching.Memory;
using Omni.Admin.Business.Contracts.Managers;
using Omni.Admin.Business.Contracts.Services.Clients;
using Omni.Admin.Entities.Dtos.Clients;
using Omni.Admin.Entities.Dtos.Scoping;

namespace Omni.Admin.Business.Managers;

/// <summary>
/// Clients for the header's scope selector, over the existing client options (the list the per-page client filters
/// used). The list is cached briefly because the scope chip needs the selected client's name on every page.
/// </summary>
public class ClientScopeLookup : IScopeLookup
{
    private const string CacheKey = "scope:client-options";
    private static readonly TimeSpan CacheFor = TimeSpan.FromMinutes(2);

    private readonly IClientAdminService _clients;
    private readonly IMemoryCache _cache;

    public ClientScopeLookup(IClientAdminService clients, IMemoryCache cache)
    {
        _clients = clients;
        _cache = cache;
    }

    public async Task<string?> GetNameAsync(string id, CancellationToken ct) =>
        Guid.TryParse(id, out var clientId) ? (await AllAsync(ct)).FirstOrDefault(c => c.Id == clientId)?.Name : null;

    public async Task<ScopeOptionPage> SearchAsync(string? q, int skip, int take, CancellationToken ct)
    {
        var term = q?.Trim();
        var matches = (await AllAsync(ct))
            .Where(c => string.IsNullOrEmpty(term) || c.Name.Contains(term, StringComparison.OrdinalIgnoreCase))
            .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var page = matches.Skip(skip).Take(take).Select(c => new ScopeOption(c.Id.ToString(), c.Name)).ToList();
        return new ScopeOptionPage(page, skip + page.Count < matches.Count);
    }

    private async Task<List<ClientOption>> AllAsync(CancellationToken ct) =>
        await _cache.GetOrCreateAsync(CacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheFor;
            return _clients.OptionsAsync(ct);
        }) ?? new List<ClientOption>();
}
