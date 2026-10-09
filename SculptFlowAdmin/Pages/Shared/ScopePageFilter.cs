using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using SculptFlowAdmin.Business.Contracts.Managers;
using SculptFlowAdmin.Common.Configs;
using SculptFlowAdmin.Common.Enums;
using SculptFlowAdmin.Common.Exceptions;

namespace SculptFlowAdmin.Pages.Shared;

/// <summary>
/// Resolves the global clinic scope for every page (docs/UI_GUIDE.md §7): ?clinicId= wins (shareable links; empty means
/// "all" and clears it), otherwise the cookie remembers the last choice. Signed-out requests are left alone.
/// </summary>
public sealed class ScopePageFilter : IAsyncPageFilter
{
    private readonly ScopeContext _scope;
    private readonly IOptions<ScopeOptions> _options;
    private readonly IScopeLookup _lookup;
    private readonly ILogger<ScopePageFilter> _log;

    public ScopePageFilter(ScopeContext scope, IOptions<ScopeOptions> options, IScopeLookup lookup, ILogger<ScopePageFilter> log)
    {
        _scope = scope;
        _options = options;
        _lookup = lookup;
        _log = log;
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var http = context.HttpContext;
        if (http.User.Identity?.IsAuthenticated != true)
        {
            await next();
            return;
        }

        var o = _options.Value;
        _scope.Capability = context.HandlerInstance.GetType().GetCustomAttribute<ScopeCapabilityAttribute>()?.Capability
            ?? ScopeCapability.None;

        string? id;
        if (http.Request.Query.TryGetValue(o.QueryKey, out var fromUrl))
        {
            id = string.IsNullOrWhiteSpace(fromUrl) ? null : fromUrl.ToString(); // "?clinicId=" = explicit All
            if (id is null) http.Response.Cookies.Delete(o.CookieName);
            else http.Response.Cookies.Append(o.CookieName, id, new CookieOptions
            {
                HttpOnly = true, IsEssential = true, SameSite = SameSiteMode.Lax, Secure = http.Request.IsHttps,
            });
        }
        else
        {
            id = http.Request.Cookies[o.CookieName];
        }

        if (id is not null)
        {
            try
            {
                var name = await _lookup.GetNameAsync(id, http.RequestAborted);
                if (name is null)
                {
                    http.Response.Cookies.Delete(o.CookieName); // stale or edited id
                    id = null;
                }
                _scope.EntityName = name;
            }
            catch (MainAppApiException ex)
            {
                // The main app didn't answer: keep the selection (the page itself reports the outage).
                _log.LogWarning(ex, "Couldn't resolve the scope {ScopeId}; keeping it without a name.", id);
            }
            _scope.EntityId = id;
        }

        await next();
    }
}
