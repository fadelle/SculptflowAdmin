using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Contracts.Managers;
using Omni.Admin.Business.Contracts.Services.Clients;
using Omni.Admin.Entities.Dtos.BusinessTypes;
using Omni.Admin.Entities.Dtos.Clients;
using Omni.Admin.Entities.Requests.Clients;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Business.Services.Clients;

public class ClientAdminService : IClientAdminService
{
    private readonly IClientsApiClient _api;
    private readonly IAdminAudit _audit;

    public ClientAdminService(IClientsApiClient api, IAdminAudit audit)
    {
        _api = api;
        _audit = audit;
    }

    public Task<PagedResult<ClientRow>> ListAsync(string? search, bool? active, int page, CancellationToken ct = default) =>
        _api.ListAsync(search, active, page, ct);

    public Task<List<ClientOption>> OptionsAsync(CancellationToken ct = default) => _api.OptionsAsync(ct);

    public Task<ClientDetail?> GetAsync(Guid id, CancellationToken ct = default) => _api.GetAsync(id, ct);

    public Task<ClientCounts> CountsAsync(Guid id, CancellationToken ct = default) => _api.CountsAsync(id, ct);

    public async Task UpdateAsync(Guid id, ClientUpdate update, CancellationToken ct = default)
    {
        var before = await _api.GetAsync(id, ct) ?? throw new KeyNotFoundException();
        await _api.UpdateAsync(id, update, ct);
        await _audit.LogAsync("client.updated", "client", id, id,
            new { before = new { before.Name, before.Phone, before.Email, before.Website, before.CountryCode, before.Timezone }, after = update }, ct);
    }

    public async Task SetActiveAsync(Guid id, bool active, CancellationToken ct = default)
    {
        await _api.SetActiveAsync(id, active, ct);
        await _audit.LogAsync(active ? "client.activated" : "client.deactivated", "client", id, id, ct: ct);
    }

    public Task<InboxSettingsDto?> GetInboxSettingsAsync(Guid id, CancellationToken ct = default) => _api.GetInboxSettingsAsync(id, ct);

    public async Task UpdateInboxSettingsAsync(Guid id, UpdateInboxSettingsRequest request, CancellationToken ct = default)
    {
        var before = await _api.GetInboxSettingsAsync(id, ct) ?? throw new KeyNotFoundException();
        var change = await _api.UpdateInboxSettingsAsync(id, request, ct);
        await _audit.LogAsync("client.inbox_settings_updated", "client", id, change.ClientId,
            new { before = new { before.AiBotNewConversations, before.AiBotReopenedConversations }, after = request }, ct);
    }

    public Task<BusinessTypeChangePreview> PreviewBusinessTypeAsync(Guid id, string toCode, CancellationToken ct = default) =>
        _api.PreviewBusinessTypeAsync(id, toCode, ct);

    public async Task ChangeBusinessTypeAsync(Guid id, string toCode, CancellationToken ct = default)
    {
        var before = await _api.GetAsync(id, ct) ?? throw new KeyNotFoundException();
        var change = await _api.ChangeBusinessTypeAsync(id, toCode, ct);
        await _audit.LogAsync("client.business_type_changed", "client", id, change.ClientId,
            new { from = before.BusinessTypeCode, to = toCode }, ct);
    }
}
