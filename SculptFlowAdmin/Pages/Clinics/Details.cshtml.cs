using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Channels;
using SculptFlowAdmin.Business.Contracts.Services.Clinics;
using SculptFlowAdmin.Business.Contracts.Services.Content;
using SculptFlowAdmin.Business.Contracts.Services.Overview;
using SculptFlowAdmin.Business.Contracts.Services.Staff;
using SculptFlowAdmin.Entities.Dtos.Channels;
using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Dtos.Staff;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Entities.Requests.Clinics;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Clinics;

public class DetailsModel : AdminPageModel
{
    private readonly IClinicAdminService _clinics;
    private readonly IStaffAdminService _staff;
    private readonly IChannelAdminService _channels;
    private readonly IContentAdminService _content;
    private readonly IOverviewAdminService _overview;

    public DetailsModel(IClinicAdminService clinics, IStaffAdminService staff, IChannelAdminService channels,
        IContentAdminService content, IOverviewAdminService overview)
    {
        _clinics = clinics;
        _staff = staff;
        _channels = channels;
        _content = content;
        _overview = overview;
    }

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    public Clinic Clinic { get; private set; } = null!;
    public ClinicCounts Counts { get; private set; } = null!;
    public IReadOnlyList<StaffRow> Staff { get; private set; } = [];
    public List<ChannelRow> Channels { get; private set; } = new();
    public List<CalendarRow> Calendars { get; private set; } = new();
    public List<TikTokRow> TikTok { get; private set; } = new();
    public KnowledgeSearchSettings? SearchSettings { get; private set; }
    public IReadOnlyList<AdminAuditEntry> Audit { get; private set; } = [];

    [BindProperty] public ClinicUpdate Edit { get; set; } = new("", null, null, null, null, "UTC", null, null, null);

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var clinic = await _clinics.GetAsync(Id, ct);
        if (clinic is null) return NotFound();
        Clinic = clinic;
        Edit = new ClinicUpdate(clinic.Name, clinic.Phone, clinic.Email, clinic.Website, clinic.CountryCode, clinic.Timezone,
            clinic.Address, clinic.OperatingHours, clinic.ConsultationInfo);
        Counts = await _clinics.CountsAsync(Id, ct);
        Staff = (await _staff.ListAsync(Id, null, 1, ct)).Items;
        Channels = await _channels.ListChannelsAsync(Id, null, false, ct);
        Calendars = await _channels.ListCalendarsAsync(Id, ct);
        TikTok = await _channels.ListTikTokAsync(Id, ct);
        SearchSettings = await _content.SearchSettingsAsync(Id, ct);
        Audit = (await _overview.AuditAsync(Id, null, 1, ct)).Items.Take(15).ToList();
        return Page();
    }

    public Task<IActionResult> OnPostUpdateAsync(CancellationToken ct) =>
        RunAsync(() => _clinics.UpdateAsync(Id, Edit, ct), "Clinic details saved.", new { id = Id });

    public Task<IActionResult> OnPostSetActiveAsync(bool active, CancellationToken ct) =>
        RunAsync(() => _clinics.SetActiveAsync(Id, active, ct), active ? "Clinic activated." : "Clinic deactivated.", new { id = Id });

    public Task<IActionResult> OnPostConnectWhatsAppAsync(string? sender, string? verifiedName, CancellationToken ct) =>
        RunAsync(() => _channels.ConnectInfobipSenderAsync(Id, sender, verifiedName, ct),
            "WhatsApp sender connected. Paste its webhook URL into the Infobip sender (see the channel's page).", new { id = Id });

    public Task<IActionResult> OnPostSearchSettingsAsync(int topK, double minimumSimilarity, CancellationToken ct) =>
        RunAsync(() => _content.UpdateSearchSettingsAsync(Id, topK, minimumSimilarity, ct), "Knowledge search settings saved.", new { id = Id });
}
