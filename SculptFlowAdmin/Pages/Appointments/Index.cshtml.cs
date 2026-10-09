using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Leads;
using SculptFlowAdmin.Common.Enums;
using SculptFlowAdmin.Entities.Dtos.Leads;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Appointments;

[ScopeCapability(ScopeCapability.Clinic)]
public class IndexModel : AdminPageModel
{
    private readonly ILeadAdminService _leads;
    private readonly ScopeContext _scope;

    public IndexModel(ILeadAdminService leads, ScopeContext scope)
    {
        _leads = leads;
        _scope = scope;
    }

    [BindProperty(SupportsGet = true)] public Guid? ClinicId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true)] public bool Upcoming { get; set; }
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public PagedResult<AppointmentRow> Result { get; private set; } = null!;

    public async Task OnGetAsync(CancellationToken ct)
    {
        ClinicId ??= _scope.FilterGuid; // the header's clinic scope when the URL names none
        Result = await _leads.ListAppointmentsAsync(ClinicId, null, Status, Upcoming, PageNumber, ct);
    }

    /// <summary>Posted from the shared appointments table (also embedded on lead pages); goes back where it came from.</summary>
    public async Task<IActionResult> OnPostStatusAsync(Guid id, string status, string? returnUrl, CancellationToken ct)
    {
        await RunAsync(() => _leads.SetAppointmentStatusAsync(id, status, ct), $"Appointment marked {status.Replace('_', ' ')}.");
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/Appointments");
    }
}
