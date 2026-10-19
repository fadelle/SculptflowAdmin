using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Leads;
using Omni.Admin.Entities.Dtos.Leads;
using Omni.Admin.Entities.Responses.PlatformAdmin;
using Omni.Admin.Entities.Requests.Leads;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Leads;

public class DetailsModel : AdminPageModel
{
    private readonly ILeadAdminService _leads;

    public DetailsModel(ILeadAdminService leads) => _leads = leads;

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    public LeadDetail Lead { get; private set; } = null!;
    public IReadOnlyList<ConversationRow> Conversations { get; private set; } = [];
    public IReadOnlyList<AppointmentRow> Appointments { get; private set; } = [];
    public List<LeadEventRow> Events { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var lead = await _leads.GetLeadAsync(Id, ct);
        if (lead is null) return NotFound();
        Lead = lead;
        Conversations = (await _leads.ListConversationsAsync(null, Id, null, null, null, 1, ct)).Items;
        Appointments = (await _leads.ListAppointmentsAsync(null, Id, null, false, 1, ct)).Items;
        Events = await _leads.LeadEventsAsync(Id, ct);
        return Page();
    }

    public Task<IActionResult> OnPostUpdateAsync(string status, string qualificationStatus, bool marketingOptIn, string? notes,
        CancellationToken ct) =>
        RunAsync(() => _leads.UpdateLeadAsync(Id, new LeadUpdate(status, qualificationStatus, marketingOptIn, notes), ct),
            "Lead saved.", new { id = Id });
}
