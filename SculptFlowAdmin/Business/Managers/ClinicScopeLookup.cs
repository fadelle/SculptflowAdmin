using Microsoft.Extensions.Caching.Memory;
using SculptFlowAdmin.Business.Contracts.Managers;
using SculptFlowAdmin.Business.Contracts.Services.Clinics;
using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Dtos.Scoping;

namespace SculptFlowAdmin.Business.Managers;

/// <summary>
/// Clinics for the header's scope selector, over the existing clinic options (the list the per-page clinic filters
/// used). The list is cached briefly because the scope chip needs the selected clinic's name on every page.
/// </summary>
public class ClinicScopeLookup : IScopeLookup
{
    private const string CacheKey = "scope:clinic-options";
    private static readonly TimeSpan CacheFor = TimeSpan.FromMinutes(2);

    private readonly IClinicAdminService _clinics;
    private readonly IMemoryCache _cache;

    public ClinicScopeLookup(IClinicAdminService clinics, IMemoryCache cache)
    {
        _clinics = clinics;
        _cache = cache;
    }

    public async Task<string?> GetNameAsync(string id, CancellationToken ct) =>
        Guid.TryParse(id, out var clinicId) ? (await AllAsync(ct)).FirstOrDefault(c => c.Id == clinicId)?.Name : null;

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

    private async Task<List<ClinicOption>> AllAsync(CancellationToken ct) =>
        await _cache.GetOrCreateAsync(CacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheFor;
            return _clinics.OptionsAsync(ct);
        }) ?? new List<ClinicOption>();
}
