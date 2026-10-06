using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Channels;
using SculptFlowAdmin.Business.Contracts.Services.Clinics;
using SculptFlowAdmin.Entities.Dtos.Channels;
using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Channels;

public class IndexModel : AdminPageModel
{
    private readonly IChannelAdminService _channels;
    private readonly IClinicAdminService _clinics;

    public IndexModel(IChannelAdminService channels, IClinicAdminService clinics)
    {
        _channels = channels;
        _clinics = clinics;
    }

    [BindProperty(SupportsGet = true)] public Guid? ClinicId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Channel { get; set; }
    [BindProperty(SupportsGet = true)] public bool Problems { get; set; }
    [BindProperty(SupportsGet = true)] public string Tab { get; set; } = "messaging";
    public List<ChannelRow> Rows { get; private set; } = new();
    public List<CalendarRow> Calendars { get; private set; } = new();
    public List<TikTokRow> TikTok { get; private set; } = new();
    public List<ClinicOption> Clinics { get; private set; } = new();
    public string ActiveProvider => _channels.ActiveWhatsAppProvider;

    public async Task OnGetAsync(CancellationToken ct)
    {
        Clinics = await _clinics.OptionsAsync(ct);
        Rows = await _channels.ListChannelsAsync(ClinicId, Channel, Problems, ct);
        Calendars = await _channels.ListCalendarsAsync(ClinicId, ct);
        TikTok = await _channels.ListTikTokAsync(ClinicId, ct);
    }

    public Task<IActionResult> OnPostDisconnectCalendarAsync(Guid id, CancellationToken ct) =>
        RunAsync(() => _channels.DisconnectCalendarAsync(id, ct), "Calendar disconnected.");

    public Task<IActionResult> OnPostCalendarSyncAsync(Guid id, bool enabled, CancellationToken ct) =>
        RunAsync(() => _channels.SetCalendarSyncAsync(id, enabled, ct), enabled ? "Calendar sync turned on." : "Calendar sync paused.");

    public Task<IActionResult> OnPostDisconnectTikTokAsync(Guid id, CancellationToken ct) =>
        RunAsync(() => _channels.DisconnectTikTokAsync(id, ct), "TikTok disconnected.");
}
