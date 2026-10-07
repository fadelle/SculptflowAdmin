using SculptFlowAdmin.Entities.Dtos.Leads;
using SculptFlowAdmin.Entities.Requests.Leads;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Entities.Responses.PlatformAdmin;

namespace SculptFlowAdmin.Business.Contracts.Services.Leads;

/// <summary>Leads, conversations, messages and appointments, through the main app's leads API; every change is audited.</summary>
public interface ILeadAdminService
{
    Task<PagedResult<LeadRow>> ListLeadsAsync(Guid? clinicId, string? status, string? search, int page, CancellationToken ct = default);

    Task<LeadDetail?> GetLeadAsync(Guid id, CancellationToken ct = default);

    Task<List<LeadEventRow>> LeadEventsAsync(Guid leadId, CancellationToken ct = default);

    Task<PagedResult<ConversationRow>> ListConversationsAsync(Guid? clinicId, Guid? leadId, string? channel, string? mode,
        string? status, int page, CancellationToken ct = default);

    Task<ConversationDetail?> GetConversationAsync(Guid id, CancellationToken ct = default);

    Task<List<MessageDetail>> MessagesAsync(Guid conversationId, int take = 500, CancellationToken ct = default);

    Task<PagedResult<MessageRow>> ListMessagesAsync(Guid? clinicId, bool failedOnly, string? sender, string? search,
        int page, CancellationToken ct = default);

    Task<PagedResult<AppointmentRow>> ListAppointmentsAsync(Guid? clinicId, Guid? leadId, string? status, bool upcomingOnly,
        int page, CancellationToken ct = default);

    Task UpdateLeadAsync(Guid id, LeadUpdate update, CancellationToken ct = default);

    Task SetModeAsync(Guid id, string mode, CancellationToken ct = default);

    Task SetStatusAsync(Guid id, string status, CancellationToken ct = default);

    Task SetAppointmentStatusAsync(Guid id, string status, CancellationToken ct = default);
}
