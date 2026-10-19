using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Channels;
using Omni.Admin.Entities.Responses.PlatformAdmin;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Channels;

public class DetailsModel : AdminPageModel
{
    private readonly IChannelAdminService _channels;

    public DetailsModel(IChannelAdminService channels) => _channels = channels;

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    public ChannelDetail Row { get; private set; } = null!;
    public List<HealthEventRow> Events { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var row = await _channels.GetChannelAsync(Id, ct);
        if (row is null) return NotFound();
        Row = row;
        Events = await _channels.HealthEventsAsync(Id, ct);
        return Page();
    }

    public Task<IActionResult> OnPostDisconnectAsync(CancellationToken ct) =>
        RunAsync(() => _channels.DisconnectAsync(Id, ct), "Channel disconnected.", new { id = Id });
}
