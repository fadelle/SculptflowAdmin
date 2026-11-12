using Omni.Admin.Entities.Dtos.Overview;
using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Overview;
using Omni.Admin.Common.Enums;
using Omni.Admin.Entities.Models;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Audit;

[ScopeCapability(ScopeCapability.Client)]
public class IndexModel : AdminPageModel
{
    private readonly IOverviewAdminService _overview;
    private readonly ScopeContext _scope;

    public IndexModel(IOverviewAdminService overview, ScopeContext scope)
    {
        _overview = overview;
        _scope = scope;
    }

    [BindProperty(SupportsGet = true)] public Guid? ClientId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    /// <summary>"api" = the main app's own record of every platform-admin API write (QA t6-admin-23), else this portal's log.</summary>
    [BindProperty(SupportsGet = true)] public string? View { get; set; }
    public PagedResult<AdminAuditEntry> Result { get; private set; } = null!;
    public PagedResult<PlatformAdminWriteRow>? Writes { get; private set; }

    public async Task OnGetAsync(CancellationToken ct)
    {
        ClientId ??= _scope.FilterGuid; // the header's client scope when the URL names none
        if (View == "api")
        {
            Writes = await _overview.AdminWritesAsync(ClientId, PageNumber, ct);
            Result = new PagedResult<AdminAuditEntry>([], 1, 1, 0);
            return;
        }
        Result = await _overview.AuditAsync(ClientId, Q, PageNumber, ct);
    }
}
