using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Channels;
using Omni.Admin.Business.Contracts.Services.Clients;
using Omni.Admin.Business.Contracts.Services.Content;
using Omni.Admin.Business.Contracts.Services.Contacts;
using Omni.Admin.Business.Contracts.Services.Overview;
using Omni.Admin.Business.Contracts.Services.Members;
using Omni.Admin.Common.Enums;
using Omni.Admin.Common.Exceptions;
using Omni.Admin.Entities.Models;
using Omni.Admin.Entities.Requests.Clients;
using Omni.Admin.Entities.Requests.Content;
using Omni.Admin.Entities.Requests.Contacts;
using Omni.Admin.Entities.Requests.Members;

namespace Omni.Admin.Controllers.Admin;

/// <summary>
/// The admin JSON API (/api/admin/*). Same rules and audit log as the pages: every endpoint needs a signed-in admin
/// (the admin cookie; 401 otherwise), and every write goes through the same service and lands in admin_audit_log.
/// Client member credentials from the main app never work here. The cookie is SameSite=Strict, so other sites can't
/// make a signed-in admin's browser call these.
/// </summary>
[ApiController]
[Authorize]
[Route("api/admin")]
public class AdminApiController : ControllerBase
{
    private readonly IOverviewAdminService _overview;
    private readonly IClientAdminService _clients;
    private readonly IMemberAdminService _members;
    private readonly IContactAdminService _contacts;
    private readonly IChannelAdminService _channels;
    private readonly IContentAdminService _content;

    public AdminApiController(IOverviewAdminService overview, IClientAdminService clients, IMemberAdminService member,
        IContactAdminService contacts, IChannelAdminService channels, IContentAdminService content)
    {
        _overview = overview;
        _clients = clients;
        _members = member;
        _contacts = contacts;
        _channels = channels;
        _content = content;
    }

    // ------------------------------------------------------------------ overview

    [HttpGet("overview")]
    public async Task<IActionResult> Overview(CancellationToken ct) =>
        Ok(new { totals = await _overview.TotalsAsync(ct), daily = await _overview.DailyMessagesAsync(14, ct) });

    [HttpGet("problems")]
    public async Task<IActionResult> Problems(CancellationToken ct) => Ok(await _overview.ProblemsAsync(ct));

    [HttpGet("events")]
    public async Task<IActionResult> Events(Guid? clientId, string? type, int page = 1, CancellationToken ct = default) =>
        Ok(await _overview.EventsAsync(clientId, type, page, ct));

    [HttpGet("audit")]
    public async Task<IActionResult> Audit(Guid? clientId, string? q, int page = 1, CancellationToken ct = default) =>
        Ok(await _overview.AuditAsync(clientId, q, page, ct));

    // ------------------------------------------------------------------ clients

    [HttpGet("clients")]
    public async Task<IActionResult> Clients(string? q, bool? active, int page = 1, CancellationToken ct = default) =>
        Ok(await _clients.ListAsync(q, active, page, ct));

    [HttpGet("clients/{id:guid}")]
    public async Task<IActionResult> Client(Guid id, CancellationToken ct)
    {
        var client = await _clients.GetAsync(id, ct);
        if (client is null) return NotFound();
        return Ok(new
        {
            client = new
            {
                client.Id, client.Name, client.Slug, client.Phone, client.Email, client.Website, client.CountryCode,
                client.Timezone, client.Address, client.OperatingHours, client.BookingInfo, client.IsActive,
                client.CreatedAt, client.UpdatedAt
            },
            counts = await _clients.CountsAsync(id, ct),
            channels = await _channels.ListChannelsAsync(id, null, false, ct),
            calendars = await _channels.ListCalendarsAsync(id, ct),
            tiktok = await _channels.ListTikTokAsync(id, ct)
        });
    }

    [HttpPut("clients/{id:guid}")]
    public Task<IActionResult> UpdateClient(Guid id, ClientUpdate body, CancellationToken ct) =>
        Write(() => _clients.UpdateAsync(id, body, ct));

    [HttpPost("clients/{id:guid}/activate")]
    public Task<IActionResult> ActivateClient(Guid id, CancellationToken ct) => Write(() => _clients.SetActiveAsync(id, true, ct));

    [HttpPost("clients/{id:guid}/deactivate")]
    public Task<IActionResult> DeactivateClient(Guid id, CancellationToken ct) => Write(() => _clients.SetActiveAsync(id, false, ct));

    [HttpPut("clients/{id:guid}/knowledge-settings")]
    public Task<IActionResult> UpdateSearchSettings(Guid id, SearchSettingsBody body, CancellationToken ct) =>
        Write(() => _content.UpdateSearchSettingsAsync(id, body.TopK, body.MinimumSimilarity, ct));

    // ------------------------------------------------------------------ members

    [HttpGet("members")]
    public async Task<IActionResult> Member(Guid? clientId, string? q, int page = 1, CancellationToken ct = default) =>
        Ok(await _members.ListAsync(clientId, q, page, ct));

    [HttpGet("members/{userId}")]
    public async Task<IActionResult> Member(string userId, CancellationToken ct) =>
        await _members.GetAsync(userId, ct) is { } s ? Ok(s) : NotFound();

    [HttpPost("members/{userId}/lock")]
    public Task<IActionResult> Lock(string userId, CancellationToken ct) => Write(() => _members.SetLoginLockedAsync(userId, true, ct));

    [HttpPost("members/{userId}/unlock")]
    public Task<IActionResult> Unlock(string userId, CancellationToken ct) => Write(() => _members.SetLoginLockedAsync(userId, false, ct));

    [HttpPost("members/{userId}/membership/activate")]
    public Task<IActionResult> ActivateMembership(string userId, CancellationToken ct) =>
        Write(() => _members.SetMembershipActiveAsync(userId, true, ct));

    [HttpPost("members/{userId}/membership/deactivate")]
    public Task<IActionResult> DeactivateMembership(string userId, CancellationToken ct) =>
        Write(() => _members.SetMembershipActiveAsync(userId, false, ct));

    [HttpPost("members/{userId}/password")]
    public Task<IActionResult> MemberPassword(string userId, PasswordBody body, CancellationToken ct) =>
        Write(() => _members.SetPasswordAsync(userId, body.Password, ct));

    // ------------------------------------------------------------------ contacts, conversations, messages, bookings

    [HttpGet("contacts")]
    public async Task<IActionResult> Contacts(Guid? clientId, string? status, string? q, int page = 1, CancellationToken ct = default) =>
        Ok(await _contacts.ListContactsAsync(clientId, status, q, page, ct));

    [HttpGet("contacts/{id:guid}")]
    public async Task<IActionResult> Contact(Guid id, CancellationToken ct)
    {
        var contact = await _contacts.GetContactAsync(id, ct);
        if (contact is null) return NotFound();
        return Ok(new
        {
            contact.Id, contact.ClientId, clientName = contact.Client?.Name, contact.FullName, contact.FirstName, contact.LastName, contact.Phone,
            contact.Email, contact.Source, contact.SourceDetail, contact.CampaignName, contact.StageKey, contact.Qualification,
            service = contact.Service?.Name, contact.PreferredLanguage, contact.CountryCode, contact.City, contact.DesiredTimeline,
            contact.Notes, contact.MarketingOptIn, contact.OptedOutAt, contact.LastContactAt, contact.NextFollowupAt, contact.CreatedAt, contact.UpdatedAt
        });
    }

    [HttpPatch("contacts/{id:guid}")]
    public Task<IActionResult> UpdateContact(Guid id, ContactUpdate body, CancellationToken ct) => Write(() => _contacts.UpdateContactAsync(id, body, ct));

    [HttpGet("conversations")]
    public async Task<IActionResult> Conversations(Guid? clientId, Guid? contactId, string? channel, string? mode, string? status,
        int page = 1, CancellationToken ct = default) =>
        Ok(await _contacts.ListConversationsAsync(clientId, contactId, channel, mode, status, page, ct));

    [HttpGet("conversations/{id:guid}")]
    public async Task<IActionResult> Conversation(Guid id, CancellationToken ct)
    {
        var c = await _contacts.GetConversationAsync(id, ct);
        if (c is null) return NotFound();
        var messages = await _contacts.MessagesAsync(id, 500, ct);
        return Ok(new
        {
            c.Id, c.ClientId, c.ContactId, c.Channel, c.Status, c.Mode, c.LastMessageAt, c.ServiceWindowExpiresAt, c.CreatedAt,
            messages = messages.Select(m => new
            {
                m.Id, m.Direction, m.SenderType, m.Origin, m.MessageType, m.Content, m.DeliveryStatus, m.FailureCode,
                m.FailureReason, m.CampaignId, m.CreatedAt
            })
        });
    }

    [HttpPost("conversations/{id:guid}/mode")]
    public Task<IActionResult> ConversationMode(Guid id, ModeBody body, CancellationToken ct) => Write(() => _contacts.SetModeAsync(id, body.Mode, ct));

    [HttpPost("conversations/{id:guid}/status")]
    public Task<IActionResult> ConversationStatus(Guid id, StatusBody body, CancellationToken ct) =>
        Write(() => _contacts.SetStatusAsync(id, body.Status, ct));

    [HttpGet("messages")]
    public async Task<IActionResult> Messages(Guid? clientId, bool failed = false, string? sender = null, string? q = null, int page = 1,
        CancellationToken ct = default) =>
        Ok(await _contacts.ListMessagesAsync(clientId, failed, sender, q, page, ct));

    [HttpGet("bookings")]
    public async Task<IActionResult> Bookings(Guid? clientId, Guid? contactId, string? status, bool upcoming = false, int page = 1,
        CancellationToken ct = default) =>
        Ok(await _contacts.ListBookingsAsync(clientId, contactId, status, upcoming, page, ct));

    [HttpPost("bookings/{id:guid}/status")]
    public Task<IActionResult> BookingStatus(Guid id, StatusBody body, CancellationToken ct) =>
        Write(() => _contacts.SetBookingStatusAsync(id, body.Status, ct));

    // ------------------------------------------------------------------ channels

    [HttpGet("channels")]
    public async Task<IActionResult> Channels(Guid? clientId, string? channel, bool problems = false, CancellationToken ct = default) =>
        Ok(await _channels.ListChannelsAsync(clientId, channel, problems, ct));

    [HttpPost("channels/{id:guid}/disconnect")]
    public Task<IActionResult> DisconnectChannel(Guid id, CancellationToken ct) => Write(() => _channels.DisconnectAsync(id, ct));

    [HttpGet("calendars")]
    public async Task<IActionResult> Calendars(Guid? clientId, CancellationToken ct) => Ok(await _channels.ListCalendarsAsync(clientId, ct));

    [HttpPost("calendars/{id:guid}/disconnect")]
    public Task<IActionResult> DisconnectCalendar(Guid id, CancellationToken ct) => Write(() => _channels.DisconnectCalendarAsync(id, ct));

    [HttpGet("tiktok")]
    public async Task<IActionResult> TikTok(Guid? clientId, CancellationToken ct) => Ok(await _channels.ListTikTokAsync(clientId, ct));

    [HttpPost("tiktok/{id:guid}/disconnect")]
    public Task<IActionResult> DisconnectTikTok(Guid id, CancellationToken ct) => Write(() => _channels.DisconnectTikTokAsync(id, ct));

    // ------------------------------------------------------------------ content

    [HttpGet("campaigns")]
    public async Task<IActionResult> Campaigns(Guid? clientId, string? status, int page = 1, CancellationToken ct = default) =>
        Ok(await _content.ListCampaignsAsync(clientId, status, page, ct));

    [HttpPost("campaigns/{id:guid}/cancel")]
    public Task<IActionResult> CancelCampaign(Guid id, CancellationToken ct) => Write(() => _content.CancelCampaignAsync(id, ct));

    [HttpGet("templates")]
    public async Task<IActionResult> Templates(Guid? clientId, string? status, CancellationToken ct) =>
        Ok(await _content.ListTemplatesAsync(clientId, status, ct));

    [HttpGet("services")]
    public async Task<IActionResult> Services(Guid? clientId, CancellationToken ct) => Ok(await _content.ListServicesAsync(clientId, ct));

    [HttpPost("services/{id:guid}/active")]
    public Task<IActionResult> ServiceActive(Guid id, ActiveBody body, CancellationToken ct) =>
        Write(() => _content.SetServiceActiveAsync(id, body.Active, ct));

    [HttpGet("knowledge")]
    public async Task<IActionResult> Knowledge(Guid? clientId, string? q, bool? active, int page = 1, CancellationToken ct = default) =>
        Ok(await _content.ListDocumentsAsync(clientId, q, active, page, ct));

    [HttpPost("knowledge/{id:guid}/active")]
    public Task<IActionResult> KnowledgeActive(Guid id, ActiveBody body, CancellationToken ct) =>
        Write(() => _content.SetDocumentActiveAsync(id, body.Active, ct));

    [HttpGet("knowledge/websites")]
    public async Task<IActionResult> Websites(Guid? clientId, CancellationToken ct) => Ok(await _content.ListWebsitesAsync(clientId, ct));

    // ------------------------------------------------------------------

    /// <summary>204 on success, 400 with {error} for a rule the admin can fix, 404 for a missing record.</summary>
    private async Task<IActionResult> Write(Func<Task> action)
    {
        try
        {
            await action();
            return NoContent();
        }
        catch (Exception ex) when (ex is AdminRuleException or ArgumentException) { return BadRequest(new { error = ex.Message }); }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
