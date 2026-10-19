using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Common.Enums;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Knowledge;

/// <summary>One "Generate test cases" batch of a clinic's search benchmark: its summary, the chunks sent, rejected
/// questions, n8n's raw reply and its own test cases (edit, review, delete, run just this batch). Only the page shell —
/// wwwroot/js/knowledge-benchmark-generation.js loads it through /api/admin/knowledge-benchmark/{clinicId}; the main app
/// answers 404 for a generation that isn't that clinic's.</summary>
[ScopeCapability(ScopeCapability.Clinic)]
public class BenchmarkGenerationModel : AdminPageModel
{
    private readonly ScopeContext _scope;

    public BenchmarkGenerationModel(ScopeContext scope) => _scope = scope;

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    [BindProperty(SupportsGet = true)] public Guid? ClinicId { get; set; }

    public string? ClinicName => _scope.EntityName;

    public void OnGet() => ClinicId ??= _scope.FilterGuid;
}
