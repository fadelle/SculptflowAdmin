using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Contracts.Managers;
using Omni.Admin.Business.Contracts.Services.Contacts;
using Omni.Admin.Entities.Dtos.Contacts;
using Omni.Admin.Entities.Requests.Contacts;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Business.Services.Contacts;

public class ContactAdminService : IContactAdminService
{
    private readonly IContactsApiClient _api;
    private readonly IAdminAudit _audit;

    public ContactAdminService(IContactsApiClient api, IAdminAudit audit)
    {
        _api = api;
        _audit = audit;
    }

    public Task<PagedResult<ContactRow>> ListContactsAsync(Guid? clientId, string? stage, string? search, int page, CancellationToken ct = default) =>
        _api.ListContactsAsync(clientId, stage, search, page, ct);

    public Task<ContactDetail?> GetContactAsync(Guid id, CancellationToken ct = default) => _api.GetContactAsync(id, ct);

    public Task<List<ContactEventRow>> ContactEventsAsync(Guid contactId, CancellationToken ct = default) => _api.ContactEventsAsync(contactId, ct);

    public Task<PagedResult<ConversationRow>> ListConversationsAsync(Guid? clientId, Guid? contactId, string? channel, string? mode,
        string? status, int page, CancellationToken ct = default) =>
        _api.ListConversationsAsync(clientId, contactId, channel, mode, status, page, ct);

    public Task<ConversationDetail?> GetConversationAsync(Guid id, CancellationToken ct = default) => _api.GetConversationAsync(id, ct);

    public Task<List<MessageDetail>> MessagesAsync(Guid conversationId, int take = 500, CancellationToken ct = default) =>
        _api.MessagesAsync(conversationId, take, ct);

    public Task<PagedResult<MessageRow>> ListMessagesAsync(Guid? clientId, bool failedOnly, string? sender, string? search,
        int page, CancellationToken ct = default) =>
        _api.ListMessagesAsync(clientId, failedOnly, sender, search, page, ct);

    public Task<PagedResult<BookingRow>> ListBookingsAsync(Guid? clientId, Guid? contactId, string? status, bool upcomingOnly,
        int page, CancellationToken ct = default) =>
        _api.ListBookingsAsync(clientId, contactId, status, upcomingOnly, page, ct);

    public async Task UpdateContactAsync(Guid id, ContactUpdate update, CancellationToken ct = default)
    {
        var before = await _api.GetContactAsync(id, ct) ?? throw new KeyNotFoundException();
        var change = await _api.UpdateContactAsync(id, update, ct);
        await _audit.LogAsync("contact.updated", "contact", id, change.ClientId,
            new { before = new { before.StageKey, before.Qualification, before.MarketingOptIn, before.Notes }, after = update }, ct);
    }

    public async Task SetModeAsync(Guid id, string mode, CancellationToken ct = default)
    {
        var change = await _api.SetModeAsync(id, mode, ct);
        await _audit.LogAsync("conversation.mode_changed", "conversation", id, change.ClientId, new { to = mode }, ct);
    }

    public async Task SetStatusAsync(Guid id, string status, CancellationToken ct = default)
    {
        var change = await _api.SetStatusAsync(id, status, ct);
        await _audit.LogAsync("conversation.status_changed", "conversation", id, change.ClientId, new { to = status }, ct);
    }

    public async Task SetBookingStatusAsync(Guid id, string status, CancellationToken ct = default)
    {
        var change = await _api.SetBookingStatusAsync(id, status, ct);
        await _audit.LogAsync("booking.status_changed", "booking", id, change.ClientId, new { to = status }, ct);
    }
}
