using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Channels;
using Omni.Admin.Business.Contracts.Services.Clients;
using Omni.Admin.Business.Contracts.Services.Content;
using Omni.Admin.Business.Contracts.Services.Overview;
using Omni.Admin.Business.Contracts.Services.Members;
using Omni.Admin.Entities.Dtos.Channels;
using Omni.Admin.Entities.Dtos.Clients;
using Omni.Admin.Entities.Dtos.Members;
using Omni.Admin.Entities.Models;
using Omni.Admin.Entities.Responses.PlatformAdmin;
using Omni.Admin.Entities.Requests.Clients;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Clients;

public class DetailsModel : AdminPageModel
{
    private readonly IClientAdminService _clients;
    private readonly IMemberAdminService _members;
    private readonly IChannelAdminService _channels;
    private readonly IContentAdminService _content;
    private readonly IOverviewAdminService _overview;

    public DetailsModel(IClientAdminService clients, IMemberAdminService member, IChannelAdminService channels,
        IContentAdminService content, IOverviewAdminService overview)
    {
        _clients = clients;
        _members = member;
        _channels = channels;
        _content = content;
        _overview = overview;
    }

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    public ClientDetail Client { get; private set; } = null!;
    public ClientCounts Counts { get; private set; } = null!;
    public IReadOnlyList<MemberRow> Members { get; private set; } = [];
    public List<ChannelRow> Channels { get; private set; } = new();
    public List<CalendarRow> Calendars { get; private set; } = new();
    public List<TikTokRow> TikTok { get; private set; } = new();
    public SearchSettingsDetail? SearchSettings { get; private set; }
    public InboxSettingsDto? InboxSettings { get; private set; }
    public IReadOnlyList<AdminAuditEntry> Audit { get; private set; } = [];

    [BindProperty] public ClientUpdate Edit { get; set; } = new("", null, null, null, null, "UTC", null, null, null);

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var client = await _clients.GetAsync(Id, ct);
        if (client is null) return NotFound();
        Client = client;
        Edit = new ClientUpdate(client.Name, client.Phone, client.Email, client.Website, client.CountryCode, client.Timezone,
            client.Address, client.OperatingHours, client.BookingInfo);
        Counts = await _clients.CountsAsync(Id, ct);
        Members = (await _members.ListAsync(Id, null, 1, ct)).Items;
        Channels = await _channels.ListChannelsAsync(Id, null, false, ct);
        Calendars = await _channels.ListCalendarsAsync(Id, ct);
        TikTok = await _channels.ListTikTokAsync(Id, ct);
        SearchSettings = await _content.SearchSettingsAsync(Id, ct);
        InboxSettings = await _clients.GetInboxSettingsAsync(Id, ct);
        Audit = (await _overview.AuditAsync(Id, null, 1, ct)).Items.Take(15).ToList();
        return Page();
    }

    public Task<IActionResult> OnPostUpdateAsync(CancellationToken ct) =>
        RunAsync(() => _clients.UpdateAsync(Id, Edit, ct), "Client details saved.", new { id = Id });

    public Task<IActionResult> OnPostSetActiveAsync(bool active, CancellationToken ct) =>
        RunAsync(() => _clients.SetActiveAsync(Id, active, ct), active ? "Client activated." : "Client deactivated.", new { id = Id });

    /// <summary>Unchecked switches aren't posted, so they bind as false.</summary>
    public Task<IActionResult> OnPostInboxSettingsAsync(bool newConversations, bool reopenedConversations, CancellationToken ct) =>
        RunAsync(() => _clients.UpdateInboxSettingsAsync(Id, new UpdateInboxSettingsRequest(newConversations, reopenedConversations), ct),
            "Inbox settings saved.", new { id = Id });

    public Task<IActionResult> OnPostSearchSettingsAsync(int topK, double minimumSimilarity, CancellationToken ct) =>
        RunAsync(() => _content.UpdateSearchSettingsAsync(Id, topK, minimumSimilarity, ct), "Knowledge search settings saved.", new { id = Id });
}
