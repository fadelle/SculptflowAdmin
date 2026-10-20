using Microsoft.Extensions.Options;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Common.Configs;
using Omni.Admin.Entities.Dtos.BusinessTypes;
using Omni.Admin.Entities.Dtos.Clients;
using Omni.Admin.Entities.Requests.BusinessTypes;
using Omni.Admin.Entities.Requests.Clients;
using Omni.Admin.Entities.Requests.Content;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Business.HttpClients.MainApp;

/// <summary>Client for the main app's platform-admin clients API (/api/platform-admin/clients).</summary>
public class ClientsApiClient : MainAppApiClient, IClientsApiClient
{
    public const string Prefix = "api/platform-admin/clients";

    public ClientsApiClient(HttpClient http, IOptions<MainAppApiOptions> options, IHttpContextAccessor context, ILogger<ClientsApiClient> logger)
        : base(http, options, context, logger, Prefix, "Clients")
    {
    }

    public Task<PagedResult<ClientRow>> ListAsync(string? search, bool? active, int page, CancellationToken ct) =>
        GetRequiredAsync<PagedResult<ClientRow>>(Query(("search", search), ("active", active?.ToString().ToLowerInvariant()), ("page", page)), ct);

    public Task<List<ClientOption>> OptionsAsync(CancellationToken ct) => GetRequiredAsync<List<ClientOption>>("/options", ct);

    public Task<ClientDetail?> GetAsync(Guid id, CancellationToken ct) => GetAsync<ClientDetail>($"/{id}", ct);

    public Task<ClientCounts> CountsAsync(Guid id, CancellationToken ct) => GetRequiredAsync<ClientCounts>($"/{id}/counts", ct);

    public Task<PlatformAdminChange> UpdateAsync(Guid id, ClientUpdate update, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/{id}", update, null, false, ct);

    public Task<PlatformAdminChange> SetActiveAsync(Guid id, bool active, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/{id}/active", new ActiveBody(active), null, false, ct);

    public Task<InboxSettingsDto?> GetInboxSettingsAsync(Guid id, CancellationToken ct) => GetAsync<InboxSettingsDto>($"/{id}/inbox-settings", ct);

    public Task<PlatformAdminChange> UpdateInboxSettingsAsync(Guid id, UpdateInboxSettingsRequest request, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/{id}/inbox-settings", request, null, false, ct);

    public Task<BusinessTypeChangePreview> PreviewBusinessTypeAsync(Guid id, string toCode, CancellationToken ct) =>
        GetRequiredAsync<BusinessTypeChangePreview>($"/{id}/business-type/preview{Query(("to", toCode))}", ct);

    public Task<PlatformAdminChange> ChangeBusinessTypeAsync(Guid id, string toCode, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/{id}/business-type", new ChangeBusinessTypeRequest(toCode), null, false, ct);
}
