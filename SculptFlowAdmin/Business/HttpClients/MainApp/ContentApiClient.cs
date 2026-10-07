using Microsoft.Extensions.Options;
using SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;
using SculptFlowAdmin.Common.Configs;
using SculptFlowAdmin.Entities.Dtos.Content;
using SculptFlowAdmin.Entities.Requests.Content;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Entities.Responses.PlatformAdmin;

namespace SculptFlowAdmin.Business.HttpClients.MainApp;

/// <summary>Client for the main app's platform-admin content API (/api/platform-admin/content): campaigns, templates,
/// procedures and the knowledge base.</summary>
public class ContentApiClient : MainAppApiClient, IContentApiClient
{
    public const string Prefix = "api/platform-admin/content";

    public ContentApiClient(HttpClient http, IOptions<MainAppApiOptions> options, IHttpContextAccessor context, ILogger<ContentApiClient> logger)
        : base(http, options, context, logger, Prefix, "Content")
    {
    }

    public Task<PagedResult<CampaignRow>> ListCampaignsAsync(Guid? clinicId, string? status, int page, CancellationToken ct) =>
        GetRequiredAsync<PagedResult<CampaignRow>>("/campaigns" + Query(("clinicId", clinicId), ("status", status), ("page", page)), ct);

    public Task<CampaignRow?> GetCampaignAsync(Guid id, CancellationToken ct) => GetAsync<CampaignRow>($"/campaigns/{id}", ct);

    public Task<PagedResult<RecipientRow>> RecipientsAsync(Guid campaignId, string? status, int page, CancellationToken ct) =>
        GetRequiredAsync<PagedResult<RecipientRow>>($"/campaigns/{campaignId}/recipients" + Query(("status", status), ("page", page)), ct);

    public Task<PlatformAdminChange> CancelCampaignAsync(Guid id, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Post, $"/campaigns/{id}/cancel", null, null, false, ct);

    public Task<List<TemplateRow>> ListTemplatesAsync(Guid? clinicId, string? status, CancellationToken ct) =>
        GetRequiredAsync<List<TemplateRow>>("/templates" + Query(("clinicId", clinicId), ("status", status)), ct);

    public Task<List<ProcedureRow>> ListProceduresAsync(Guid? clinicId, CancellationToken ct) =>
        GetRequiredAsync<List<ProcedureRow>>("/procedures" + Query(("clinicId", clinicId)), ct);

    public Task<PlatformAdminChange> SetProcedureActiveAsync(Guid id, bool active, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/procedures/{id}/active", new ActiveBody(active), null, false, ct);

    public Task<PagedResult<KnowledgeDocRow>> ListDocumentsAsync(Guid? clinicId, string? search, bool? active, int page, CancellationToken ct) =>
        GetRequiredAsync<PagedResult<KnowledgeDocRow>>("/knowledge" + Query(("clinicId", clinicId), ("search", search),
            ("active", active?.ToString().ToLowerInvariant()), ("page", page)), ct);

    public Task<KnowledgeDocDetail?> GetDocumentAsync(Guid id, CancellationToken ct) => GetAsync<KnowledgeDocDetail>($"/knowledge/{id}", ct);

    public Task<PlatformAdminChange> SetDocumentActiveAsync(Guid id, bool active, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/knowledge/{id}/active", new ActiveBody(active), null, false, ct);

    public Task<List<WebsiteSourceRow>> ListWebsitesAsync(Guid? clinicId, CancellationToken ct) =>
        GetRequiredAsync<List<WebsiteSourceRow>>("/knowledge/websites" + Query(("clinicId", clinicId)), ct);

    public Task<SearchSettingsDetail?> SearchSettingsAsync(Guid clinicId, CancellationToken ct) =>
        GetAsync<SearchSettingsDetail>($"/knowledge/settings/{clinicId}", ct);

    public Task<PlatformAdminChange> UpdateSearchSettingsAsync(Guid clinicId, int topK, double minimumSimilarity, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/knowledge/settings/{clinicId}", new SearchSettingsBody(topK, minimumSimilarity),
            null, false, ct);
}
