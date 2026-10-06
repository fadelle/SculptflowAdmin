using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Channels;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Channels;

public class DetailsModel : AdminPageModel
{
    private readonly IChannelAdminService _channels;

    public DetailsModel(IChannelAdminService channels) => _channels = channels;

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    public ChannelIntegration Row { get; private set; } = null!;
    public List<WhatsAppHealthEvent> Events { get; private set; } = new();
    public string? InfobipWebhookUrl { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var row = await _channels.GetChannelAsync(Id, ct);
        if (row is null) return NotFound();
        Row = row;
        Events = await _channels.HealthEventsAsync(Id, ct);
        InfobipWebhookUrl = _channels.InfobipWebhookUrl(row);
        return Page();
    }

    public Task<IActionResult> OnPostDisconnectAsync(CancellationToken ct) =>
        RunAsync(() => _channels.DisconnectAsync(Id, ct), "Channel disconnected.", new { id = Id });
}
