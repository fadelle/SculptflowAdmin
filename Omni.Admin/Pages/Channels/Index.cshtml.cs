using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Channels;
using Omni.Admin.Common.Enums;
using Omni.Admin.Entities.Dtos.Channels;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Channels;

[ScopeCapability(ScopeCapability.Client)]
public class IndexModel : AdminPageModel
{
    private readonly IChannelAdminService _channels;
    private readonly ScopeContext _scope;

    public IndexModel(IChannelAdminService channels, ScopeContext scope)
    {
        _channels = channels;
        _scope = scope;
    }

    [BindProperty(SupportsGet = true)] public Guid? ClientId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Channel { get; set; }
    [BindProperty(SupportsGet = true)] public bool Problems { get; set; }
    [BindProperty(SupportsGet = true)] public string Tab { get; set; } = "messaging";
    public List<ChannelRow> Rows { get; private set; } = new();
    public List<CalendarRow> Calendars { get; private set; } = new();
    public List<TikTokRow> TikTok { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken ct)
    {
        ClientId ??= _scope.FilterGuid; // the header's client scope when the URL names none
        Rows = await _channels.ListChannelsAsync(ClientId, Channel, Problems, ct);
        Calendars = await _channels.ListCalendarsAsync(ClientId, ct);
        TikTok = await _channels.ListTikTokAsync(ClientId, ct);
    }

    public Task<IActionResult> OnPostDisconnectCalendarAsync(Guid id, CancellationToken ct) =>
        RunAsync(() => _channels.DisconnectCalendarAsync(id, ct), "Calendar disconnected.");

    public Task<IActionResult> OnPostCalendarSyncAsync(Guid id, bool enabled, CancellationToken ct) =>
        RunAsync(() => _channels.SetCalendarSyncAsync(id, enabled, ct), enabled ? "Calendar sync turned on." : "Calendar sync paused.");

    public Task<IActionResult> OnPostDisconnectTikTokAsync(Guid id, CancellationToken ct) =>
        RunAsync(() => _channels.DisconnectTikTokAsync(id, ct), "TikTok disconnected.");
}
