using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Clinics;
using SculptFlowAdmin.Business.Contracts.Services.Leads;
using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Dtos.Leads;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Appointments;

public class IndexModel : AdminPageModel
{
    private readonly ILeadAdminService _leads;
    private readonly IClinicAdminService _clinics;

    public IndexModel(ILeadAdminService leads, IClinicAdminService clinics)
    {
        _leads = leads;
        _clinics = clinics;
    }

    [BindProperty(SupportsGet = true)] public Guid? ClinicId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true)] public bool Upcoming { get; set; }
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public PagedResult<AppointmentRow> Result { get; private set; } = null!;
    public List<ClinicOption> Clinics { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        Clinics = await _clinics.OptionsAsync(ct);
        Result = await _leads.ListAppointmentsAsync(ClinicId, null, Status, Upcoming, PageNumber, ct);
    }

    /// <summary>Posted from the shared appointments table (also embedded on lead pages); goes back where it came from.</summary>
    public async Task<IActionResult> OnPostStatusAsync(Guid id, string status, string? returnUrl, CancellationToken ct)
    {
        await RunAsync(() => _leads.SetAppointmentStatusAsync(id, status, ct), $"Appointment marked {status.Replace('_', ' ')}.");
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/Appointments");
    }
}
