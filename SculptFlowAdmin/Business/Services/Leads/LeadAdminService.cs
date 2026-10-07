using SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;
using SculptFlowAdmin.Business.Contracts.Managers;
using SculptFlowAdmin.Business.Contracts.Services.Leads;
using SculptFlowAdmin.Entities.Dtos.Leads;
using SculptFlowAdmin.Entities.Requests.Leads;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Entities.Responses.PlatformAdmin;

namespace SculptFlowAdmin.Business.Services.Leads;

public class LeadAdminService : ILeadAdminService
{
    private readonly ILeadsApiClient _api;
    private readonly IAdminAudit _audit;

    public LeadAdminService(ILeadsApiClient api, IAdminAudit audit)
    {
        _api = api;
        _audit = audit;
    }

    public Task<PagedResult<LeadRow>> ListLeadsAsync(Guid? clinicId, string? status, string? search, int page, CancellationToken ct = default) =>
        _api.ListLeadsAsync(clinicId, status, search, page, ct);

    public Task<LeadDetail?> GetLeadAsync(Guid id, CancellationToken ct = default) => _api.GetLeadAsync(id, ct);

    public Task<List<LeadEventRow>> LeadEventsAsync(Guid leadId, CancellationToken ct = default) => _api.LeadEventsAsync(leadId, ct);

    public Task<PagedResult<ConversationRow>> ListConversationsAsync(Guid? clinicId, Guid? leadId, string? channel, string? mode,
        string? status, int page, CancellationToken ct = default) =>
        _api.ListConversationsAsync(clinicId, leadId, channel, mode, status, page, ct);

    public Task<ConversationDetail?> GetConversationAsync(Guid id, CancellationToken ct = default) => _api.GetConversationAsync(id, ct);

    public Task<List<MessageDetail>> MessagesAsync(Guid conversationId, int take = 500, CancellationToken ct = default) =>
        _api.MessagesAsync(conversationId, take, ct);

    public Task<PagedResult<MessageRow>> ListMessagesAsync(Guid? clinicId, bool failedOnly, string? sender, string? search,
        int page, CancellationToken ct = default) =>
        _api.ListMessagesAsync(clinicId, failedOnly, sender, search, page, ct);

    public Task<PagedResult<AppointmentRow>> ListAppointmentsAsync(Guid? clinicId, Guid? leadId, string? status, bool upcomingOnly,
        int page, CancellationToken ct = default) =>
        _api.ListAppointmentsAsync(clinicId, leadId, status, upcomingOnly, page, ct);

    public async Task UpdateLeadAsync(Guid id, LeadUpdate update, CancellationToken ct = default)
    {
        var before = await _api.GetLeadAsync(id, ct) ?? throw new KeyNotFoundException();
        var change = await _api.UpdateLeadAsync(id, update, ct);
        await _audit.LogAsync("lead.updated", "lead", id, change.ClinicId,
            new { before = new { before.Status, before.QualificationStatus, before.MarketingOptIn, before.Notes }, after = update }, ct);
    }

    public async Task SetModeAsync(Guid id, string mode, CancellationToken ct = default)
    {
        var change = await _api.SetModeAsync(id, mode, ct);
        await _audit.LogAsync("conversation.mode_changed", "conversation", id, change.ClinicId, new { to = mode }, ct);
    }

    public async Task SetStatusAsync(Guid id, string status, CancellationToken ct = default)
    {
        var change = await _api.SetStatusAsync(id, status, ct);
        await _audit.LogAsync("conversation.status_changed", "conversation", id, change.ClinicId, new { to = status }, ct);
    }

    public async Task SetAppointmentStatusAsync(Guid id, string status, CancellationToken ct = default)
    {
        var change = await _api.SetAppointmentStatusAsync(id, status, ct);
        await _audit.LogAsync("appointment.status_changed", "appointment", id, change.ClinicId, new { to = status }, ct);
    }
}
