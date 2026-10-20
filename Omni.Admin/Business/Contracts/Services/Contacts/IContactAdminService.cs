using Omni.Admin.Entities.Dtos.Contacts;
using Omni.Admin.Entities.Requests.Contacts;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Business.Contracts.Services.Contacts;

/// <summary>Contacts, conversations, messages and bookings, through the main app's contacts API; every change is audited.</summary>
public interface IContactAdminService
{
    Task<PagedResult<ContactRow>> ListContactsAsync(Guid? clientId, string? stage, string? search, int page, CancellationToken ct = default);

    Task<ContactDetail?> GetContactAsync(Guid id, CancellationToken ct = default);

    Task<List<ContactEventRow>> ContactEventsAsync(Guid contactId, CancellationToken ct = default);

    Task<PagedResult<ConversationRow>> ListConversationsAsync(Guid? clientId, Guid? contactId, string? channel, string? mode,
        string? status, int page, CancellationToken ct = default);

    Task<ConversationDetail?> GetConversationAsync(Guid id, CancellationToken ct = default);

    Task<List<MessageDetail>> MessagesAsync(Guid conversationId, int take = 500, CancellationToken ct = default);

    Task<PagedResult<MessageRow>> ListMessagesAsync(Guid? clientId, bool failedOnly, string? sender, string? search,
        int page, CancellationToken ct = default);

    Task<PagedResult<BookingRow>> ListBookingsAsync(Guid? clientId, Guid? contactId, string? status, bool upcomingOnly,
        int page, CancellationToken ct = default);

    Task UpdateContactAsync(Guid id, ContactUpdate update, CancellationToken ct = default);

    Task SetModeAsync(Guid id, string mode, CancellationToken ct = default);

    Task SetStatusAsync(Guid id, string status, CancellationToken ct = default);

    Task SetBookingStatusAsync(Guid id, string status, CancellationToken ct = default);
}
