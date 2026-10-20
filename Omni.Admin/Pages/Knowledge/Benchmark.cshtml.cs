using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Common.Enums;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Knowledge;

/// <summary>Search benchmark: how well one client's Knowledge Base search finds the right chunk. Only the page shell — the
/// client comes from the header's client scope (or ?clientId=), and everything on it is loaded and driven by
/// wwwroot/js/knowledge-benchmark.js through /api/admin/knowledge-benchmark/{clientId}, which calls the main app.</summary>
[ScopeCapability(ScopeCapability.Client)]
public class BenchmarkModel : AdminPageModel
{
    private readonly ScopeContext _scope;

    public BenchmarkModel(ScopeContext scope) => _scope = scope;

    [BindProperty(SupportsGet = true)] public Guid? ClientId { get; set; }

    /// <summary>The selected client's name, when the header knows it.</summary>
    public string? ClientName => _scope.EntityName;

    public void OnGet() => ClientId ??= _scope.FilterGuid;
}
