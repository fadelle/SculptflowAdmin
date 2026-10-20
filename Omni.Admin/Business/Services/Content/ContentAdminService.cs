using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Contracts.Managers;
using Omni.Admin.Business.Contracts.Services.Content;
using Omni.Admin.Entities.Dtos.Content;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Business.Services.Content;

public class ContentAdminService : IContentAdminService
{
    private readonly IContentApiClient _api;
    private readonly IAdminAudit _audit;

    public ContentAdminService(IContentApiClient api, IAdminAudit audit)
    {
        _api = api;
        _audit = audit;
    }

    public Task<PagedResult<CampaignRow>> ListCampaignsAsync(Guid? clientId, string? status, int page, CancellationToken ct = default) =>
        _api.ListCampaignsAsync(clientId, status, page, ct);

    public Task<CampaignRow?> GetCampaignAsync(Guid id, CancellationToken ct = default) => _api.GetCampaignAsync(id, ct);

    public Task<PagedResult<RecipientRow>> RecipientsAsync(Guid campaignId, string? status, int page, CancellationToken ct = default) =>
        _api.RecipientsAsync(campaignId, status, page, ct);

    public Task<List<TemplateRow>> ListTemplatesAsync(Guid? clientId, string? status, CancellationToken ct = default) =>
        _api.ListTemplatesAsync(clientId, status, ct);

    public Task<List<ServiceRow>> ListServicesAsync(Guid? clientId, CancellationToken ct = default) => _api.ListServicesAsync(clientId, ct);

    public Task<PagedResult<KnowledgeDocRow>> ListDocumentsAsync(Guid? clientId, string? search, bool? active, int page,
        CancellationToken ct = default) =>
        _api.ListDocumentsAsync(clientId, search, active, page, ct);

    public Task<KnowledgeDocDetail?> GetDocumentAsync(Guid id, CancellationToken ct = default) => _api.GetDocumentAsync(id, ct);

    public Task<SearchSettingsDetail?> SearchSettingsAsync(Guid clientId, CancellationToken ct = default) => _api.SearchSettingsAsync(clientId, ct);

    public Task<List<WebsiteSourceRow>> ListWebsitesAsync(Guid? clientId, CancellationToken ct = default) => _api.ListWebsitesAsync(clientId, ct);

    public async Task CancelCampaignAsync(Guid id, CancellationToken ct = default)
    {
        var change = await _api.CancelCampaignAsync(id, ct);
        await _audit.LogAsync("campaign.cancelled", "campaign", id, change.ClientId, ct: ct);
    }

    public async Task SetServiceActiveAsync(Guid id, bool active, CancellationToken ct = default)
    {
        var change = await _api.SetServiceActiveAsync(id, active, ct);
        await _audit.LogAsync(active ? "service.activated" : "service.deactivated", "service", id, change.ClientId, ct: ct);
    }

    public async Task SetDocumentActiveAsync(Guid id, bool active, CancellationToken ct = default)
    {
        var change = await _api.SetDocumentActiveAsync(id, active, ct);
        await _audit.LogAsync(active ? "knowledge.activated" : "knowledge.deactivated", "knowledge_document", id, change.ClientId, ct: ct);
    }

    public async Task UpdateSearchSettingsAsync(Guid clientId, int topK, double minimumSimilarity, CancellationToken ct = default)
    {
        var before = await _api.SearchSettingsAsync(clientId, ct);
        await _api.UpdateSearchSettingsAsync(clientId, topK, minimumSimilarity, ct);
        await _audit.LogAsync("knowledge.settings_updated", "knowledge_search_settings", before?.Id, clientId,
            new { before = before is null ? null : new { before.TopK, before.MinimumSimilarity }, after = new { topK, minimumSimilarity } }, ct);
    }
}
