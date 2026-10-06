using SculptFlowAdmin.Entities.Dtos.Leads;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Entities.Responses.Paging;

namespace SculptFlowAdmin.Persistence.Contracts.Leads;

public interface ILeadAdminRepository
{
    Task<PagedResult<LeadRow>> ListLeadsAsync(Guid? clinicId, string? status, string? search, int page, CancellationToken ct = default);

    Task<Lead?> GetLeadAsync(Guid id, CancellationToken ct = default);

    Task<List<EventLog>> LeadEventsAsync(Guid leadId, CancellationToken ct = default);

    Task<PagedResult<ConversationRow>> ListConversationsAsync(Guid? clinicId, Guid? leadId, string? channel, string? mode,
        string? status, int page, CancellationToken ct = default);

    Task<Conversation?> GetConversationAsync(Guid id, CancellationToken ct = default);

    Task<List<Message>> MessagesAsync(Guid conversationId, int take = 500, CancellationToken ct = default);

    Task<PagedResult<MessageRow>> ListMessagesAsync(Guid? clinicId, bool failedOnly, string? sender, string? search,
        int page, CancellationToken ct = default);

    Task<PagedResult<AppointmentRow>> ListAppointmentsAsync(Guid? clinicId, Guid? leadId, string? status, bool upcomingOnly,
        int page, CancellationToken ct = default);

    /// <summary>Tracked.</summary>
    Task<Lead?> GetLeadForUpdateAsync(Guid id, CancellationToken ct = default);

    /// <summary>Tracked.</summary>
    Task<Conversation?> GetConversationForUpdateAsync(Guid id, CancellationToken ct = default);

    /// <summary>Tracked.</summary>
    Task<Appointment?> GetAppointmentForUpdateAsync(Guid id, CancellationToken ct = default);
}
