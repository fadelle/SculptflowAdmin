using SculptFlowAdmin.Entities.Dtos.Leads;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Entities.Requests.Leads;
using SculptFlowAdmin.Entities.Responses.Paging;

namespace SculptFlowAdmin.Business.Contracts.Services.Leads;

public interface ILeadAdminService
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

    Task UpdateLeadAsync(Guid id, LeadUpdate update, CancellationToken ct = default);

    /// <summary>Same rules as the main app's Take Over / Return to AI (ConversationModeSync adds the timeline marker).
    /// The main app's open Inbox tabs pick it up on their next refresh (no SignalR from here).</summary>
    Task SetModeAsync(Guid id, string mode, CancellationToken ct = default);

    Task SetStatusAsync(Guid id, string status, CancellationToken ct = default);

    Task SetAppointmentStatusAsync(Guid id, string status, CancellationToken ct = default);
}
