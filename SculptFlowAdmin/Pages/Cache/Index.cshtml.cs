using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Caching;
using SculptFlowAdmin.Entities.Responses.Caching;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Cache;

/// <summary>The main app's cache, through its cache API: list keys, view one, remove one or a prefix, clear all.</summary>
public class IndexModel : MainAppPageModel
{
    private readonly ICacheAdminService _cache;

    public IndexModel(ICacheAdminService cache) => _cache = cache;

    public string? Prefix { get; set; }
    public List<CacheKeyResponse> Keys { get; private set; } = new();
    public CacheEntryResponse? Selected { get; private set; }
    public string? SelectedKey { get; private set; }

    public async Task OnGetAsync(string? prefix, string? key, CancellationToken ct)
    {
        Prefix = Clean(prefix);
        SelectedKey = Clean(key);
        if (!await LoadAsync(async () => Keys = await _cache.ListAsync(Prefix, ct))) return;
        if (SelectedKey is not null) await LoadAsync(async () => Selected = await _cache.GetAsync(SelectedKey, ct));
    }

    public Task<IActionResult> OnPostRemoveAsync(string? key, string? prefix, CancellationToken ct) =>
        RunAsync(() => _cache.RemoveAsync(key ?? "", ct), $"{key} removed. The main app loads it again on next use.", new { prefix });

    public async Task<IActionResult> OnPostRemovePrefixAsync(string? prefix, CancellationToken ct)
    {
        var removed = 0;
        var result = await RunAsync(async () => removed = await _cache.RemoveByPrefixAsync(prefix ?? "", ct), "", new { prefix });
        if (FlashError is null) Flash = $"Removed {removed} key(s) starting with {prefix?.Trim()}.";
        return result;
    }

    public async Task<IActionResult> OnPostClearAsync(CancellationToken ct)
    {
        var removed = 0;
        var result = await RunAsync(async () => removed = await _cache.ClearAsync(ct), "");
        if (FlashError is null) Flash = $"Cache cleared ({removed} key(s)) and settings reloaded.";
        return result;
    }
}
