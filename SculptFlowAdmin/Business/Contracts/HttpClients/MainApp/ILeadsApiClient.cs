using SculptFlowAdmin.Entities.Dtos.Leads;
using SculptFlowAdmin.Entities.Requests.Leads;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Entities.Responses.PlatformAdmin;

namespace SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;

public interface ILeadsApiClient
{
    Task<PagedResult<LeadRow>> ListLeadsAsync(Guid? clinicId, string? status, string? search, int page, CancellationToken ct);

    Task<LeadDetail?> GetLeadAsync(Guid id, CancellationToken ct);

    Task<List<LeadEventRow>> LeadEventsAsync(Guid id, CancellationToken ct);

    Task<PlatformAdminChange> UpdateLeadAsync(Guid id, LeadUpdate update, CancellationToken ct);

    Task<PagedResult<ConversationRow>> ListConversationsAsync(Guid? clinicId, Guid? leadId, string? channel, string? mode, string? status,
        int page, CancellationToken ct);

    Task<ConversationDetail?> GetConversationAsync(Guid id, CancellationToken ct);

    Task<List<MessageDetail>> MessagesAsync(Guid conversationId, int take, CancellationToken ct);

    Task<PlatformAdminChange> SetModeAsync(Guid id, string mode, CancellationToken ct);

    Task<PlatformAdminChange> SetStatusAsync(Guid id, string status, CancellationToken ct);

    Task<PagedResult<MessageRow>> ListMessagesAsync(Guid? clinicId, bool failedOnly, string? sender, string? search, int page,
        CancellationToken ct);

    Task<PagedResult<AppointmentRow>> ListAppointmentsAsync(Guid? clinicId, Guid? leadId, string? status, bool upcomingOnly, int page,
        CancellationToken ct);

    Task<PlatformAdminChange> SetAppointmentStatusAsync(Guid id, string status, CancellationToken ct);
}
