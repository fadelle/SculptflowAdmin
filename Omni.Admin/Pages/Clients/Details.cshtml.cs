using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Billing;
using Omni.Admin.Business.Contracts.Services.Channels;
using Omni.Admin.Common.Exceptions;
using Omni.Admin.Common.Helpers;
using Omni.Admin.Entities.Responses.Billing;
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
    private readonly IBillingAdminService _billing;

    public DetailsModel(IClientAdminService clients, IMemberAdminService member, IChannelAdminService channels,
        IContentAdminService content, IOverviewAdminService overview, IBillingAdminService billing)
    {
        _billing = billing;
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
    /// <summary>The client's running or last Knowledge Base re-index (QA t4-ai-15), or null.</summary>
    public Omni.Admin.Entities.Dtos.Content.KnowledgeReindexProgress? Reindex { get; private set; }
    public InboxSettingsDto? InboxSettings { get; private set; }
    public IReadOnlyList<AdminAuditEntry> Audit { get; private set; } = [];
    /// <summary>Plan and money for the go-live checklist; null when the main app's billing didn't answer (the page still opens).</summary>
    public ClientBillingSummary? Billing { get; private set; }
    /// <summary>What is still missing for this client to be live (QA t6-admin-28).</summary>
    public IReadOnlyList<GoLiveStep> GoLiveSteps { get; private set; } = [];

    [BindProperty] public ClientUpdate Edit { get; set; } = new("", null, null, null, null, "UTC", null, null, null);

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var client = await _clients.GetAsync(Id, ct);
        if (client is null) return NotFound();
        Client = client;
        Edit = new ClientUpdate(client.Name, client.Phone, client.Email, client.Website, client.CountryCode, client.Timezone,
            client.Address, client.OperatingHours, client.BookingInfo);
        // The panels don't depend on each other, so they load together (QA t6-admin-18: eight calls one after another took 10-12 s).
        var counts = _clients.CountsAsync(Id, ct);
        var members = _members.ListAsync(Id, null, 1, ct);
        var channels = _channels.ListChannelsAsync(Id, null, false, ct);
        var calendars = _channels.ListCalendarsAsync(Id, ct);
        var tikTok = _channels.ListTikTokAsync(Id, ct);
        var search = _content.SearchSettingsAsync(Id, ct);
        var inbox = _clients.GetInboxSettingsAsync(Id, ct);
        var audit = _overview.AuditAsync(Id, null, 1, ct);
        var billing = TryBillingAsync(ct);
        var reindex = _content.TryReindexProgressAsync(Id, ct);
        await Task.WhenAll(counts, members, channels, calendars, tikTok, search, inbox, audit, billing, reindex);
        Reindex = reindex.Result;
        Counts = counts.Result;
        Members = members.Result.Items;
        Channels = channels.Result;
        Calendars = calendars.Result;
        TikTok = tikTok.Result;
        SearchSettings = search.Result;
        InboxSettings = inbox.Result;
        Audit = audit.Result.Items.Take(15).ToList();
        Billing = billing.Result;
        GoLiveSteps = GoLive.Steps(Id, client.IsActive, Members, Channels, InboxSettings, Billing);
        return Page();
    }

    private async Task<ClientBillingSummary?> TryBillingAsync(CancellationToken ct)
    {
        try
        {
            return (await _billing.ClientAsync(Id, ct))?.Summary;
        }
        catch (MainAppApiException)
        {
            return null; // the checklist says billing didn't answer
        }
    }

    public Task<IActionResult> OnPostUpdateAsync(CancellationToken ct) =>
        RunAsync(() => _clients.UpdateAsync(Id, Edit, ct), "Client details saved.", new { id = Id });

    public Task<IActionResult> OnPostSetActiveAsync(bool active, CancellationToken ct) =>
        RunAsync(() => _clients.SetActiveAsync(Id, active, ct), active ? "Client activated." : "Client deactivated.", new { id = Id });

    /// <summary>One connected account's AI. Unchecked switches aren't posted, so they bind as false; an empty bot is none.</summary>
    public Task<IActionResult> OnPostAccountAiAsync(Guid channelIntegrationId, bool aiEnabled, Guid? botId, bool newConversations,
        bool reopenedConversations, CancellationToken ct) =>
        RunAsync(() => _clients.UpdateAccountAiAsync(Id, channelIntegrationId,
                new UpdateAccountAiRequest(aiEnabled, botId, newConversations, reopenedConversations), ct),
            "Account AI settings saved.", new { id = Id });

    /// <summary>A new owner invite link (QA t6-admin-20); the main app refuses when the owner already joined or was never invited.</summary>
    public Task<IActionResult> OnPostResendOwnerInviteAsync(CancellationToken ct) =>
        RunAsync(() => _clients.ResendOwnerInviteAsync(Id, ct), "A new owner invite was sent.", new { id = Id });

    public Task<IActionResult> OnPostDefaultLanguageAsync(string language, CancellationToken ct) =>
        RunAsync(() => _clients.SetDefaultLanguageAsync(Id, language, ct), "Default language saved.", new { id = Id });

    public Task<IActionResult> OnPostReindexAsync(CancellationToken ct) =>
        RunAsync(() => _content.StartReindexAsync(Id, ct), "Re-index started: every knowledge entry is re-read with the current settings.", new { id = Id });

    public Task<IActionResult> OnPostSearchSettingsAsync(int? topK, double? minimumSimilarity, CancellationToken ct) =>
        // A value that isn't a number binds to null and is refused, never saved as 0 (QA t6-admin-05: "abc" removed the cut-off).
        RunAsync(() => topK is null || minimumSimilarity is null || !ModelState.IsValid
                ? throw new ArgumentException("Top K and minimum similarity must both be numbers.")
                : _content.UpdateSearchSettingsAsync(Id, topK.Value, minimumSimilarity.Value, ct),
            "Knowledge search settings saved.", new { id = Id });
}
