using Microsoft.Extensions.Options;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Common.Configs;
using Omni.Admin.Entities.Dtos.Contacts;
using Omni.Admin.Entities.Requests.Contacts;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Business.HttpClients.MainApp;

/// <summary>Client for the main app's platform-admin contacts API (/api/platform-admin/contacts): contacts, conversations,
/// messages and bookings.</summary>
public class ContactsApiClient : MainAppApiClient, IContactsApiClient
{
    public const string Prefix = "api/platform-admin/contacts";

    public ContactsApiClient(HttpClient http, IOptions<MainAppApiOptions> options, IHttpContextAccessor context, ILogger<ContactsApiClient> logger)
        : base(http, options, context, logger, Prefix, "Contacts")
    {
    }

    public Task<PagedResult<ContactRow>> ListContactsAsync(Guid? clientId, string? stage, string? search, int page, CancellationToken ct) =>
        GetRequiredAsync<PagedResult<ContactRow>>(Query(("clientId", clientId), ("stage", stage), ("search", search), ("page", page)), ct);

    public Task<ContactDetail?> GetContactAsync(Guid id, CancellationToken ct) => GetAsync<ContactDetail>($"/{id}", ct);

    public Task<List<ContactEventRow>> ContactEventsAsync(Guid id, CancellationToken ct) => GetRequiredAsync<List<ContactEventRow>>($"/{id}/events", ct);

    public Task<PlatformAdminChange> UpdateContactAsync(Guid id, ContactUpdate update, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/{id}", update, null, false, ct);

    public Task<PagedResult<ConversationRow>> ListConversationsAsync(Guid? clientId, Guid? contactId, string? channel, string? mode,
        string? status, int page, CancellationToken ct) =>
        GetRequiredAsync<PagedResult<ConversationRow>>("/conversations" + Query(("clientId", clientId), ("contactId", contactId),
            ("channel", channel), ("mode", mode), ("status", status), ("page", page)), ct);

    public Task<ConversationDetail?> GetConversationAsync(Guid id, CancellationToken ct) => GetAsync<ConversationDetail>($"/conversations/{id}", ct);

    public Task<List<MessageDetail>> MessagesAsync(Guid conversationId, int take, CancellationToken ct) =>
        GetRequiredAsync<List<MessageDetail>>($"/conversations/{conversationId}/messages" + Query(("take", take)), ct);

    public Task<PlatformAdminChange> SetModeAsync(Guid id, string mode, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/conversations/{id}/mode", new ModeBody(mode), null, false, ct);

    public Task<PlatformAdminChange> SetStatusAsync(Guid id, string status, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/conversations/{id}/status", new StatusBody(status), null, false, ct);

    public Task<PagedResult<MessageRow>> ListMessagesAsync(Guid? clientId, bool failedOnly, string? sender, string? search, int page,
        CancellationToken ct) =>
        GetRequiredAsync<PagedResult<MessageRow>>("/messages" + Query(("clientId", clientId), ("failedOnly", failedOnly),
            ("sender", sender), ("search", search), ("page", page)), ct);

    public Task<PagedResult<BookingRow>> ListBookingsAsync(Guid? clientId, Guid? contactId, string? status, bool upcomingOnly,
        int page, CancellationToken ct) =>
        GetRequiredAsync<PagedResult<BookingRow>>("/bookings" + Query(("clientId", clientId), ("contactId", contactId),
            ("status", status), ("upcomingOnly", upcomingOnly), ("page", page)), ct);

    public Task<PlatformAdminChange> SetBookingStatusAsync(Guid id, string status, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/bookings/{id}/status", new StatusBody(status), null, false, ct);
}
