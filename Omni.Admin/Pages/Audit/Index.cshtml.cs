using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Overview;
using Omni.Admin.Common.Enums;
using Omni.Admin.Entities.Models;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Audit;

[ScopeCapability(ScopeCapability.Clinic)]
public class IndexModel : AdminPageModel
{
    private readonly IOverviewAdminService _overview;
    private readonly ScopeContext _scope;

    public IndexModel(IOverviewAdminService overview, ScopeContext scope)
    {
        _overview = overview;
        _scope = scope;
    }

    [BindProperty(SupportsGet = true)] public Guid? ClinicId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public PagedResult<AdminAuditEntry> Result { get; private set; } = null!;

    public async Task OnGetAsync(CancellationToken ct)
    {
        ClinicId ??= _scope.FilterGuid; // the header's clinic scope when the URL names none
        Result = await _overview.AuditAsync(ClinicId, Q, PageNumber, ct);
    }
}
