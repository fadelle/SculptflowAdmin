using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using SculptFlowAdmin.Common.Configs;

namespace SculptFlowAdmin.Common.Helpers;

/// <summary>URL helpers for the shell and the shared partials (docs/UI_GUIDE.md §11).</summary>
public static class UiExtensions
{
    /// <summary>Current URL with some query keys replaced (null ⇒ removed); every other filter is kept.</summary>
    public static string WithQuery(this HttpRequest request, params (string Key, string? Value)[] changes)
    {
        var query = QueryHelpers.ParseQuery(request.QueryString.Value)
            .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
        foreach (var (key, value) in changes)
        {
            if (value is null) query.Remove(key);
            else query[key] = value;
        }
        var pairs = query.SelectMany(kv => kv.Value.Select(v => new KeyValuePair<string, string>(kv.Key, v ?? "")));
        return request.PathBase + request.Path + new QueryBuilder(pairs).ToQueryString();
    }

    /// <summary>True when the URL narrows the list (any query value other than paging, handler, tab, id or the global
    /// clinic scope): picks "No leads match your filters" over "No leads yet", and shows the toolbar's Clear.</summary>
    public static bool HasFilters(this HttpRequest request)
    {
        var scopeKey = request.HttpContext.RequestServices.GetService<IOptions<ScopeOptions>>()?.Value.QueryKey ?? "clinicId";
        return request.Query.Any(kv => kv.Key is not ("p" or "handler" or "tab" or "id")
            && !kv.Key.Equals(scopeKey, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(kv.Value));
    }

    /// <summary>The empty-list message for a list of <paramref name="things"/> ("leads"), filtered or not.</summary>
    public static string EmptyMessage(this HttpRequest request, string things) =>
        request.HasFilters() ? $"No {things} match your filters." : $"No {things} yet.";
}
