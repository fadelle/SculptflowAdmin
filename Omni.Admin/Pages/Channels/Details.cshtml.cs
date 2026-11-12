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

    /// <summary>"Check connection" (QA t8-20): the stored token asked about the number; the answer is the flash message.</summary>
    public async Task<IActionResult> OnPostCheckAsync(CancellationToken ct)
    {
        Omni.Admin.Entities.Dtos.Channels.ChannelCheckResult? result = null;
        return await RunAsync(async () => result = await _channels.CheckAsync(Id, ct),
            () => result is { Ok: true }
                ? $"Connection works: Meta knows {result.DisplayPhoneNumber}{(string.IsNullOrEmpty(result.VerifiedName) ? "" : " (" + result.VerifiedName + ")")}."
                : $"Meta refused the stored token: {result?.Error}. The channel now shows it as a warning; the client reconnects WhatsApp to fix it.",
            new { id = Id });
    }

    public Task<IActionResult> OnPostDisconnectAsync(CancellationToken ct) =>
        RunAsync(() => _channels.DisconnectAsync(Id, ct), "Channel disconnected.", new { id = Id });
}
