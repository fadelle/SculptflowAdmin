using Omni.Admin.Entities.Dtos.Contacts;
using Omni.Admin.Entities.Requests.Contacts;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Business.Contracts.HttpClients.MainApp;

public interface IContactsApiClient
{
    Task<PagedResult<ContactRow>> ListContactsAsync(Guid? clientId, string? stage, string? search, int page, CancellationToken ct);

    Task<ContactDetail?> GetContactAsync(Guid id, CancellationToken ct);

    Task<List<ContactEventRow>> ContactEventsAsync(Guid id, CancellationToken ct);

    Task<PlatformAdminChange> UpdateContactAsync(Guid id, ContactUpdate update, CancellationToken ct);

    /// <summary>Everything the main app holds about the contact, as indented JSON (passed through, not copied); null when unknown.</summary>
    Task<string?> ExportContactAsync(Guid id, CancellationToken ct);

    /// <summary>Deletes the contact and all its data in the main app, for good.</summary>
    Task<PlatformAdminChange> DeleteContactAsync(Guid id, CancellationToken ct);

    Task<PagedResult<ConversationRow>> ListConversationsAsync(Guid? clientId, Guid? contactId, string? channel, string? mode, string? status,
        int page, CancellationToken ct);

    Task<ConversationDetail?> GetConversationAsync(Guid id, CancellationToken ct);

    Task<List<MessageDetail>> MessagesAsync(Guid conversationId, int take, CancellationToken ct);

    Task<PlatformAdminChange> SetModeAsync(Guid id, string mode, CancellationToken ct);

    Task<PlatformAdminChange> SetStatusAsync(Guid id, string status, CancellationToken ct);

    Task<PagedResult<MessageRow>> ListMessagesAsync(Guid? clientId, bool failedOnly, string? sender, string? search, int page,
        CancellationToken ct);

    Task<PagedResult<BookingRow>> ListBookingsAsync(Guid? clientId, Guid? contactId, string? status, bool upcomingOnly, int page,
        CancellationToken ct);

    Task<PlatformAdminChange> SetBookingStatusAsync(Guid id, string status, CancellationToken ct);
}
