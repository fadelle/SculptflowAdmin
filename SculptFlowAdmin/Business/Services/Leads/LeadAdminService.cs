using SculptFlowAdmin.Business.Contracts.Managers;
using SculptFlowAdmin.Business.Contracts.Services.Leads;
using SculptFlowAdmin.Common.Enums;
using SculptFlowAdmin.Common.Exceptions;
using SculptFlowAdmin.Common.Helpers;
using SculptFlowAdmin.Entities.Dtos.Leads;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Entities.Requests.Leads;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Persistence.Contracts;
using SculptFlowAdmin.Persistence.Contracts.Leads;

namespace SculptFlowAdmin.Business.Services.Leads;

/// <summary>Leads, conversations, messages and appointments across every clinic.</summary>
public class LeadAdminService : ILeadAdminService
{
    private readonly ILeadAdminRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAdminAudit _audit;

    public LeadAdminService(ILeadAdminRepository repository, IUnitOfWork unitOfWork, IAdminAudit audit)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public Task<PagedResult<LeadRow>> ListLeadsAsync(Guid? clinicId, string? status, string? search, int page, CancellationToken ct = default) =>
        _repository.ListLeadsAsync(clinicId, status, search, page, ct);

    public Task<Lead?> GetLeadAsync(Guid id, CancellationToken ct = default) =>
        _repository.GetLeadAsync(id, ct);

    public Task<List<EventLog>> LeadEventsAsync(Guid leadId, CancellationToken ct = default) =>
        _repository.LeadEventsAsync(leadId, ct);

    public Task<PagedResult<ConversationRow>> ListConversationsAsync(Guid? clinicId, Guid? leadId, string? channel, string? mode,
        string? status, int page, CancellationToken ct = default) =>
        _repository.ListConversationsAsync(clinicId, leadId, channel, mode, status, page, ct);

    public Task<Conversation?> GetConversationAsync(Guid id, CancellationToken ct = default) =>
        _repository.GetConversationAsync(id, ct);

    public Task<List<Message>> MessagesAsync(Guid conversationId, int take = 500, CancellationToken ct = default) =>
        _repository.MessagesAsync(conversationId, take, ct);

    public Task<PagedResult<MessageRow>> ListMessagesAsync(Guid? clinicId, bool failedOnly, string? sender, string? search,
        int page, CancellationToken ct = default) =>
        _repository.ListMessagesAsync(clinicId, failedOnly, sender, search, page, ct);

    public Task<PagedResult<AppointmentRow>> ListAppointmentsAsync(Guid? clinicId, Guid? leadId, string? status, bool upcomingOnly,
        int page, CancellationToken ct = default) =>
        _repository.ListAppointmentsAsync(clinicId, leadId, status, upcomingOnly, page, ct);

    // ------------------------------------------------------------------ leads

    public async Task UpdateLeadAsync(Guid id, LeadUpdate update, CancellationToken ct = default)
    {
        if (!LeadStatus.All.Contains(update.Status)) throw new AdminRuleException("Unknown lead status.");
        if (!LeadQualificationStatus.All.Contains(update.QualificationStatus)) throw new AdminRuleException("Unknown qualification.");
        var lead = await _repository.GetLeadForUpdateAsync(id, ct) ?? throw new KeyNotFoundException();
        var before = new { lead.Status, lead.QualificationStatus, lead.MarketingOptIn, lead.Notes };
        var now = DateTimeOffset.UtcNow;
        lead.Status = update.Status;
        lead.QualificationStatus = update.QualificationStatus;
        if (lead.MarketingOptIn && !update.MarketingOptIn) lead.OptedOutAt = now;
        if (!lead.MarketingOptIn && update.MarketingOptIn) lead.OptedOutAt = null;
        lead.MarketingOptIn = update.MarketingOptIn;
        lead.Notes = InputText.Clean(update.Notes);
        lead.UpdatedAt = now;
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.LogAsync("lead.updated", "lead", id, lead.ClinicId, new { before, after = update }, ct);
    }

    // ------------------------------------------------------------------ conversations

    /// <summary>Same rules as the main app's Take Over / Return to AI (ConversationModeSync adds the timeline marker).
    /// The main app's open Inbox tabs pick it up on their next refresh (no SignalR from here).</summary>
    public async Task SetModeAsync(Guid id, string mode, CancellationToken ct = default)
    {
        if (mode != ConversationMode.Ai && mode != ConversationMode.Human) throw new AdminRuleException("Mode must be ai or human.");
        var c = await _repository.GetConversationForUpdateAsync(id, ct) ?? throw new KeyNotFoundException();
        if (c.Mode == mode) return;
        var previous = c.Mode;
        var now = DateTimeOffset.UtcNow;
        ConversationModeSync.Apply(c, mode, now);
        c.UpdatedAt = now;
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.LogAsync("conversation.mode_changed", "conversation", id, c.ClinicId, new { from = previous, to = mode }, ct);
    }

    public async Task SetStatusAsync(Guid id, string status, CancellationToken ct = default)
    {
        if (status is not (ConversationStatus.Active or ConversationStatus.Closed or ConversationStatus.Archived))
            throw new AdminRuleException("Unknown conversation status.");
        var c = await _repository.GetConversationForUpdateAsync(id, ct) ?? throw new KeyNotFoundException();
        if (c.Status == status) return;
        var previous = c.Status;
        c.Status = status;
        c.UpdatedAt = DateTimeOffset.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.LogAsync("conversation.status_changed", "conversation", id, c.ClinicId, new { from = previous, to = status }, ct);
    }

    // ------------------------------------------------------------------ messages

    // ------------------------------------------------------------------ appointments

    public async Task SetAppointmentStatusAsync(Guid id, string status, CancellationToken ct = default)
    {
        if (!AppointmentStatus.All.Contains(status)) throw new AdminRuleException("Unknown appointment status.");
        var a = await _repository.GetAppointmentForUpdateAsync(id, ct) ?? throw new KeyNotFoundException();
        if (a.Status == status) return;
        var previous = a.Status;
        a.Status = status;
        a.UpdatedAt = DateTimeOffset.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.LogAsync("appointment.status_changed", "appointment", id, a.ClinicId, new { from = previous, to = status }, ct);
    }
}
