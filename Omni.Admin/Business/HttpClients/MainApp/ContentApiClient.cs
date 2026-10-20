using Microsoft.Extensions.Options;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Common.Configs;
using Omni.Admin.Entities.Dtos.Content;
using Omni.Admin.Entities.Requests.Content;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Business.HttpClients.MainApp;

/// <summary>Client for the main app's platform-admin content API (/api/platform-admin/content): campaigns, templates,
/// services and the knowledge base.</summary>
public class ContentApiClient : MainAppApiClient, IContentApiClient
{
    public const string Prefix = "api/platform-admin/content";

    public ContentApiClient(HttpClient http, IOptions<MainAppApiOptions> options, IHttpContextAccessor context, ILogger<ContentApiClient> logger)
        : base(http, options, context, logger, Prefix, "Content")
    {
    }

    public Task<PagedResult<CampaignRow>> ListCampaignsAsync(Guid? clientId, string? status, int page, CancellationToken ct) =>
        GetRequiredAsync<PagedResult<CampaignRow>>("/campaigns" + Query(("clientId", clientId), ("status", status), ("page", page)), ct);

    public Task<CampaignRow?> GetCampaignAsync(Guid id, CancellationToken ct) => GetAsync<CampaignRow>($"/campaigns/{id}", ct);

    public Task<PagedResult<RecipientRow>> RecipientsAsync(Guid campaignId, string? status, int page, CancellationToken ct) =>
        GetRequiredAsync<PagedResult<RecipientRow>>($"/campaigns/{campaignId}/recipients" + Query(("status", status), ("page", page)), ct);

    public Task<PlatformAdminChange> CancelCampaignAsync(Guid id, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Post, $"/campaigns/{id}/cancel", null, null, false, ct);

    public Task<List<TemplateRow>> ListTemplatesAsync(Guid? clientId, string? status, CancellationToken ct) =>
        GetRequiredAsync<List<TemplateRow>>("/templates" + Query(("clientId", clientId), ("status", status)), ct);

    public Task<List<ServiceRow>> ListServicesAsync(Guid? clientId, CancellationToken ct) =>
        GetRequiredAsync<List<ServiceRow>>("/services" + Query(("clientId", clientId)), ct);

    public Task<PlatformAdminChange> SetServiceActiveAsync(Guid id, bool active, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/services/{id}/active", new ActiveBody(active), null, false, ct);

    public Task<PagedResult<KnowledgeDocRow>> ListDocumentsAsync(Guid? clientId, string? search, bool? active, int page, CancellationToken ct) =>
        GetRequiredAsync<PagedResult<KnowledgeDocRow>>("/knowledge" + Query(("clientId", clientId), ("search", search),
            ("active", active?.ToString().ToLowerInvariant()), ("page", page)), ct);

    public Task<KnowledgeDocDetail?> GetDocumentAsync(Guid id, CancellationToken ct) => GetAsync<KnowledgeDocDetail>($"/knowledge/{id}", ct);

    public Task<PlatformAdminChange> SetDocumentActiveAsync(Guid id, bool active, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/knowledge/{id}/active", new ActiveBody(active), null, false, ct);

    public Task<List<WebsiteSourceRow>> ListWebsitesAsync(Guid? clientId, CancellationToken ct) =>
        GetRequiredAsync<List<WebsiteSourceRow>>("/knowledge/websites" + Query(("clientId", clientId)), ct);

    public Task<SearchSettingsDetail?> SearchSettingsAsync(Guid clientId, CancellationToken ct) =>
        GetAsync<SearchSettingsDetail>($"/knowledge/settings/{clientId}", ct);

    public Task<PlatformAdminChange> UpdateSearchSettingsAsync(Guid clientId, int topK, double minimumSimilarity, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/knowledge/settings/{clientId}", new SearchSettingsBody(topK, minimumSimilarity),
            null, false, ct);
}
