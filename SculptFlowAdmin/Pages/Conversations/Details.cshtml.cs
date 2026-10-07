using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Clinics;
using SculptFlowAdmin.Business.Contracts.Services.Leads;
using SculptFlowAdmin.Common.Enums;
using SculptFlowAdmin.Entities.Responses.PlatformAdmin;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Conversations;

public class DetailsModel : AdminPageModel
{
    private readonly ILeadAdminService _leads;
    private readonly IClinicAdminService _clinics;

    public DetailsModel(ILeadAdminService leads, IClinicAdminService clinics)
    {
        _leads = leads;
        _clinics = clinics;
    }

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    public ConversationDetail Conversation { get; private set; } = null!;
    public ClinicDetail? Clinic { get; private set; }
    public List<MessageDetail> Messages { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var c = await _leads.GetConversationAsync(Id, ct);
        if (c is null) return NotFound();
        Conversation = c;
        Clinic = await _clinics.GetAsync(c.ClinicId, ct);
        Messages = await _leads.MessagesAsync(Id, 500, ct);
        return Page();
    }

    public Task<IActionResult> OnPostModeAsync(string mode, CancellationToken ct) =>
        RunAsync(() => _leads.SetModeAsync(Id, mode, ct), mode == ConversationMode.Ai ? "Returned to AI." : "Handed to staff.", new { id = Id });

    public Task<IActionResult> OnPostStatusAsync(string status, CancellationToken ct) =>
        RunAsync(() => _leads.SetStatusAsync(Id, status, ct), $"Conversation marked {status}.", new { id = Id });
}
