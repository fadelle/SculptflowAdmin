using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Channels;
using SculptFlowAdmin.Business.Contracts.Services.Clinics;
using SculptFlowAdmin.Business.Contracts.Services.Content;
using SculptFlowAdmin.Business.Contracts.Services.Leads;
using SculptFlowAdmin.Business.Contracts.Services.Overview;
using SculptFlowAdmin.Business.Contracts.Services.Staff;
using SculptFlowAdmin.Common.Enums;
using SculptFlowAdmin.Common.Exceptions;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Entities.Requests.Channels;
using SculptFlowAdmin.Entities.Requests.Clinics;
using SculptFlowAdmin.Entities.Requests.Content;
using SculptFlowAdmin.Entities.Requests.Leads;
using SculptFlowAdmin.Entities.Requests.Staff;

namespace SculptFlowAdmin.Controllers.Admin;

/// <summary>
/// The admin JSON API (/api/admin/*). Same rules and audit log as the pages: every endpoint needs a signed-in admin
/// (the admin cookie; 401 otherwise), and every write goes through the same service and lands in admin_audit_log.
/// Clinic staff credentials from the main app never work here. The cookie is SameSite=Strict, so other sites can't
/// make a signed-in admin's browser call these.
/// </summary>
[ApiController]
[Authorize]
[Route("api/admin")]
public class AdminApiController : ControllerBase
{
    private readonly IOverviewAdminService _overview;
    private readonly IClinicAdminService _clinics;
    private readonly IStaffAdminService _staff;
    private readonly ILeadAdminService _leads;
    private readonly IChannelAdminService _channels;
    private readonly IContentAdminService _content;

    public AdminApiController(IOverviewAdminService overview, IClinicAdminService clinics, IStaffAdminService staff,
        ILeadAdminService leads, IChannelAdminService channels, IContentAdminService content)
    {
        _overview = overview;
        _clinics = clinics;
        _staff = staff;
        _leads = leads;
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
    public async Task<IActionResult> Events(Guid? clinicId, string? type, int page = 1, CancellationToken ct = default) =>
        Ok(await _overview.EventsAsync(clinicId, type, page, ct));

    [HttpGet("audit")]
    public async Task<IActionResult> Audit(Guid? clinicId, string? q, int page = 1, CancellationToken ct = default) =>
        Ok(await _overview.AuditAsync(clinicId, q, page, ct));

    // ------------------------------------------------------------------ clinics

    [HttpGet("clinics")]
    public async Task<IActionResult> Clinics(string? q, bool? active, int page = 1, CancellationToken ct = default) =>
        Ok(await _clinics.ListAsync(q, active, page, ct));

    [HttpGet("clinics/{id:guid}")]
    public async Task<IActionResult> Clinic(Guid id, CancellationToken ct)
    {
        var clinic = await _clinics.GetAsync(id, ct);
        if (clinic is null) return NotFound();
        return Ok(new
        {
            clinic = new
            {
                clinic.Id, clinic.Name, clinic.Slug, clinic.Phone, clinic.Email, clinic.Website, clinic.CountryCode,
                clinic.Timezone, clinic.Address, clinic.OperatingHours, clinic.ConsultationInfo, clinic.IsActive,
                clinic.CreatedAt, clinic.UpdatedAt
            },
            counts = await _clinics.CountsAsync(id, ct),
            channels = await _channels.ListChannelsAsync(id, null, false, ct),
            calendars = await _channels.ListCalendarsAsync(id, ct),
            tiktok = await _channels.ListTikTokAsync(id, ct)
        });
    }

    [HttpPut("clinics/{id:guid}")]
    public Task<IActionResult> UpdateClinic(Guid id, ClinicUpdate body, CancellationToken ct) =>
        Write(() => _clinics.UpdateAsync(id, body, ct));

    [HttpPost("clinics/{id:guid}/activate")]
    public Task<IActionResult> ActivateClinic(Guid id, CancellationToken ct) => Write(() => _clinics.SetActiveAsync(id, true, ct));

    [HttpPost("clinics/{id:guid}/deactivate")]
    public Task<IActionResult> DeactivateClinic(Guid id, CancellationToken ct) => Write(() => _clinics.SetActiveAsync(id, false, ct));

    [HttpPost("clinics/{id:guid}/whatsapp/infobip")]
    public async Task<IActionResult> ConnectInfobip(Guid id, InfobipSenderBody body, CancellationToken ct)
    {
        try
        {
            var row = await _channels.ConnectInfobipSenderAsync(id, body.Sender, ct);
            return Ok(new { row.Id, row.Status, row.DisplayName, webhookUrl = row.InfobipWebhookUrl });
        }
        catch (Exception ex) when (ex is AdminRuleException or ArgumentException) { return BadRequest(new { error = ex.Message }); }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPut("clinics/{id:guid}/knowledge-settings")]
    public Task<IActionResult> UpdateSearchSettings(Guid id, SearchSettingsBody body, CancellationToken ct) =>
        Write(() => _content.UpdateSearchSettingsAsync(id, body.TopK, body.MinimumSimilarity, ct));

    // ------------------------------------------------------------------ staff

    [HttpGet("staff")]
    public async Task<IActionResult> Staff(Guid? clinicId, string? q, int page = 1, CancellationToken ct = default) =>
        Ok(await _staff.ListAsync(clinicId, q, page, ct));

    [HttpGet("staff/{userId}")]
    public async Task<IActionResult> StaffMember(string userId, CancellationToken ct) =>
        await _staff.GetAsync(userId, ct) is { } s ? Ok(s) : NotFound();

    [HttpPost("staff/{userId}/lock")]
    public Task<IActionResult> Lock(string userId, CancellationToken ct) => Write(() => _staff.SetLoginLockedAsync(userId, true, ct));

    [HttpPost("staff/{userId}/unlock")]
    public Task<IActionResult> Unlock(string userId, CancellationToken ct) => Write(() => _staff.SetLoginLockedAsync(userId, false, ct));

    [HttpPost("staff/{userId}/membership/activate")]
    public Task<IActionResult> ActivateMembership(string userId, CancellationToken ct) =>
        Write(() => _staff.SetMembershipActiveAsync(userId, true, ct));

    [HttpPost("staff/{userId}/membership/deactivate")]
    public Task<IActionResult> DeactivateMembership(string userId, CancellationToken ct) =>
        Write(() => _staff.SetMembershipActiveAsync(userId, false, ct));

    [HttpPost("staff/{userId}/password")]
    public Task<IActionResult> StaffPassword(string userId, PasswordBody body, CancellationToken ct) =>
        Write(() => _staff.SetPasswordAsync(userId, body.Password, ct));

    // ------------------------------------------------------------------ leads, conversations, messages, appointments

    [HttpGet("leads")]
    public async Task<IActionResult> Leads(Guid? clinicId, string? status, string? q, int page = 1, CancellationToken ct = default) =>
        Ok(await _leads.ListLeadsAsync(clinicId, status, q, page, ct));

    [HttpGet("leads/{id:guid}")]
    public async Task<IActionResult> Lead(Guid id, CancellationToken ct)
    {
        var lead = await _leads.GetLeadAsync(id, ct);
        if (lead is null) return NotFound();
        return Ok(new
        {
            lead.Id, lead.ClinicId, clinicName = lead.Clinic?.Name, lead.FullName, lead.FirstName, lead.LastName, lead.Phone,
            lead.Email, lead.Source, lead.SourceDetail, lead.CampaignName, lead.Status, lead.QualificationStatus,
            procedure = lead.Procedure?.Name, lead.PreferredLanguage, lead.CountryCode, lead.City, lead.DesiredTimeline,
            lead.Notes, lead.MarketingOptIn, lead.OptedOutAt, lead.LastContactAt, lead.NextFollowupAt, lead.CreatedAt, lead.UpdatedAt
        });
    }

    [HttpPatch("leads/{id:guid}")]
    public Task<IActionResult> UpdateLead(Guid id, LeadUpdate body, CancellationToken ct) => Write(() => _leads.UpdateLeadAsync(id, body, ct));

    [HttpGet("conversations")]
    public async Task<IActionResult> Conversations(Guid? clinicId, Guid? leadId, string? channel, string? mode, string? status,
        int page = 1, CancellationToken ct = default) =>
        Ok(await _leads.ListConversationsAsync(clinicId, leadId, channel, mode, status, page, ct));

    [HttpGet("conversations/{id:guid}")]
    public async Task<IActionResult> Conversation(Guid id, CancellationToken ct)
    {
        var c = await _leads.GetConversationAsync(id, ct);
        if (c is null) return NotFound();
        var messages = await _leads.MessagesAsync(id, 500, ct);
        return Ok(new
        {
            c.Id, c.ClinicId, c.LeadId, c.Channel, c.Status, c.Mode, c.LastMessageAt, c.ServiceWindowExpiresAt, c.CreatedAt,
            messages = messages.Select(m => new
            {
                m.Id, m.Direction, m.SenderType, m.Origin, m.MessageType, m.Content, m.DeliveryStatus, m.FailureCode,
                m.FailureReason, m.CampaignId, m.CreatedAt
            })
        });
    }

    [HttpPost("conversations/{id:guid}/mode")]
    public Task<IActionResult> ConversationMode(Guid id, ModeBody body, CancellationToken ct) => Write(() => _leads.SetModeAsync(id, body.Mode, ct));

    [HttpPost("conversations/{id:guid}/status")]
    public Task<IActionResult> ConversationStatus(Guid id, StatusBody body, CancellationToken ct) =>
        Write(() => _leads.SetStatusAsync(id, body.Status, ct));

    [HttpGet("messages")]
    public async Task<IActionResult> Messages(Guid? clinicId, bool failed = false, string? sender = null, string? q = null, int page = 1,
        CancellationToken ct = default) =>
        Ok(await _leads.ListMessagesAsync(clinicId, failed, sender, q, page, ct));

    [HttpGet("appointments")]
    public async Task<IActionResult> Appointments(Guid? clinicId, Guid? leadId, string? status, bool upcoming = false, int page = 1,
        CancellationToken ct = default) =>
        Ok(await _leads.ListAppointmentsAsync(clinicId, leadId, status, upcoming, page, ct));

    [HttpPost("appointments/{id:guid}/status")]
    public Task<IActionResult> AppointmentStatus(Guid id, StatusBody body, CancellationToken ct) =>
        Write(() => _leads.SetAppointmentStatusAsync(id, body.Status, ct));

    // ------------------------------------------------------------------ channels

    [HttpGet("channels")]
    public async Task<IActionResult> Channels(Guid? clinicId, string? channel, bool problems = false, CancellationToken ct = default) =>
        Ok(await _channels.ListChannelsAsync(clinicId, channel, problems, ct));

    [HttpPost("channels/{id:guid}/disconnect")]
    public Task<IActionResult> DisconnectChannel(Guid id, CancellationToken ct) => Write(() => _channels.DisconnectAsync(id, ct));

    [HttpGet("calendars")]
    public async Task<IActionResult> Calendars(Guid? clinicId, CancellationToken ct) => Ok(await _channels.ListCalendarsAsync(clinicId, ct));

    [HttpPost("calendars/{id:guid}/disconnect")]
    public Task<IActionResult> DisconnectCalendar(Guid id, CancellationToken ct) => Write(() => _channels.DisconnectCalendarAsync(id, ct));

    [HttpGet("tiktok")]
    public async Task<IActionResult> TikTok(Guid? clinicId, CancellationToken ct) => Ok(await _channels.ListTikTokAsync(clinicId, ct));

    [HttpPost("tiktok/{id:guid}/disconnect")]
    public Task<IActionResult> DisconnectTikTok(Guid id, CancellationToken ct) => Write(() => _channels.DisconnectTikTokAsync(id, ct));

    // ------------------------------------------------------------------ content

    [HttpGet("campaigns")]
    public async Task<IActionResult> Campaigns(Guid? clinicId, string? status, int page = 1, CancellationToken ct = default) =>
        Ok(await _content.ListCampaignsAsync(clinicId, status, page, ct));

    [HttpPost("campaigns/{id:guid}/cancel")]
    public Task<IActionResult> CancelCampaign(Guid id, CancellationToken ct) => Write(() => _content.CancelCampaignAsync(id, ct));

    [HttpGet("templates")]
    public async Task<IActionResult> Templates(Guid? clinicId, string? status, CancellationToken ct) =>
        Ok(await _content.ListTemplatesAsync(clinicId, status, ct));

    [HttpGet("procedures")]
    public async Task<IActionResult> Procedures(Guid? clinicId, CancellationToken ct) => Ok(await _content.ListProceduresAsync(clinicId, ct));

    [HttpPost("procedures/{id:guid}/active")]
    public Task<IActionResult> ProcedureActive(Guid id, ActiveBody body, CancellationToken ct) =>
        Write(() => _content.SetProcedureActiveAsync(id, body.Active, ct));

    [HttpGet("knowledge")]
    public async Task<IActionResult> Knowledge(Guid? clinicId, string? q, bool? active, int page = 1, CancellationToken ct = default) =>
        Ok(await _content.ListDocumentsAsync(clinicId, q, active, page, ct));

    [HttpPost("knowledge/{id:guid}/active")]
    public Task<IActionResult> KnowledgeActive(Guid id, ActiveBody body, CancellationToken ct) =>
        Write(() => _content.SetDocumentActiveAsync(id, body.Active, ct));

    [HttpGet("knowledge/websites")]
    public async Task<IActionResult> Websites(Guid? clinicId, CancellationToken ct) => Ok(await _content.ListWebsitesAsync(clinicId, ct));

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
