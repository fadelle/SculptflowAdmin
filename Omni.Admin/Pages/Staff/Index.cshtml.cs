using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Staff;
using Omni.Admin.Common.Enums;
using Omni.Admin.Entities.Dtos.Staff;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Staff;

[ScopeCapability(ScopeCapability.Clinic)]
public class IndexModel : AdminPageModel
{
    private readonly IStaffAdminService _staff;
    private readonly ScopeContext _scope;

    public IndexModel(IStaffAdminService staff, ScopeContext scope)
    {
        _staff = staff;
        _scope = scope;
    }

    [BindProperty(SupportsGet = true)] public Guid? ClinicId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public PagedResult<StaffRow> Result { get; private set; } = null!;

    public async Task OnGetAsync(CancellationToken ct)
    {
        ClinicId ??= _scope.FilterGuid; // the header's clinic scope when the URL names none
        Result = await _staff.ListAsync(ClinicId, Q, PageNumber, ct);
    }
}
