using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Common.Enums;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Knowledge;

/// <summary>Search benchmark: how well one clinic's Knowledge Base search finds the right chunk. Only the page shell — the
/// clinic comes from the header's clinic scope (or ?clinicId=), and everything on it is loaded and driven by
/// wwwroot/js/knowledge-benchmark.js through /api/admin/knowledge-benchmark/{clinicId}, which calls the main app.</summary>
[ScopeCapability(ScopeCapability.Clinic)]
public class BenchmarkModel : AdminPageModel
{
    private readonly ScopeContext _scope;

    public BenchmarkModel(ScopeContext scope) => _scope = scope;

    [BindProperty(SupportsGet = true)] public Guid? ClinicId { get; set; }

    /// <summary>The selected clinic's name, when the header knows it.</summary>
    public string? ClinicName => _scope.EntityName;

    public void OnGet() => ClinicId ??= _scope.FilterGuid;
}
